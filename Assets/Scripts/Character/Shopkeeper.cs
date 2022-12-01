using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shopkeeper : MonoBehaviour
{
    [SerializeField] List<ItemBase> availableItems;

    public List<ItemBase> AvailableItems { get { return availableItems; } }

    public IEnumerator Trade()
    {
        yield return ShopController.Instance.StartTrade(this);
    }
}
