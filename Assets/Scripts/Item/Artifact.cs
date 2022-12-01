using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Artifact")]
public class Artifact : ItemBase
{
    [Header("Artifact")]
    [SerializeField] SkillBase skill;
    [SerializeField] bool isUnique;

    public SkillBase Skill { get { return skill; } }
    public bool IsUnique { get { return isUnique; } }

    public override bool IsReusable { get { return !isUnique; } }
    public override bool CanUseInsideBattle { get { return false; } }

    public override bool Use(Beast beast)
    {
        return beast.HasSkill(skill);
    }

    public bool CheckForLearn(Beast beast)
    {
        return beast.BeastBase.ArtifactSkills.Contains(skill);
    }
}
