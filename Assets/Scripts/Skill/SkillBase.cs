using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Category
{
    None,
    Physical,
    Special,
    Status
}

public enum Target
{
    Self,
    Friend,
    Foe
}

[CreateAssetMenu(fileName = "Skill", menuName = "Beasts/Create new Skill")]
public class SkillBase : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] AudioClip sfx;

    [Header("Type")]
    [SerializeField] Category category;
    [SerializeField] Element element;
    [SerializeField] Target target;

    [Header("Effects")]
    [SerializeField] SkillEffect effect;
    [SerializeField] List<SecondaryEffects> secondaryEffects;

    [Header("Properties")]
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int priority;
    [SerializeField] int sP;
    [SerializeField] int hP;
    [SerializeField] bool alwaysHit;
    [SerializeField] bool hasRecoil;

    public string Name { get { return name; } }
    public string Description { get { return description; } }
    public Sprite Icon { get { return icon; } }
    public AudioClip SFX { get { return sfx; } }

    public Category Category { get { return category; } }
    public Element Element { get { return element; } }
    public Target Target { get { return target; } }

    public SkillEffect SkillEffect { get { return effect; } }
    public List<SecondaryEffects> SecondaryEffects { get { return secondaryEffects; } }

    public int Power { get { return power; } }
    public int Accuracy { get { return accuracy; } }
    public int Priority { get { return priority; } }
    public int SP { get { return sP; } }
    public int HP { get { return hP; } }
    public bool AlwaysHit { get { return alwaysHit; } }
    public bool HasRecoil { get { return hasRecoil; } }
}

[Serializable]
public class SkillEffect
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] StatusEffectID status;

    public List<StatBoost> Boosts { get { return boosts; } }
    public StatusEffectID Status { get { return status; } }
}

[Serializable]
public class SecondaryEffects : SkillEffect
{
    [SerializeField] Target target;
    [SerializeField] int chance;

    public Target Target { get { return target; } }
    public int Chance { get { return chance; } }
}

[Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}
