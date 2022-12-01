using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] float price;
    [SerializeField] bool isSellable;

    public string Name { get { return name; } }
    public string Description { get { return description; } }
    public Sprite Icon { get { return icon; } }
    public float Price { get { return price; } }
    public bool IsSellable { get { return isSellable; } }

    public virtual bool IsReusable { get { return false; } }
    public virtual bool CanUseInsideBattle { get { return true; } }
    public virtual bool CanUseOutsideBattle { get { return true; } }

    public virtual bool Use(Beast beast)
    {
        return false;
    }
}
