using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMember : MonoBehaviour
{
    [SerializeField] HPBar hpBar;
    [SerializeField] SPBar spBar;

    [SerializeField] Text hpText;
    [SerializeField] Text spText;
    [SerializeField] Text nameText;
    [SerializeField] Text lvText;
    [SerializeField] Text messageText;

    [SerializeField] Image statusImage;

    [SerializeField] Sprite brnIcon;
    [SerializeField] Sprite frzIcon;
    [SerializeField] Sprite psnIcon;
    [SerializeField] Sprite shkIcon;
    [SerializeField] Sprite blsIcon;
    [SerializeField] Sprite crsIcon;

    private Beast _beast;

    private Dictionary<StatusEffectID, Sprite> statusIcons;

    public void Init(Beast beast)
    {
        _beast = beast;

        UpdateMember();
        SetMessage("");

        _beast.OnStatusChanged += SetStatus;
        _beast.OnHPChanged += () =>
        {
            hpBar.SetHP((float)beast.HP / beast.MaxHP);
            hpText.text = beast.HP.ToString();
        };
        _beast.OnSPChanged += () =>
        {
            spBar.SetSP((float)beast.SP / beast.MaxSP);
            spText.text = beast.SP.ToString();
        };
        _beast.OnLevelChanged += () => { lvText.text = "Lv" + _beast.Level; };
    }

    public void UpdateMember()
    {
        hpBar.SetHP((float)_beast.HP / _beast.MaxHP);
        spBar.SetSP((float)_beast.SP / _beast.MaxSP);

        hpText.text = _beast.HP.ToString();
        spText.text = _beast.SP.ToString();
        nameText.text = _beast.BeastBase.Name;
        lvText.text = "Lv" + _beast.Level;

        statusIcons = new Dictionary<StatusEffectID, Sprite>()
        {

            { StatusEffectID.BRN, brnIcon },
            { StatusEffectID.FRZ, frzIcon },
            { StatusEffectID.PSN, psnIcon },
            { StatusEffectID.SHK, shkIcon },
            { StatusEffectID.CRS, crsIcon },
            { StatusEffectID.BLS, blsIcon },
        };
        SetStatus();
    }

    public void UpdateCurrentMember(bool selected)
    {
        if (selected) { nameText.color = Color.magenta; }
        else { nameText.color = Color.black; }
    }

    public void SetStatus()
    {
        if (_beast.Status == null)
        {
            statusImage.sprite = null;
            statusImage.gameObject.SetActive(false);

        }
        else
        {
            statusImage.gameObject.SetActive(true);
            statusImage.sprite = statusIcons[_beast.Status.ID];
        }
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
