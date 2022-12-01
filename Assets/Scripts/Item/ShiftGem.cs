using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Shift Gem")]
public class ShiftGem : ItemBase
{
    [SerializeField] bool isUnique;

    public override bool IsReusable { get { return !isUnique; } }

    public override bool Use(Beast beast)
    {
        return true;
    }
}
