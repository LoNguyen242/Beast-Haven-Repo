using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalletUI : MonoBehaviour
{
    [SerializeField] Text moneyText;

    private void Start()
    {
        SetMoneyText();

        Wallet.Instance.OnMoneyChanged += SetMoneyText;
    }

    private void SetMoneyText()
    {
        moneyText.text = "G" + Wallet.Instance.Money;
    }
}
