using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Beast
{
    [SerializeField] BeastBase beastBase;
    [SerializeField] int level;

    public BeastBase BeastBase { get { return beastBase; } }
    public int Level { get { return level; } }
    public int Exp { get; set; }
    public int HP { get; set; }
    public int SP { get; set; }

    public List<Skill> Skills { get; set; }
    public Skill CurrentSkill { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public StatusEffect Status { get; private set; }
    public int StatusTime { get; set; }
    public Queue<string> StatusChanges { get; set; }

    public event Action OnStatusChanged;
    public event Action OnHPChanged;
    public event Action OnSPChanged;
    public event Action OnLevelChanged;

    public Beast(BeastBase _base, int _level)
    {
        beastBase = _base;
        level = _level;

        Init();
    }

    public Beast(BeastSaveData saveData)
    {
        beastBase = BeastDB.GetObjectByName(saveData.name);

        level = saveData.level;
        Exp = saveData.exp;
        HP = saveData.hp;
        SP = saveData.sp;

        if (saveData.statusId != null) { Status = StatusEffectDB.StatusEffects[saveData.statusId.Value]; }
        else { Status = null; }

        Skills = saveData.skills.Select(s => new Skill(s)).ToList();

        CalculateStats();

        StatusChanges = new Queue<string>();

        OnBattleEnd();
    }

    public BeastSaveData GetSaveData()
    {
        var saveData = new BeastSaveData()
        {
            name = BeastBase.name,
            level = Level,
            exp = Exp,
            hp = HP,
            sp = SP,
            statusId = Status?.ID,
            skills = Skills.Select(s => s.GetSaveData()).ToList(),
        };

        return saveData;
    }

    public void Init()
    {
        Skills = new List<Skill>();
        foreach (var skill in BeastBase.LearnableSkills)
        {
            if (skill.Level <= Level) { Skills.Add(new Skill(skill.SkillBase)); }

            if (Skills.Count >= 4) { break; }
        }

        Exp = BeastBase.GetExpForLevel(level);

        CalculateStats();
        HP = MaxHP;
        SP = MaxSP;

        StatusChanges = new Queue<string>();

        OnBattleEnd();
    }

    private void CalculateStats()
    {
        int oldMaxHP = MaxHP;
        MaxHP = Mathf.FloorToInt(((Level / 40f) + 1) * BeastBase.MaxHP + Level);

        if (oldMaxHP != 0)
        {
            int hpDiff = MaxHP - oldMaxHP;
            HP += hpDiff;
            UpdateHP(-hpDiff);
        }

        int oldMaxSP = MaxSP;
        MaxSP = Mathf.FloorToInt((((Level / 40f) + 1) * BeastBase.MaxSP));

        if (oldMaxSP != 0)
        {
            int spDiff = MaxSP - oldMaxSP;
            SP += spDiff;
            UpdateSP(-spDiff);
        }

        Stats = new Dictionary<Stat, int>
        {
            { Stat.ATK, Mathf.FloorToInt(((Level / 20f) + 1) * BeastBase.Attack / 0.6f) },
            { Stat.DEF, Mathf.FloorToInt(((Level / 20f) + 1) * BeastBase.Defense / 0.6f) },
            { Stat.SATK, Mathf.FloorToInt(((Level / 20f) + 1) * BeastBase.SpecialAttack / 0.6f) },
            { Stat.SDEF, Mathf.FloorToInt(((Level / 20f) + 1) * BeastBase.SpecialDefense / 0.6f) },
            { Stat.SPD, Mathf.FloorToInt(((Level / 20f) + 1) * BeastBase.Speed / 0.6f) },
        };
    }

    public int MaxHP { get; private set; }

    public int MaxSP { get; private set; }

    public int Attack
    {
        get { return GetStat(Stat.ATK); }
    }

    public int Defense
    {
        get { return GetStat(Stat.DEF); }
    }

    public int MagicAttack
    {
        get { return GetStat(Stat.SATK); }
    }

    public int MagicDefense
    {
        get { return GetStat(Stat.SDEF); }
    }

    public int Speed
    {
        get { return GetStat(Stat.SPD); }
    }

    public void ResetBoosts()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.ATK, 0},
            {Stat.DEF, 0},
            {Stat.SATK, 0},
            {Stat.SDEF, 0},
            {Stat.SPD, 0},
            {Stat.ACC, 0},
            {Stat.EVA, 0}
        };
    }

    private int GetStat(Stat stat)
    {
        int statValue = Stats[stat];

        //Apply stat boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 2f, 1.8f, 2.2f, 2.6f, 3f };

        if (boost >= 0) { statValue = Mathf.FloorToInt(statValue * boostValues[boost]); }
        else { statValue = Mathf.FloorToInt(statValue / boostValues[-boost]); }

        return statValue;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;
            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -5, 5);

            if (boost > 0) { StatusChanges.Enqueue(BeastBase.Name + "'s " + stat + " rose!"); }
            else if (boost < 0) { StatusChanges.Enqueue(BeastBase.Name + "'s " + stat + " fell!"); }

            Debug.Log(stat + " has been boosted to " + StatBoosts[stat]);
        }
    }

    public void SetStatus(StatusEffectID statusEffectID)
    {
        if (Status != null) return;

        Status = StatusEffectDB.StatusEffects[statusEffectID];
        Status?.OnStartTurn?.Invoke(this);
        StatusChanges.Enqueue(BeastBase.Name + Status.Message);
        OnStatusChanged?.Invoke();
    }

    public void EndStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public bool OnBeforeTurn()
    {
        bool canPerformTurn = true;
        if (Status?.OnBeforeTurn != null) 
        {
            if (!Status.OnBeforeTurn(this)) { canPerformTurn = false; } 
        }

        return canPerformTurn;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleEnd()
    {
        ResetBoosts();
        Status = null;
    }

    public DamageDetails TakeDamage(Skill skill, Beast attacker)
    {
        float burnt = (attacker.Status == StatusEffectDB.StatusEffects[StatusEffectID.BRN]) ? 0.75f : 1f;
        float frozen = (Status == StatusEffectDB.StatusEffects[StatusEffectID.FRZ]) ? 1.25f : 1f;

        float attack = (skill.SkillBase.Category == Category.Physical) ? attacker.Attack * burnt : attacker.MagicAttack * burnt;
        float defense = (skill.SkillBase.Category == Category.Physical) ? Defense * frozen : MagicDefense * frozen;

        if (attacker.Status == StatusEffectDB.StatusEffects[StatusEffectID.CRS])
        { attack = (skill.SkillBase.Category == Category.Physical) ? attacker.Defense : attacker.MagicDefense; }
        if (Status == StatusEffectDB.StatusEffects[StatusEffectID.CRS])
        { defense = (skill.SkillBase.Category == Category.Physical) ? Attack : MagicAttack; }

        float critical = 1f;
        if (Random.value * 100f <= 5f) { critical = 2f; }

        float element = 1f;
        for (int i = 0; i < beastBase.WeakElements.Length; i++)
        { element *= GetElementEffectiveness(1, skill.SkillBase.Element, BeastBase.WeakElements[i]); }
        for (int i = 0; i < beastBase.ResistElements.Length; i++)
        { element *= GetElementEffectiveness(2, skill.SkillBase.Element, BeastBase.ResistElements[i]); }
        for (int i = 0; i < beastBase.ImmuneElements.Length; i++)
        { element *= GetElementEffectiveness(3, skill.SkillBase.Element, BeastBase.ImmuneElements[i]); }

        if (attacker.Status == StatusEffectDB.StatusEffects[StatusEffectID.BLS] || Status == StatusEffectDB.StatusEffects[StatusEffectID.BLS])
        { element = 1f; }

        float flatDmg = (((40 + attack + (6 * attacker.Level)) * skill.SkillBase.Power)
            / (20 + defense)) / 4 + Random.Range(1, 6);
        float modifiers = Random.Range(0.85f, 1f) * critical * element;
        int overallDmg = Mathf.FloorToInt(flatDmg * modifiers);

        UpdateHP(overallDmg);

        var damageDetails = new DamageDetails()
        {
            Critical = critical,
            ElementEffectiveness = element,
            Fainted = false
        };

        return damageDetails;
    }

    public float GetElementEffectiveness(int affinity, Element attackElement, Element defenseElement)
    {
        if (attackElement == Element.Null || defenseElement == Element.Null) { return 1f; }

        if (affinity == 1)
        {
            if (attackElement == defenseElement) { return 1.5f; }
            else { return 1f; }
        }
        else if (affinity == 2)
        {
            if (attackElement == defenseElement) { return 0.5f; }
            else { return 1f; }
        }
        else
        {
            if (attackElement == defenseElement) { return 0f; }
            else { return 1f; }
        }
    }

    public Skill GetRandomSkill()
    {
        var skillsCanPerform = Skills.Where(s => s.SkillBase.SP < SP).ToList();

        int random = Random.Range(0, skillsCanPerform.Count);
        return skillsCanPerform[random];
    }

    public void Heal()
    {
        HP = MaxHP;
        SP = MaxSP;
        OnHPChanged?.Invoke();
        OnSPChanged?.Invoke();
        EndStatus();
    }

    public void UpdateHP(int amount)
    {
        HP = Mathf.Clamp(HP - amount, 0, MaxHP);
        OnHPChanged?.Invoke();
    }

    public void UpdateSP(int amount)
    {
        SP = Mathf.Clamp(SP - amount, 0, MaxSP);
        OnSPChanged?.Invoke();
    }

    public bool CheckForLevelUp()
    {
        if (Exp > beastBase.GetExpForLevel(level + 1))
        {
            level++;
            CalculateStats();
            OnLevelChanged?.Invoke();
            return true;
        }

        return false;
    }

    public LearnableSkill GetLearnableSkillAtCurrLevel()
    {
        return BeastBase.LearnableSkills.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnNewSkill(SkillBase skillToLearn)
    {
        if (Skills.Count > 4) { return; }

        Skills.Add(new Skill(skillToLearn));
    }

    public bool HasSkill(SkillBase skillToCheck)
    {
        return Skills.Count(s => s.SkillBase == skillToCheck) > 0;
    }

    public PowerShift CheckForPowerShift(ItemBase item)
    {
        return BeastBase.PowerShifts.FirstOrDefault(s => s.RequiredLevel <= level && s.RequiredItem == item);
    }

    public void ShiftIntoNextStage(PowerShift powerShift)
    {
        beastBase = powerShift.NextStage;
        CalculateStats();
    }
}

public class DamageDetails
{
    public float Critical { get; set; }
    public float ElementEffectiveness { get; set; }
    public bool Fainted { get; set; }
}

[Serializable]
public class BeastSaveData
{
    public string name;
    public int level;
    public int exp;
    public int hp;
    public int sp;

    public StatusEffectID? statusId;

    public List<SkillSaveData> skills;
}
