using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory { ShiftGems, Traps, Artifacts, Sweets }

public class Bag : MonoBehaviour, ISavable
{
    [SerializeField] List<ItemSlot> shiftGemSlots;
    [SerializeField] List<ItemSlot> trapSlots;
    [SerializeField] List<ItemSlot> artifactSlots;
    [SerializeField] List<ItemSlot> sweetSlots;

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "Shift Gems", "Traps", "Artifacts", "Sweets"
    };
    private List<List<ItemSlot>> itemSlots;

    public event Action OnUpdated;

    private void Awake()
    {
        itemSlots = new List<List<ItemSlot>>()
        {
            shiftGemSlots,
            trapSlots,
            artifactSlots,
            sweetSlots
        };
    }

    public static Bag GetBag()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Bag>();
    }

    public List<ItemSlot> GetCategory(int categoryIndex)
    {
        return itemSlots[categoryIndex];
    }

    public ItemBase GetItem(int categoryIndex, int itemIndex)
    {
        var currentSlots = GetCategory(categoryIndex);
        return currentSlots[itemIndex].ItemBase;
    }

    private ItemCategory GetCategoryByItem(ItemBase item)
    {
        if (item is ShiftGem) { return ItemCategory.ShiftGems; }
        else if (item is Trap) { return ItemCategory.Traps; }
        else if (item is Artifact) { return ItemCategory.Artifacts; }
        else { return ItemCategory.Sweets; }
    }

    public int GetItemCount(ItemBase item)
    {
        var categoryIndex = (int)GetCategoryByItem(item);
        var currentSlots = GetCategory(categoryIndex);
        var itemSlot = currentSlots.FirstOrDefault(slot => slot.ItemBase == item);

        if (itemSlot != null) { return itemSlot.Count; }
        else { return 0; }
    }

    public ItemBase UseItem(int categoryIndex, int itemIndex, Beast selectedBeast)
    {
        var item = GetItem(categoryIndex, itemIndex);
        bool itemUsed = item.Use(selectedBeast);
        if (itemUsed)
        {
            if (!item.IsReusable) { RemoveItem(item); }

            return item;
        }

        return null;
    }

    public void RemoveItem(ItemBase item, int count = 1)
    {
        var categoryIndex = (int)GetCategoryByItem(item);
        var currentSlots = GetCategory(categoryIndex);
        var itemSlot = currentSlots.FirstOrDefault(slot => slot.ItemBase == item);

        itemSlot.Count -= count;
        if (itemSlot.Count <= 0) { currentSlots.Remove(itemSlot); }

        OnUpdated?.Invoke();
    }

    public void AddItem(ItemBase item, int count = 1)
    {
        var categoryIndex = (int)GetCategoryByItem(item);
        var currentSlots = GetCategory(categoryIndex);
        var itemSlot = currentSlots.FirstOrDefault(slot => slot.ItemBase == item);
        if (itemSlot != null) { itemSlot.Count += count; }
        else
        {
            currentSlots.Add(new ItemSlot()
            {
                ItemBase = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
    }

    public bool HasItem(ItemBase item)
    {
        var categoryIndex = (int)GetCategoryByItem(item);
        var currentSlots = GetCategory(categoryIndex);
        return currentSlots.Exists(slot => slot.ItemBase == item);
    }

    public object CaptureState()
    {
        var saveData = new BagSaveData()
        {
            shiftGems = shiftGemSlots.Select(i => i.GetSaveData()).ToList(),
            traps = trapSlots.Select(i => i.GetSaveData()).ToList(),
            artifacts = artifactSlots.Select(i => i.GetSaveData()).ToList(),
            sweets = sweetSlots.Select(i => i.GetSaveData()).ToList(),
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as BagSaveData;

        shiftGemSlots = saveData.shiftGems.Select(i => new ItemSlot(i)).ToList();
        trapSlots = saveData.traps.Select(i => new ItemSlot(i)).ToList();
        artifactSlots = saveData.artifacts.Select(i => new ItemSlot(i)).ToList();
        sweetSlots = saveData.sweets.Select(i => new ItemSlot(i)).ToList();
        itemSlots = new List<List<ItemSlot>>()
        {
            shiftGemSlots,
            trapSlots,
            artifactSlots,
            sweetSlots
        };

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase itemBase;
    [SerializeField] int count;

    public ItemBase ItemBase
    {
        get { return itemBase; }
        set { itemBase = value; }
    }
    public int Count
    {
        get { return count; }
        set { count = value; }
    }

    public ItemSlot() { }

    public ItemSlot(ItemSaveData saveData)
    {
        itemBase = ItemDB.GetObjectByName(saveData.name);
        count = saveData.count;
    }

    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = itemBase.name,
            count = count
        };

        return saveData;
    }
}

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class BagSaveData
{
    public List<ItemSaveData> shiftGems;
    public List<ItemSaveData> traps;
    public List<ItemSaveData> artifacts;
    public List<ItemSaveData> sweets;
}
