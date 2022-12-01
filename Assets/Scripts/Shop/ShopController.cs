using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { Menu, Buy, Sell, Busy }

public class ShopController : MonoBehaviour
{
    [SerializeField] ShopUI shopUI;
    [SerializeField] BagUI bagUI;
    [SerializeField] CountSelectorUI countSelectorUI;

    private Shopkeeper shopkeeper;

    private Bag bag;

    private ShopState state;

    public event Action OnStartShop;
    public event Action OnFinishShop;

    public static ShopController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start()
    {
        bag = Bag.GetBag();
    }

    public IEnumerator StartTrade(Shopkeeper shopkeeper)
    {
        this.shopkeeper = shopkeeper;

        OnStartShop?.Invoke();
        yield return OpenShop();
    }

    private IEnumerator OpenShop()
    {
        state = ShopState.Menu;

        int selectecedChoice = 0;

        DialogManager.Instance.SetSpeaker(false, shopkeeper.name);
        yield return DialogManager.Instance.ShowDialogString
            ("How may I serve you?"
            , waitForInput: false
            , choices: new List<string> { "Buy", "Sell", "Quit" }
            , onChoiceSelected: (choice) => selectecedChoice = choice);

        if (selectecedChoice == 0)
        {
            state = ShopState.Buy;

            shopUI.ShowShopUI(shopkeeper.AvailableItems, BackFromBuy, (item) => StartCoroutine(BuyItem(item)));
        }
        else if (selectecedChoice == 1)
        {
            state = ShopState.Sell;

            bagUI.gameObject.SetActive(true);
        }
        else if (selectecedChoice == 2)
        {
            yield return DialogManager.Instance.ShowDialogString("Come back here whenever you need.");
            OnFinishShop?.Invoke();
            yield break;
        }
    }

    public void HandleUpdate()
    {
        if (state == ShopState.Buy) { shopUI.HandleUpdate(); }
        else if (state == ShopState.Sell)
        { bagUI.HandleUpdate(BackFromSell, (currentItem) => StartCoroutine(SellItem(currentItem))); }
    }

    private void BackFromSell()
    {
        bagUI.gameObject.SetActive(false);
        StartCoroutine(OpenShop());
    }

    private IEnumerator SellItem(ItemBase item)
    {
        state = ShopState.Busy;

        DialogManager.Instance.SetSpeaker(false, shopkeeper.name);
        if (!item.IsSellable)
        {
            yield return DialogManager.Instance.ShowDialogString("You should't sell that here!");
            state = ShopState.Sell;

            yield break;
        }

        int itemCount = bag.GetItemCount(item);
        float sellingPrice = Mathf.Round(item.Price / 1.5f);
        int countToSell = 1;
        if (itemCount > 1)
        {
            yield return DialogManager.Instance.ShowDialogString
                ("How many would you like to sell?", waitForInput: false, autoClose: false);

            yield return countSelectorUI.ShowSelector
                (itemCount, sellingPrice, (currentCount) => countToSell = currentCount);

            DialogManager.Instance.CloseDialog();
        }

        sellingPrice = sellingPrice * countToSell;

        int currentChoice = 0;

        yield return DialogManager.Instance.ShowDialogString
            ("I could give you " + sellingPrice + "G for that. Would you like?"
            , waitForInput: false
            , choices: new List<string> { "Yes", "No" }
            , onChoiceSelected: (choice) => currentChoice = choice);

        if (currentChoice == 0)
        {
            bag.RemoveItem(item, countToSell);
            Wallet.Instance.AddMoney(sellingPrice);

            DialogManager.Instance.SetSpeaker(true);
            yield return DialogManager.Instance.ShowDialogString
                ("The Protagonist turned over " + item.Name + " and earned " + sellingPrice + "G!");
            DialogManager.Instance.SetSpeaker(false, shopkeeper.name);
            yield return DialogManager.Instance.ShowDialogString("Thank you for shopping with us");
        }
        else if (currentChoice == 1)
        {
            DialogManager.Instance.SetSpeaker(false, shopkeeper.name);
            yield return DialogManager.Instance.ShowDialogString("Customer is god, they say.");
        }

        state = ShopState.Sell;
    }

    private void BackFromBuy()
    {
        shopUI.gameObject.SetActive(false);
        StartCoroutine(OpenShop());
    }

    private IEnumerator BuyItem(ItemBase item)
    {
        state = ShopState.Busy;

        DialogManager.Instance.SetSpeaker(false, shopkeeper.name);
        yield return DialogManager.Instance.ShowDialogString
            ("Have a closer look then.", waitForInput: false, autoClose: false);

        int countToBuy = 1;
        yield return countSelectorUI.ShowSelector
            (99, item.Price, (currentCount) => countToBuy = currentCount);

        DialogManager.Instance.CloseDialog();

        float buyingPrice = item.Price * countToBuy;
        if (Wallet.Instance.HasEnoughMoney(buyingPrice))
        {
            int currentChoice = 0;

            yield return DialogManager.Instance.ShowDialogString
                ("I would take you " + buyingPrice + "G for that. Would you like?"
                , waitForInput: false
                , choices: new List<string> { "Yes", "No" }
                , onChoiceSelected: (choice) => currentChoice = choice);

            if (currentChoice == 0)
            {
                bag.AddItem(item, countToBuy);
                Wallet.Instance.TakeMoney(buyingPrice);

                DialogManager.Instance.SetSpeaker(true);
                yield return DialogManager.Instance.ShowDialogString
                    ("The Protagonist turned over " + buyingPrice + "G and earned " + item.Name + "!");
                DialogManager.Instance.SetSpeaker(false, shopkeeper.name);
                yield return DialogManager.Instance.ShowDialogString("Thank you for shopping with us");
            }
            else if (currentChoice == 1)
            {
                DialogManager.Instance.SetSpeaker(false, shopkeeper.name);
                yield return DialogManager.Instance.ShowDialogString("Customer is god, they say.");
            }
        }
        else
        {
            DialogManager.Instance.SetSpeaker(false, shopkeeper.name);
            yield return DialogManager.Instance.ShowDialogString
                ("I'm afraid you don't have enough gold for that.");
        }

        state = ShopState.Buy;
    }
}
