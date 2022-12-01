using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<BeastEncounterRecord> wildBeasts;
    [HideInInspector]
    [SerializeField] int totalChance = 0;

    private void OnValidate()
    {
        totalChance = 0;

        foreach (var record in wildBeasts)
        {
            record.ChanceLower = totalChance;
            record.ChanceUpper = totalChance + record.chancePercen;

            totalChance += record.chancePercen;
        }
    }

    public Beast GetRandomWildBeast()
    {
        int random = UnityEngine.Random.Range(1, 101);
        var beastRecord = wildBeasts.First(b => random >= b.ChanceLower && random <= b.ChanceUpper);

        var levelRange = beastRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : UnityEngine.Random.Range(levelRange.x, levelRange.y + 1);

        var wildBeast = new Beast(beastRecord.beastBase, level);
        wildBeast.Init();
        return wildBeast;
    }
}

[Serializable]
public class BeastEncounterRecord
{
    public BeastBase beastBase;
    public Vector2Int levelRange;
    public int chancePercen;

    public int ChanceLower { get; set; }
    public int ChanceUpper { get; set; }
}
