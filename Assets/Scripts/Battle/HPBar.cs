using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject hitPoint;

    public bool IsUpdating { get; private set; }

    public void SetHP(float hpNormalized)
    {
        hitPoint.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    public IEnumerator SetHPSmooth(float newHP)
    {
        IsUpdating = true;

        float currentHP = hitPoint.transform.localScale.x;
        float changeAmount = currentHP - newHP;

        while (currentHP - newHP > Mathf.Epsilon)
        {
            currentHP -= changeAmount * Time.deltaTime;
            hitPoint.transform.localScale = new Vector3(currentHP, 1f);
            yield return null;
        }

        hitPoint.transform.localScale = new Vector3(newHP, 1f);

        IsUpdating = false;
    }
}
