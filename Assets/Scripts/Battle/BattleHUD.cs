using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BattleHUD : MonoBehaviour
{
    [SerializeField] protected HPBar hpBar;
    [SerializeField] protected SPBar spBar;

    [SerializeField] protected Text nameText;
    [SerializeField] protected Text lvText;

    [SerializeField] protected Image statusImage;

    [SerializeField] protected Sprite brnIcon;
    [SerializeField] protected Sprite frzIcon;
    [SerializeField] protected Sprite psnIcon;
    [SerializeField] protected Sprite shkIcon;
    [SerializeField] protected Sprite blsIcon;
    [SerializeField] protected Sprite crsIcon;

    protected Beast _beast;

    protected Dictionary<StatusEffectID, Sprite> statusIcons;

    public virtual void SetHUD(Beast beast)
    {
        if (_beast != null)
        {
            _beast.OnStatusChanged -= SetStatus;
            _beast.OnHPChanged -= UpdateHP;
            _beast.OnSPChanged -= UpdateSP;
        }

        _beast = beast;

        hpBar.SetHP((float)beast.HP / beast.MaxHP);
        spBar.SetSP((float)beast.SP / beast.MaxSP);

        nameText.text = beast.BeastBase.Name;
        SetLevel();

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

        _beast.OnStatusChanged += SetStatus;
        _beast.OnHPChanged += UpdateHP;
        _beast.OnSPChanged += UpdateSP;
    }

    public void SetLevel()
    {
        lvText.text = "Lv" + _beast.Level;
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

    public void ClearHUD()
    {
        if (_beast != null)
        {
            _beast.OnStatusChanged -= SetStatus;
            _beast.OnHPChanged -= UpdateHP;
            _beast.OnSPChanged -= UpdateSP;
        }
    }

    public abstract void UpdateHP();
        
    public abstract void UpdateSP();

    public abstract IEnumerator UpdateHPAsync();

    public abstract IEnumerator WaitForHPUpdate();

    public abstract IEnumerator UpdateSPAsync();

    public abstract IEnumerator WaitForSPUpdate();

    public abstract IEnumerator SetExpSmooth(bool reset = false);
}
