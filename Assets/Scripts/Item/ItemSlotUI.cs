using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text countText;

    private RectTransform rectTransform;

    public Text NameText { get { return nameText; } }
    public Text CountText { get { return countText; } }

    public float Height { get { return rectTransform.rect.height; } }

    public void SetNameAndCount(ItemSlot itemSlot)
    {
        rectTransform = GetComponent<RectTransform>();

        nameText.text = itemSlot.ItemBase.Name;
        countText.text = itemSlot.Count.ToString();
    }

    public void SetNameAndPrice(ItemBase item)
    {
        rectTransform = GetComponent<RectTransform>();

        nameText.text = item.Name;
        countText.text = "G" + item.Price;
    }
}
