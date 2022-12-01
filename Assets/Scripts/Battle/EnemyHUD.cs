using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHUD : BattleHUD
{
    public override void SetHUD(Beast beast)
    {
        base.SetHUD(beast);
    }

    public override void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public override IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_beast.HP / _beast.MaxHP);
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
    }

    public override IEnumerator WaitForSPUpdate()
    {
        yield return new WaitUntil(() => spBar.IsUpdating == false);
    }

    public override IEnumerator SetExpSmooth(bool reset = false)
    {
        yield break;
    }
}
