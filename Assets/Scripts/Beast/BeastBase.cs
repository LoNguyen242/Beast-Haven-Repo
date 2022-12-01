using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element
{
    Null,
    Neutral,
    Fire,
    Water,
    Nature,
    Ice,
    Earth,
    Electric,
    Wind,
    Light,
    Dark
}

public enum Stat
{
    ATK,
    DEF,
    SATK,
    SDEF,
    SPD,
    ACC,
    EVA
}

[CreateAssetMenu(fileName = "Beast", menuName = "Beasts/Create new Beast")]
public class BeastBase : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] string name;
    [SerializeField] string title;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite battleSprite;

    [Header("Affinities")]
    [SerializeField] Element[] weakElements = new Element[2];
    [SerializeField] Element[] resistElements = new Element[2];
    [SerializeField] Element[] immuneElements = new Element[1];

    [Header("Stats")]
    [SerializeField] int maxHP;
    [SerializeField] int maxSP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int specialAttack;
    [SerializeField] int specialDefense;
    [SerializeField] int speed;
    [SerializeField] int captureRate = 100;
    [SerializeField] int baseExp;

    [Header("Skills")]
    [SerializeField] List<LearnableSkill> learnableSkills;
    [SerializeField] List<SkillBase> artifactSkills;

    [Header("Power Shift")]
    [SerializeField] List<PowerShift> powerShifts;

    public string Name { get { return name; } }
    public string Title { get { return title; } }
    public string Description { get { return description; } }
    public Sprite BattleSprite { get { return battleSprite; } }

    public Element[] WeakElements { get { return weakElements; } }
    public Element[] ResistElements { get { return resistElements; } }
    public Element[] ImmuneElements { get { return immuneElements; } }

    public int MaxHP { get { return maxHP; } }
    public int MaxSP { get { return maxSP; } }
    public int Attack { get { return attack; } }
    public int Defense { get { return defense; } }
    public int SpecialAttack { get { return specialAttack; } }
    public int SpecialDefense { get { return specialDefense; } }
    public int Speed { get { return speed; } }
    public int CaptureRate { get { return captureRate; } }
    public int BaseExp { get { return baseExp; } }

    public List<LearnableSkill> LearnableSkills { get { return learnableSkills; } }
    public List<SkillBase> ArtifactSkills { get { return artifactSkills; } }

    public List<PowerShift> PowerShifts { get { return powerShifts; } }

    public int GetExpForLevel(int level)
    {
        return (level * level * level);
    }
}

[Serializable]
public class LearnableSkill
{
    [SerializeField] SkillBase skillBase;
    [SerializeField] int level;

    public SkillBase SkillBase { get { return skillBase; } }
    public int Level { get { return level; } }
}

[Serializable]
public class PowerShift
{
    [SerializeField] BeastBase nextStage;
    [SerializeField] ShiftGem requiredItem;
    [SerializeField] int requiredLevel;

    public BeastBase NextStage { get { return nextStage; } }
    public ShiftGem RequiredItem { get { return requiredItem; } }
    public int RequiredLevel { get { return requiredLevel; } }
}
