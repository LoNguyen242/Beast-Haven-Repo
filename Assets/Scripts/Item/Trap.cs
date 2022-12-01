using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Trap")]
public class Trap : ItemBase
{
    [Header("Trap")]
    [SerializeField] float captureRateModifier = 1f;

    public float CaptureRateModifier { get { return captureRateModifier; } }

    public override bool CanUseOutsideBattle { get { return false; } }

    public override bool Use(Beast beast)
    {
        return true;
    }
}
