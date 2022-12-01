using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public SkillBase SkillBase { get; set; }
    public int SP { get; set; }
    public int HP { get; set; }

    public Skill(SkillBase bBase)
    {
        SkillBase = bBase;
        SP = bBase.SP;
        HP = bBase.HP;
    }

    public Skill(SkillSaveData saveData)
    {
        SkillBase = SkillDB.GetObjectByName(saveData.name);
    }

    public SkillSaveData GetSaveData() 
    {
        var saveData = new SkillSaveData()
        {
            name = SkillBase.name
        };

        return saveData;
    }
}

[Serializable]
public class SkillSaveData
{
    public string name;
}
