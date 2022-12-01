using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : BattleHUD
{
    [SerializeField] GameObject expBar;

    [SerializeField] Text hpText;
    [SerializeField] Text spText;

    public override void SetHUD(Beast beast)
    {
        base.SetHUD(beast);

        SetExp();

        hpText.text = beast.HP.ToString();
        spText.text = beast.SP.ToString();
    }

    public override void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public override IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_beast.HP / _beast.MaxHP);
        hpText.text = _beast.HP.ToString();
    }

    public override IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }

    public override void UpdateSP()
    {
        StartCoroutine(UpdateSPAsync());
    }

    public override IEnumerator UpdateSPAsync()
    {
        yield return spBar.SetSPSmooth((float)_beast.SP / _beast.MaxSP);
        spText.text = _beast.SP.ToString();
    }

    public override IEnumerator WaitForSPUpdate()
    {
        yield return new WaitUntil(() => spBar.IsUpdating == false);
    }

    private float GetNormalizedExp()
    {
        int currLevelExp = _beast.BeastBase.GetExpForLevel(_beast.Level);
        int nextLevelExp = _beast.BeastBase.GetExpForLevel(_beast.Level + 1);
        float normalizedExp = (float)(_beast.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    private void SetExp()
    {
        if (expBar == null) { return; }

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1f, 1f);
    }

    public override IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) { yield break; }

        if (reset) { expBar.transform.localScale = new Vector3(0f, 1f, 1f); }

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1f).WaitForCompletion();
    }
}
