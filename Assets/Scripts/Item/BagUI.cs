using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum BagState { ItemSelection, PartySelection, ForgetSelection, Busy }

public class BagUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text categoryText;
    [SerializeField] Text desText;

    [SerializeField] Image leftArrow;
    [SerializeField] Image rightArrow;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] ForgetSelector forgetSelector;
    [SerializeField] SkillDetails skillDetails;

    private Bag bag;
    private List<ItemSlotUI> itemSlotUIList;
    private RectTransform itemListRect;

    private SkillBase skillToLearn;

    private BagState state;
    private int currentItem;
    private int currentCategory;

    private Action<ItemBase> OnItemUsed;

    private void Awake()
    {
        bag = Bag.GetBag();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();

        bag.OnUpdated += UpdateItemList;

        DialogManager.Instance.SetSpeaker(true);
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null)
    {
        OnItemUsed = onItemUsed;

        if (state == BagState.ItemSelection)
        {
            int prevCategory = currentCategory;
            int prevItem = currentItem;

            categoryText.text = Bag.ItemCategories[currentCategory];

            if (Input.GetKeyDown(KeyCode.RightArrow)) { currentCategory++; }
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) { currentCategory--; }
            else if (Input.GetKeyDown(KeyCode.DownArrow)) { currentItem++; }
            else if (Input.GetKeyDown(KeyCode.UpArrow)) { currentItem--; }

            if (currentCategory > Bag.ItemCategories.Count - 1) { currentCategory = 0; }
            else if (currentCategory < 0) { currentCategory = Bag.ItemCategories.Count - 1; }
            currentCategory = Mathf.Clamp(currentCategory, 0, Bag.ItemCategories.Count - 1);

            bool showUpArrow = currentItem > 2;
            bool showDownArrow = currentItem + 2 < itemSlotUIList.Count - 2;
            upArrow.gameObject.SetActive(showUpArrow);
            downArrow.gameObject.SetActive(showDownArrow);

            if (currentItem > bag.GetCategory(currentCategory).Count - 1) { currentItem = 0; }
            else if (currentItem < 0) { currentItem = bag.GetCategory(currentCategory).Count - 1; }
            currentItem = Mathf.Clamp(currentItem, 0, bag.GetCategory(currentCategory).Count - 1);

            if (prevCategory != currentCategory)
            {
                ResetItemSelection();
                categoryText.text = Bag.ItemCategories[currentCategory];
                UpdateItemList();
            }
            else if (prevItem != currentItem) { UpdateItemSelection(); }

            if (Input.GetKeyDown(KeyCode.Z)) { StartCoroutine(UseCategory()); }
            else if (Input.GetKeyDown(KeyCode.X)) { onBack?.Invoke(); }
        }
        else if (state == BagState.PartySelection)
        {
            Action onSelected = () => { StartCoroutine(UseItem()); };

            Action onBackPartyScreen = () => { ClosePartyScreen(); };

            partyScreen.HandlePartyScreen(onSelected, onBackPartyScreen);
        }
        else if (state == BagState.ForgetSelection)
        {
            Action<int> onChanged = (currentForget) =>
            {
                forgetSelector.UpdateForgetSelector(currentForget);
                skillDetails.UpdateSkillDetails(partyScreen.CurrentMember.Skills[currentForget]);
            };

            Action<int> onSelected = (currentForget) => { StartCoroutine(OnSelectedForget(currentForget)); };

            Action onBackForgetSelector = () => { StartCoroutine(OnBackForget()); };

            forgetSelector.HandleForgetSelection(onChanged, onSelected, onBackForgetSelector);
        }
    }

    private void UpdateItemList()
    {
        foreach (Transform child in itemList.transform) { Destroy(child.gameObject); }

        itemSlotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in bag.GetCategory(currentCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetNameAndCount(itemSlot);
            itemSlotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    private void UpdateItemSelection()
    {
        var slots = bag.GetCategory(currentCategory);
        currentItem = Mathf.Clamp(currentItem, 0, slots.Count - 1);

        for (int i = 0; i < itemSlotUIList.Count; i++)
        {
            if (i == currentItem) { itemSlotUIList[i].NameText.color = Color.magenta; }
            else { itemSlotUIList[i].NameText.color = Color.black; }
        }

        if (slots.Count > 0)
        {
            var item = slots[currentItem].ItemBase;
            itemIcon.sprite = item.Icon;
            desText.text = item.Description;
        }

        HandleScrolling();
    }

    private void HandleScrolling()
    {
        if (itemSlotUIList.Count <= 6) { return; }
        float scrollPos = Mathf.Clamp(currentItem - 2, 0, currentItem) * itemSlotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);
    }

    private void ResetItemSelection()
    {
        currentItem = 0;

        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        desText.text = "";
    }

    private IEnumerator UseCategory()
    {
        state = BagState.Busy;

        var item = bag.GetItem(currentCategory, currentItem);

        if (GameController.Instance.State == GameState.Shop)
        {
            OnItemUsed?.Invoke(item);
            state = BagState.ItemSelection;

            yield break;
        }

        if (GameController.Instance.State == GameState.Battle)
        {
            if (!item.CanUseInsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogString(item.Name + " couldn't be used inside battle!");

                state = BagState.ItemSelection;
                yield break;
            }
        }
        else
        {
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogString(item.Name + " couldn't be used outside battle!");

                state = BagState.ItemSelection;
                yield break;
            }
        }

        if (currentCategory == (int)ItemCategory.Traps) { yield return UseItem(); }
        else
        {
            OpenPartyScreen();

            if (item is Artifact) { partyScreen.ShowMessage(item as Artifact); }
            if (item is ShiftGem) { partyScreen.ShowMessage(item as ShiftGem); }
        }
    }

    private IEnumerator UseItem()
    {
        state = BagState.Busy;

        yield return HandleArtifact();

        var beast = partyScreen.CurrentMember;
        var item = bag.GetItem(currentCategory, currentItem);
        if (item is ShiftGem)
        {
            var powerShift = beast.CheckForPowerShift(item);
            if (powerShift != null) { yield return PowerShiftManager.Instance.Shift(beast, powerShift); }
            else
            {
                yield return DialogManager.Instance.ShowDialogString("It did't have any effect...");
                ClosePartyScreen();
                yield break;
            }
        }

        var usedItem = bag.UseItem(currentCategory, currentItem, beast);
        if (usedItem != null)
        {
            if (usedItem is Sweet)
            {
                yield return DialogManager.Instance.ShowDialogString
                    ("The Protagonist used " + usedItem.Name + " successfully!");
            }

            OnItemUsed?.Invoke(usedItem);
        }
        else
        {
            if (currentCategory == (int)ItemCategory.Sweets)
            {
                yield return DialogManager.Instance.ShowDialogString("It did't have any effect...");
            }
        }

        state = BagState.PartySelection;
        //ClosePartyScreen();
    }

    private IEnumerator HandleArtifact()
    {
        var artifact = bag.GetItem(currentCategory, currentItem) as Artifact;
        if (artifact == null) { yield break; }

        var beast = partyScreen.CurrentMember;
        if (beast.HasSkill(artifact.Skill))
        {
            yield return DialogManager.Instance.ShowDialogString
                (beast.BeastBase.Name + " has already known " + artifact.Skill.Name + "!");
            yield break;
        }

        if (!artifact.CheckForLearn(beast))
        {
            yield return DialogManager.Instance.ShowDialogString
                (beast.BeastBase.Name + " could't learn " + artifact.Skill.Name + "!");
            yield break;
        }

        if (beast.Skills.Count < 4)
        {
            beast.LearnNewSkill(artifact.Skill);
            yield return DialogManager.Instance.ShowDialogString
                (beast.BeastBase.Name + " learned " + artifact.Skill.Name + "!");
        }
        else
        {
            yield return SelectForget(beast, artifact.Skill);
            yield return new WaitUntil(() => state != BagState.ForgetSelection);
        }
    }

    private IEnumerator SelectForget(Beast beast, SkillBase newSkill)
    {
        state = BagState.Busy;

        yield return DialogManager.Instance.ShowDialogString
            (beast.BeastBase.Name + " tried to learn " + newSkill.Name + "!");
        yield return DialogManager.Instance.ShowDialogString
            (("Choose a skill " + beast.BeastBase.Name + "  would forget!"));

        forgetSelector.EnableForgetSelector(true);
        skillDetails.EnableSkillDetails(true);

        forgetSelector.SetForgetNames(beast.Skills.Select(x => x.SkillBase).ToList());

        forgetSelector.UpdateForgetSelector(0);
        skillDetails.UpdateSkillDetails(partyScreen.CurrentMember.Skills[0]);

        skillToLearn = newSkill;

        state = BagState.ForgetSelection;
    }

    private IEnumerator OnSelectedForget(int currentForget)
    {
        forgetSelector.EnableForgetSelector(false);
        skillDetails.EnableSkillDetails(false);
        DialogManager.Instance.CloseDialog();

        var beast = partyScreen.CurrentMember;
        var forgetSkill = beast.Skills[currentForget].SkillBase;
        beast.Skills[currentForget] = new Skill(skillToLearn);

        yield return DialogManager.Instance.ShowDialogString
            (beast.BeastBase.Name + " forgot " + forgetSkill.Name + " and learned " + skillToLearn.Name + "!");

        skillToLearn = null;

        state = BagState.ItemSelection;
    }

    private IEnumerator OnBackForget()
    {
        forgetSelector.EnableForgetSelector(false);
        skillDetails.EnableSkillDetails(false);

        var beast = partyScreen.CurrentMember;

        yield return DialogManager.Instance.ShowDialogString
            (beast.BeastBase.Name + " did't learn " + skillToLearn.Name + "!");

        skillToLearn = null;

        state = BagState.ItemSelection;
    }

    private void OpenPartyScreen()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        { this.transform.GetChild(i).gameObject.SetActive(false); }
        partyScreen.gameObject.SetActive(true);

        state = BagState.PartySelection;
    }

    private void ClosePartyScreen()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        { this.transform.GetChild(i).gameObject.SetActive(true); }
        partyScreen.ClearMessage();
        partyScreen.gameObject.SetActive(false);

        state = BagState.ItemSelection;
    }
}
