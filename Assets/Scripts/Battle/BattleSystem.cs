using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState
{
    BattleStart, TurnPerformance, ActionSelection, SkillSelection, PartySelection, BagSelection, ChoiceSelection
        , ForgetSelection, BattleEnd, Busy
}
public enum BattleAction { Fight, Party, Bag, Run }

public class BattleSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;

    [SerializeField] BagUI bagUI;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] ForgetSelector forgetSelector;
    [SerializeField] SkillDetails skillDetails;
    [SerializeField] BattleSelector selector;

    [SerializeField] GameObject trapSprite;

    [Header("Audio")]
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip challengerBattleMusic;
    [SerializeField] AudioClip victoryMusic;

    private BeastParty playerParty;
    private BeastParty challengerParty;
    private Beast wildBeast;

    private PlayerController player;
    private ChallengerController challenger;

    private SkillBase skillToLearn;

    private BattleState state;
    private int currentAction;
    private int currentSkill;
    private int escapeAttempts;
    private bool choice = true;
    private bool isChallenge = false;
    private bool isOpenDetails = false;

    public event Action<bool> OnBattleWin;

    //Battle Start
    public void StartBattle(BeastParty beastParty, Beast wildBeast)
    {
        state = BattleState.BattleStart;

        isChallenge = false;

        this.playerParty = beastParty;
        this.wildBeast = wildBeast;
        player = playerParty.GetComponent<PlayerController>();

        AudioManager.Instance.PlayMusic(wildBattleMusic);

        StartCoroutine(SetupBattle());
    }

    public void StartChallenge(BeastParty playerParty, BeastParty challengerParty)
    {
        state = BattleState.BattleStart;

        isChallenge = true;

        this.playerParty = playerParty;
        this.challengerParty = challengerParty;
        player = playerParty.GetComponent<PlayerController>();
        challenger = challengerParty.GetComponent<ChallengerController>();

        AudioManager.Instance.PlayMusic(challengerBattleMusic);

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.ClearUnit();
        enemyUnit.ClearUnit();

        if (!isChallenge)
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            yield return dialogBox.TypeDialog("Something is coming...");

            enemyUnit.gameObject.SetActive(true);
            enemyUnit.SetUnit(wildBeast);
            yield return dialogBox.TypeDialog
                (player.name + " encountered a wild " + enemyUnit.Beast.BeastBase.Name + "!");

            playerUnit.gameObject.SetActive(true);
            playerUnit.SetUnit(playerParty.GetHealthyBeast());
            yield return dialogBox.TypeDialog(player.name + " sent out " + playerUnit.Beast.BeastBase.Name + "!");
        }
        else
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            yield return dialogBox.TypeDialog(challenger.name + " challenged " + player.name + " for a battle!");

            enemyUnit.gameObject.SetActive(true);
            var enemyBeast = challengerParty.GetHealthyBeast();
            enemyUnit.SetUnit(enemyBeast);
            yield return dialogBox.TypeDialog(challenger.name + " sent out " + enemyBeast.BeastBase.Name + "!");

            playerUnit.gameObject.SetActive(true);
            var playerBeast = playerParty.GetHealthyBeast();
            playerUnit.SetUnit(playerBeast);
            yield return dialogBox.TypeDialog(player.name + " sent out " + playerBeast.BeastBase.Name + "!");
        }

        escapeAttempts = 0;

        selector.SetSkill(playerUnit.Beast.Skills);
        partyScreen.SetParty();

        SelectAction();
    }

    //Turn Performance
    IEnumerator PerformTurn(BattleAction playerAction)
    {
        state = BattleState.TurnPerformance;

        if (playerAction == BattleAction.Fight)
        {
            playerUnit.Beast.CurrentSkill = playerUnit.Beast.Skills[currentSkill];
            enemyUnit.Beast.CurrentSkill = enemyUnit.Beast.GetRandomSkill();

            int playerPriority = playerUnit.Beast.CurrentSkill.SkillBase.Priority;
            int enemyPriority = enemyUnit.Beast.CurrentSkill.SkillBase.Priority;
            bool playerFirst = true;
            if (enemyPriority > playerPriority) { playerFirst = false; }
            else if (enemyPriority == playerPriority)
            {
                float playerShocked = (playerUnit.Beast.Status == StatusEffectDB.StatusEffects[StatusEffectID.SHK]) ? 0.75f : 1f;
                float enemyShocked = (enemyUnit.Beast.Status == StatusEffectDB.StatusEffects[StatusEffectID.SHK]) ? 0.75f : 1f;

                playerFirst = playerUnit.Beast.Speed * playerShocked > enemyUnit.Beast.Speed * enemyShocked;
            }

            var firstUnit = (playerFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerFirst) ? enemyUnit : playerUnit;
            var secondBeast = secondUnit.Beast;

            yield return PerformSkill(firstUnit, secondUnit, firstUnit.Beast.CurrentSkill);
            if (state == BattleState.BattleEnd) { yield break; }

            if (secondBeast.HP > 0)
            {
                yield return PerformSkill(secondUnit, firstUnit, secondUnit.Beast.CurrentSkill);
                if (state == BattleState.BattleEnd) { yield break; }
            }

            yield return PerformAfterTurn(firstUnit);
            yield return PerformAfterTurn(secondUnit);
            if (state == BattleState.BattleEnd) { yield break; }
        }
        else
        {
            if (playerAction == BattleAction.Party)
            {
                state = BattleState.Busy;

                var selectedMember = partyScreen.CurrentMember;
                yield return SwitchBeast(selectedMember);
            }
            else if (playerAction == BattleAction.Bag) { selector.EnableActionSelector(false); }
            else if (playerAction == BattleAction.Run)
            {
                selector.EnableActionSelector(false);

                yield return TryToRun();
            }

            var enemySkill = enemyUnit.Beast.GetRandomSkill();
            yield return PerformSkill(enemyUnit, playerUnit, enemySkill);
            if (state == BattleState.BattleEnd) { yield break; }

            yield return PerformAfterTurn(playerUnit);
            yield return PerformAfterTurn(enemyUnit);
            if (state == BattleState.BattleEnd) { yield break; }
        }

        if (state != BattleState.BattleEnd) { SelectAction(); }
    }

    IEnumerator PerformAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleEnd) { yield break; }

        yield return new WaitUntil(() => state == BattleState.TurnPerformance);

        sourceUnit.Beast.OnAfterTurn();
        yield return sourceUnit.HUD.WaitForHPUpdate();
        yield return sourceUnit.HUD.WaitForSPUpdate();
        yield return ShowStatusChanges(sourceUnit.Beast);

        if (sourceUnit.Beast.HP <= 0)
        {
            yield return HandleFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.TurnPerformance);
        }
    }

    private void CheckForBattleEnd(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextBeast = playerParty.GetHealthyBeast();
            if (nextBeast != null) { OpenPartyScreen(); }
            else { EndBattle(false); }
        }
        else
        {
            if (!isChallenge) { EndBattle(true); }
            else
            {
                var nextBeast = challengerParty.GetHealthyBeast();
                if (nextBeast != null) { StartCoroutine(SelectChoice(nextBeast)); }
                else { EndBattle(true); }
            }
        }
    }

    //Battle End
    private void EndBattle(bool won)
    {
        state = BattleState.BattleEnd;

        foreach (var beast in playerParty.Beasts) { beast.OnBattleEnd(); }
        playerUnit.HUD.ClearHUD();
        enemyUnit.HUD.ClearHUD();

        OnBattleWin(won);
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection) { HandleActionSelection(); }
        else if (state == BattleState.SkillSelection) { HandleSkillSelection(); }
        else if (state == BattleState.PartySelection) { HandlePartySelection(); }
        else if (state == BattleState.BagSelection)
        {
            Action onBack = () =>
            {
                bagUI.gameObject.SetActive(false);

                SelectAction();
            };

            Action<ItemBase> onItemUsed = (ItemBase usedItem) => { StartCoroutine(OnItemUsed(usedItem)); };

            bagUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.ChoiceSelection) { HandleChoiceSelection(); }
        else if (state == BattleState.ForgetSelection)
        {
            Action<int> onChanged = (currentForget) =>
            {
                forgetSelector.UpdateForgetSelector(currentForget);
                skillDetails.UpdateSkillDetails(playerUnit.Beast.Skills[currentForget]);
            };

            Action<int> onSelected = (currentForget) =>
            {
                forgetSelector.EnableForgetSelector(false);
                skillDetails.EnableSkillDetails(false);

                var forgetSkill = playerUnit.Beast.Skills[currentForget].SkillBase;
                playerUnit.Beast.Skills[currentForget] = new Skill(skillToLearn);

                StartCoroutine(dialogBox.TypeDialog(playerUnit.Beast.BeastBase.Name
                    + " forgot " + forgetSkill.Name + " and learned " + skillToLearn.Name + "!"));

                skillToLearn = null;

                state = BattleState.TurnPerformance;
            };

            Action onBack = () =>
            {
                forgetSelector.EnableForgetSelector(false);
                skillDetails.EnableSkillDetails(false);

                StartCoroutine(dialogBox.TypeDialog(playerUnit.Beast.BeastBase.Name
                    + " forgot " + skillToLearn.Name + "!"));

                skillToLearn = null;

                state = BattleState.TurnPerformance;
            };

            forgetSelector.HandleForgetSelection(onChanged, onSelected, onBack);
        }
    }

    //Action Selection
    private void SelectAction()
    {
        state = BattleState.ActionSelection;

        selector.EnableActionSelector(true);
        selector.EnableSkillSelector(false);
        skillDetails.EnableSkillDetails(false);

        selector.UpdateActionSelector(currentAction);
    }

    private void HandleActionSelection()
    {
        int prevAction = currentAction;

        if (Input.GetKeyDown(KeyCode.DownArrow)) { ++currentAction; }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) { --currentAction; }

        if (currentAction > 3) { currentAction = 0; }
        else if (currentAction < 0) { currentAction = 3; }
        currentAction = Mathf.Clamp(currentAction, 0, 3);

        if (prevAction != currentAction) { selector.UpdateActionSelector(currentAction); }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0) { SelectSkill(); }
            else if (currentAction == 1) { OpenPartyScreen(); }
            else if (currentAction == 2) { SelectItem(); }
            else if (currentAction == 3) { StartCoroutine(PerformTurn(BattleAction.Run)); }
        }
    }

    //Skill Selection
    private void SelectSkill()
    {
        state = BattleState.SkillSelection;
        currentSkill = 0;

        selector.EnableActionSelector(false);
        selector.EnableSkillSelector(true);

        selector.UpdateSkillSelector(currentSkill);
        skillDetails.UpdateSkillDetails(playerUnit.Beast.Skills[currentSkill]);
    }

    private void HandleSkillSelection()
    {
        int prevSkill = currentSkill;

        if (Input.GetKeyDown(KeyCode.DownArrow)) { ++currentSkill; }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) { --currentSkill; }

        if (currentSkill > playerUnit.Beast.Skills.Count - 1) { currentSkill = 0; }
        else if (currentSkill < 0) { currentSkill = playerUnit.Beast.Skills.Count - 1; }
        currentSkill = Mathf.Clamp(currentSkill, 0, playerUnit.Beast.Skills.Count - 1);

        if (prevSkill != currentSkill)
        {
            selector.UpdateSkillSelector(currentSkill);
            skillDetails.UpdateSkillDetails(playerUnit.Beast.Skills[currentSkill]);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var skill = playerUnit.Beast.Skills[currentSkill];
            if (playerUnit.Beast.SP < skill.SP || playerUnit.Beast.HP < skill.HP)
            {
                StartCoroutine(CancelSkill());
                return;
            }

            selector.EnableSkillSelector(false);
            skillDetails.EnableSkillDetails(false);

            StartCoroutine(PerformTurn(BattleAction.Fight));
        }
        else if (Input.GetKeyDown(KeyCode.Y) && !isOpenDetails) 
        {
            isOpenDetails = true;
            skillDetails.EnableSkillDetails(true); 
        }
        else if (Input.GetKeyDown(KeyCode.Y) && isOpenDetails) 
        {
            isOpenDetails = false;
            skillDetails.EnableSkillDetails(false); 
        }
        else if (Input.GetKeyDown(KeyCode.X)) { SelectAction(); }
    }

    private IEnumerator CancelSkill()
    {
        state = BattleState.Busy;

        selector.EnableSkillSelector(false);
        skillDetails.EnableSkillDetails(false);

        yield return dialogBox.TypeDialog("Now is not the time to use that!");

        SelectSkill();
    }

    private IEnumerator PerformSkill(BattleUnit sourceUnit, BattleUnit targetUnit, Skill skill)
    {
        bool canPerfomSkill = sourceUnit.Beast.OnBeforeTurn();
        if (!canPerfomSkill)
        {
            yield return ShowStatusChanges(sourceUnit.Beast);
            yield break;
        }

        yield return ShowStatusChanges(sourceUnit.Beast);

        if (skill.SP != 0)
        {
            sourceUnit.Beast.UpdateSP(skill.SP);
            yield return sourceUnit.HUD.WaitForSPUpdate();
        }

        if (skill.HP != 0)
        {
            sourceUnit.Beast.UpdateHP(skill.HP);
            yield return sourceUnit.HUD.WaitForHPUpdate();
        }

        yield return new WaitForSeconds(1f);
        yield return dialogBox.TypeDialog
            (sourceUnit.Beast.BeastBase.Name + " used " + skill.SkillBase.Name + "!");

        if (CheckForSkillHit(sourceUnit.Beast, targetUnit.Beast, skill))
        {
            sourceUnit.PlayAttackAnimation();
            if (skill.SkillBase.SFX != null) { AudioManager.Instance.PlaySFX(skill.SkillBase.SFX); }
            yield return new WaitForSeconds(1f);

            if (skill.SkillBase.Category == Category.Status)
            {
                yield return PerformEffects(sourceUnit.Beast, targetUnit.Beast, skill.SkillBase.Target,
                        skill.SkillBase.SkillEffect);
            }
            else
            {
                targetUnit.PlayHitAnimation();
                AudioManager.Instance.PlaySFX(AudioID.Hit);
                yield return new WaitForSeconds(1f);

                var damageDetails = targetUnit.Beast.TakeDamage(skill, sourceUnit.Beast);
                yield return targetUnit.HUD.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);

                if (skill.SkillBase.HasRecoil)
                {
                    int attack = (skill.SkillBase.Category == Category.Physical) ? sourceUnit.Beast.Attack : sourceUnit.Beast.MagicAttack;
                    int defense = (skill.SkillBase.Category == Category.Physical) ? sourceUnit.Beast.Defense : sourceUnit.Beast.MagicDefense;
                    int recoil = (40 + attack) * skill.SkillBase.Power / (20 + defense) / 8 + UnityEngine.Random.Range(1, 6);

                    sourceUnit.Beast.UpdateHP(recoil);
                    yield return sourceUnit.HUD.WaitForHPUpdate();

                    yield return new WaitForSeconds(1f);
                    yield return dialogBox.TypeDialog
                        (sourceUnit.Beast.BeastBase.Name + " was damaged by recoil!");

                    if (sourceUnit.Beast.HP <= 0) { yield return HandleFainted(sourceUnit); }
                }
            }
            if (skill.SkillBase.SecondaryEffects != null && skill.SkillBase.SecondaryEffects.Count > 0
                && targetUnit.Beast.HP > 0)
            {
                foreach (var effect in skill.SkillBase.SecondaryEffects)
                {
                    int random = UnityEngine.Random.Range(1, 101);
                    if (random <= effect.Chance)
                    { yield return PerformEffects(sourceUnit.Beast, targetUnit.Beast, effect.Target, effect); }
                }
            }

            if (targetUnit.Beast.HP <= 0) { yield return HandleFainted(targetUnit); }
        }
        else 
        {
            targetUnit.PlayDodgeAnimation();
            yield return dialogBox.TypeDialog(sourceUnit.Beast.BeastBase.Name + "'s attack missed!"); 
        }
    }

    private bool CheckForSkillHit(Beast source, Beast target, Skill skill)
    {
        if (skill.SkillBase.AlwaysHit) { return true; }

        float skillAccuracy = skill.SkillBase.Accuracy;
        int accuracy = source.StatBoosts[Stat.ACC];
        int evasion = target.StatBoosts[Stat.EVA];

        var boostValues = new float[] { 1f, 1.2f, 1.4f, 1.6f, 1.8f, 2f };

        if (accuracy > 0) { skillAccuracy *= boostValues[accuracy]; }
        else if (accuracy < 0) { skillAccuracy /= boostValues[-accuracy]; }

        if (evasion > 0) { skillAccuracy /= boostValues[evasion]; }
        else if (evasion < 0) { skillAccuracy *= boostValues[-evasion]; }

        return UnityEngine.Random.Range(1, 101) <= skillAccuracy;
    }

    private IEnumerator PerformEffects(Beast source, Beast target, Target skillTarget, SkillEffect effects)
    {
        if (effects.Boosts != null)
        {
            if (skillTarget == Target.Self) { source.ApplyBoosts(effects.Boosts); }
            else { target.ApplyBoosts(effects.Boosts); }
        }

        if (effects.Status != StatusEffectID.None)
        {
            if (skillTarget == Target.Self) { source.SetStatus(effects.Status); }
            else { target.SetStatus(effects.Status); }
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    private IEnumerator HandleFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog(faintedUnit.Beast.BeastBase.Name + " fainted!");
        faintedUnit.PlayFaintAnimation();
        faintedUnit.HUD.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);

        yield return GainExp(faintedUnit);

        CheckForBattleEnd(faintedUnit);
    }

    private IEnumerator GainExp(BattleUnit faintedUnit)
    {
        if (!faintedUnit.IsPlayerUnit)
        {
            bool battleWon = true;
            if (isChallenge) { battleWon = challengerParty.GetHealthyBeast() == null; }

            if (battleWon) { AudioManager.Instance.PlayMusic(victoryMusic); }

            int baseExp = faintedUnit.Beast.BeastBase.BaseExp;
            int enemyLevel = faintedUnit.Beast.Level;
            float challengerBonus = (isChallenge) ? 1.5f : 1f;
            float random = UnityEngine.Random.Range(1f, 1.25f);
            int expGain = Mathf.FloorToInt((baseExp * enemyLevel * challengerBonus * random) / 5);
            playerUnit.Beast.Exp += expGain;

            yield return dialogBox.TypeDialog(playerUnit.Beast.BeastBase.Name + " gained " + expGain + " EXP!");
            yield return playerUnit.HUD.SetExpSmooth();

            while (playerUnit.Beast.CheckForLevelUp())
            {
                playerUnit.HUD.SetLevel();
                yield return dialogBox.TypeDialog(playerUnit.Beast.BeastBase.Name + " grew to level "
                    + playerUnit.Beast.Level + "!");

                var newSkill = playerUnit.Beast.GetLearnableSkillAtCurrLevel();
                if (newSkill != null)
                {
                    if (playerUnit.Beast.Skills.Count < 4)
                    {
                        playerUnit.Beast.LearnNewSkill(newSkill.SkillBase);
                        yield return dialogBox.TypeDialog
                            (playerUnit.Beast.BeastBase.Name + " learned " + newSkill.SkillBase.Name + "!");
                        selector.SetSkill(playerUnit.Beast.Skills);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog
                            (playerUnit.Beast.BeastBase.Name + " tried to learn " + newSkill.SkillBase.Name + "!");
                        yield return SelectForget(playerUnit.Beast, newSkill.SkillBase);
                        yield return new WaitUntil(() => state != BattleState.ForgetSelection);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.HUD.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    //Party Selection
    private void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartySelection;

        partyScreen.gameObject.SetActive(true);
        selector.EnableActionSelector(false);
    }

    private void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.CurrentMember;
            if (selectedMember.HP <= 0)
            {
                StartCoroutine(dialogBox.TypeDialog(selectedMember.BeastBase.Name + " is fainted!"));
                return;
            }

            if (selectedMember == playerUnit.Beast)
            {
                StartCoroutine(dialogBox.TypeDialog(selectedMember.BeastBase.Name + " is in battle!"));
                return;
            }

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            { StartCoroutine(PerformTurn(BattleAction.Party)); }
            else
            {
                state = BattleState.Busy;

                bool isSwitching = partyScreen.CalledFrom == BattleState.ChoiceSelection;
                StartCoroutine(SwitchBeast(selectedMember, isSwitching));
            }

            partyScreen.CalledFrom = null;
        };
        Action onBack = () =>
        {
            if (playerUnit.Beast.HP <= 0)
            {
                StartCoroutine(dialogBox.TypeDialog("Now is the time to choose a new Beast!"));
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ChoiceSelection) { StartCoroutine(SendNextBeast()); }
            else { SelectAction(); }

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandlePartyScreen(onSelected, onBack);
    }

    private IEnumerator SwitchBeast(Beast newBeast, bool isSwitching = false)
    {
        playerUnit.ClearUnit();
        selector.EnableActionSelector(false);
        partyScreen.gameObject.SetActive(false);

        if (playerUnit.Beast.HP > 0)
        {
            yield return dialogBox.TypeDialog("Come back " + playerUnit.Beast.BeastBase.Name + "!");
            playerUnit.PlayRetreatAnimation();
            yield return new WaitForSeconds(1f);
        }

        playerUnit.SetUnit(newBeast);
        selector.SetSkill(newBeast.Skills);
        yield return dialogBox.TypeDialog("Go " + newBeast.BeastBase.Name + "!");

        if (isSwitching) { StartCoroutine(SendNextBeast()); }
        else { state = BattleState.TurnPerformance; }
    }

    private IEnumerator SendNextBeast()
    {
        state = BattleState.Busy;

        var nextBeast = challengerParty.GetHealthyBeast();
        enemyUnit.SetUnit(nextBeast);
        yield return dialogBox.TypeDialog(challenger.name + " sent out " + nextBeast.BeastBase.Name + "!");

        state = BattleState.TurnPerformance;
    }

    //Bag Selection
    private void SelectItem()
    {
        state = BattleState.BagSelection;

        bagUI.gameObject.SetActive(true);
        selector.EnableActionSelector(false);
    }

    private IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;

        bagUI.gameObject.SetActive(false);

        if (usedItem is Trap) { yield return UseBeastTrap((Trap)usedItem); }

        yield return PerformTurn(BattleAction.Bag);
    }

    //Capture
    private IEnumerator UseBeastTrap(Trap beastTrap)
    {
        state = BattleState.Busy;

        selector.EnableActionSelector(false);

        if (isChallenge)
        {
            yield return dialogBox.TypeDialog(player.name + " can't steal other tamers pokemon!");

            state = BattleState.TurnPerformance;

            yield break;
        }

        yield return dialogBox.TypeDialog(player.name + " used " + beastTrap.Name + "!");

        var trapObj = Instantiate(trapSprite, playerUnit.transform.position - new Vector3(50f, 10f)
            , Quaternion.identity);
        var trap = trapObj.GetComponent<SpriteRenderer>();
        trap.sprite = beastTrap.Icon;
        yield return trap.transform.DOJump(enemyUnit.transform.position
            + new Vector3(-50f, 0f), 2f, 1, 1f).WaitForCompletion();

        enemyUnit.HUD.gameObject.SetActive(false);
        yield return enemyUnit.PlayCaptureAnimation();

        int shakeCount = TryToCaptureBeast(enemyUnit.Beast, beastTrap);
        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(1f);
            trap.transform.DOPunchRotation(new Vector3(0f, 0f, 10f), 0.75f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            yield return new WaitForSeconds(1f);
            yield return dialogBox.TypeDialog(enemyUnit.Beast.BeastBase.Name + " was captured successfully!");
            yield return trap.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddBeast(enemyUnit.Beast);
            yield return dialogBox.TypeDialog
                (enemyUnit.Beast.BeastBase.Name + " was added to " + player.name + "'s party!");

            trap.gameObject.SetActive(false);

            EndBattle(true);
        }
        else
        {
            yield return new WaitForSeconds(1f);
            yield return trap.DOFade(0, 0.25f);
            yield return enemyUnit.PlayBreakOutAnimation();
            yield return dialogBox.TypeDialog(enemyUnit.Beast.BeastBase.Name + " broke free!");

            trap.gameObject.SetActive(false);
            enemyUnit.HUD.gameObject.SetActive(true);

            state = BattleState.TurnPerformance;
        }
    }

    private int TryToCaptureBeast(Beast beast, Trap beastTrap)
    {
        int captureRate = beast.BeastBase.CaptureRate;
        float statusBonus = StatusEffectDB.GetStatusBonus(beast.Status);
        float levelDif = Mathf.Max((40 - beast.Level) / 10, 1);
        float trapModifier = beastTrap.CaptureRateModifier;
        float successRate = ((beast.MaxHP * 2) - beast.HP) * captureRate * statusBonus * levelDif * trapModifier
            / (beast.MaxHP * 2);
        if (successRate >= 120) { return 4; }

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 100) > successRate) { break; }

            shakeCount++;
        }

        return shakeCount;
    }

    //Choice Selection
    private IEnumerator SelectChoice(Beast newBeast)
    {
        state = BattleState.Busy;

        yield return dialogBox.TypeDialog
            (challenger.name + " is sending out " + newBeast.BeastBase.Name + "!");
        yield return dialogBox.TypeDialog("Do " + player.name + " want to change Beast ?");

        state = BattleState.ChoiceSelection;

        selector.EnableActionSelector(false);
        selector.EnableChoiceSelector(true);

        selector.UpdateChoiceSelector(choice);
    }

    private void HandleChoiceSelection()
    {
        bool prevChoice = choice;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) { choice = !choice; }

        if (prevChoice != choice) { selector.UpdateChoiceSelector(choice); }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            selector.EnableChoiceSelector(false);
            if (choice == true) { OpenPartyScreen(); }
            else { StartCoroutine(SendNextBeast()); }
        }
    }

    //Forget Selection
    private IEnumerator SelectForget(Beast beast, SkillBase newSkill)
    {
        state = BattleState.Busy;

        yield return dialogBox.TypeDialog("Choose a skill " + playerUnit.Beast.BeastBase.Name + "  would forget!");

        state = BattleState.ForgetSelection;

        forgetSelector.EnableForgetSelector(true);
        skillDetails.EnableSkillDetails(true);

        forgetSelector.SetForgetNames(beast.Skills.Select(x => x.SkillBase).ToList());

        forgetSelector.UpdateForgetSelector(0);
        skillDetails.UpdateSkillDetails(playerUnit.Beast.Skills[0]);

        skillToLearn = newSkill;
    }

    //Run
    private IEnumerator TryToRun()
    {
        state = BattleState.Busy;

        if (isChallenge)
        {
            yield return dialogBox.TypeDialog(player.name + " can't run from challenge!");

            state = BattleState.TurnPerformance;

            yield break;
        }

        escapeAttempts++;

        int playerSpeed = playerUnit.Beast.Speed;
        int enemySpeed = enemyUnit.Beast.Speed;

        if (enemySpeed < playerSpeed)
        {
            playerUnit.PlayRetreatAnimation();
            yield return new WaitForSeconds(1f);
            yield return dialogBox.TypeDialog(player.name + " ran away safely!");

            EndBattle(true);
        }
        else
        {
            float escapeChance = (playerSpeed * 2 / enemySpeed) * escapeAttempts;
            escapeChance = escapeChance % 100;

            if (UnityEngine.Random.Range(0, 101) < escapeChance)
            {
                playerUnit.PlayRetreatAnimation();
                yield return new WaitForSeconds(1f);
                yield return dialogBox.TypeDialog(player.name + " ran away safely!");

                EndBattle(true);
            }
            else
            {
                playerUnit.PlayRetreatAnimation();
                yield return new WaitForSeconds(1f);
                yield return dialogBox.TypeDialog(player.name + " tried to escape but failed!");
                playerUnit.PlayEnterAnimation();

                state = BattleState.TurnPerformance;
            }
        }
    }

    //MESSAGE
    private IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f) { yield return dialogBox.TypeDialog("A critical hit!"); }

        if (damageDetails.ElementEffectiveness > 1f)
        { yield return dialogBox.TypeDialog("A super effective hit!"); }
        else if (damageDetails.ElementEffectiveness < 1f)
        { yield return dialogBox.TypeDialog("A not very effective hit!"); }
    }

    private IEnumerator ShowStatusChanges(Beast beast)
    {
        while (beast.StatusChanges.Count > 0)
        {
            var message = beast.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
}
