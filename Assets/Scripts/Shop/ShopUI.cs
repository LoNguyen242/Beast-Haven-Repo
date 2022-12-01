using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text desText;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    private List<ItemBase> availableItems;
    private List<ItemSlotUI> itemSlotUIList;
    private RectTransform itemListRect;

    private int currentItem;

    private Action<ItemBase> OnItemSelected;
    private Action OnBack;

    private void Awake()
    {
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    public void ShowShopUI(List<ItemBase> availableItems, Action onBack, Action<ItemBase> onItemSelected)
    {
        this.availableItems = availableItems;
        OnItemSelected = onItemSelected;
        OnBack = onBack;

        gameObject.SetActive(true);

        UpdateAvailableItemList();
    }

    public void HandleUpdate()
    {
        int prevItem = currentItem;

        if (Input.GetKeyDown(KeyCode.DownArrow)) { currentItem++; }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) { currentItem--; }

        if (currentItem > availableItems.Count - 1) { currentItem = 0; }
        else if (currentItem < 0) { currentItem = availableItems.Count - 1; }
        currentItem = Mathf.Clamp(currentItem, 0, availableItems.Count - 1);

        if (prevItem != currentItem) { UpdateAvailableItemSelection(); }

        if (Input.GetKeyDown(KeyCode.Z)) { OnItemSelected?.Invoke(availableItems[currentItem]); }
        else if (Input.GetKeyDown(KeyCode.X)) { OnBack?.Invoke(); }
    }

    private void UpdateAvailableItemList()
    {
        foreach (Transform child in itemList.transform) { Destroy(child.gameObject); }

        itemSlotUIList = new List<ItemSlotUI>();
        foreach (var item in availableItems)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetNameAndPrice(item);
            itemSlotUIList.Add(slotUIObj);
        }

        UpdateAvailableItemSelection();
    }

    private void UpdateAvailableItemSelection()
    {
        currentItem = Mathf.Clamp(currentItem, 0, availableItems.Count - 1);

        for (int i = 0; i < itemSlotUIList.Count; i++)
        {
            if (i == currentItem) { itemSlotUIList[i].NameText.color = Color.magenta; }
            else { itemSlotUIList[i].NameText.color = Color.black; }
        }

        if (availableItems.Count > 0)
        {
            var item = availableItems[currentItem];
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

        bool showUpArrow = currentItem > 2;
        bool showDownArrow = currentItem + 2 < itemSlotUIList.Count - 2;
        upArrow.gameObject.SetActive(showUpArrow);
        downArrow.gameObject.SetActive(showDownArrow);
    }
}
