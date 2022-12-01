using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPBar : MonoBehaviour
{
    [SerializeField] GameObject skillPoint;

    public bool IsUpdating { get; private set; }

    public void SetSP(float spNormalized)
    {
        skillPoint.transform.localScale = new Vector3(spNormalized, 1f);
    }

    public IEnumerator SetSPSmooth(float newSP)
    {
        IsUpdating= true;

        float currentSP = skillPoint.transform.localScale.x;
        float changeAmount = currentSP - newSP;

        while (currentSP - newSP > Mathf.Epsilon)
        {
            currentSP -= changeAmount * Time.deltaTime;
            skillPoint.transform.localScale = new Vector3(currentSP, 1f);
            yield return null;
        }

        skillPoint.transform.localScale = new Vector3(newSP, 1f);

        IsUpdating = false;
    }
}
