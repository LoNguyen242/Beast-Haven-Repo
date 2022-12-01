using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public enum StatusEffectID
{
    None,
    BRN,
    FRZ,
    PSN,
    SHK,
    BLS,
    CRS,
}

public class StatusEffectDB
{
    public static void Init()
    {
        foreach (var kvp in StatusEffects)
        {
            var statusEffectID = kvp.Key;
            var statusEffect = kvp.Value;

            statusEffect.ID = statusEffectID;
        }
    }

    public static Dictionary<StatusEffectID, StatusEffect> StatusEffects { get; set; }
        = new Dictionary<StatusEffectID, StatusEffect>()
        {
            {
                StatusEffectID.BRN,
                new StatusEffect()
                {
                    Name = "Burn",
                    Message = " has been burnt!",
                    OnStartTurn = SetStatusTime,
                    OnAfterTurn = BurnEffect
                }
            },
            {
                StatusEffectID.FRZ,
                new StatusEffect()
                {
                    Name = "Freeze",
                    Message = " has been frozen!",
                    OnStartTurn = SetStatusTime,
                    OnBeforeTurn = FreezeEffect
                }
            },
            {
                StatusEffectID.PSN,
                new StatusEffect()
                {
                    Name = "Poison",
                    Message = " has been poisoned!",
                    OnStartTurn = SetStatusTime,
                    OnAfterTurn = PoisonEffect
                }
            },
            {
                StatusEffectID.SHK,
                new StatusEffect()
                {
                    Name = "Shock",
                    Message = " has been shocked!",
                    OnStartTurn = SetStatusTime,
                    OnBeforeTurn = ShockEffect
                }
            },
            {
                StatusEffectID.BLS,
                new StatusEffect()
                {
                    Name = "Blessing",
                    Message = " has received blessing!",
                    OnStartTurn = SetStatusTime,
                    OnAfterTurn = BlessingEffect
                }
            },
            {
                StatusEffectID.CRS,
                new StatusEffect()
                {
                    Name = "Curse",
                    Message = " has been cursed!",
                    OnStartTurn = SetStatusTime,
                    OnBeforeTurn = CurseEffect
                }
            }
        };

    static void BurnEffect(Beast beast)
    {
        int value = (beast.Attack > beast.MagicAttack) ? beast.Attack : beast.MagicAttack;
        beast.UpdateHP(value / 16);
        beast.StatusChanges.Enqueue(beast.BeastBase.Name + " was hurt by its burn!");

        UpdateStatusTime(beast);
    }

    static bool FreezeEffect(Beast beast)
    {
        UpdateStatusTime(beast);

        if (Random.Range(1, 5) == 1)
        {
            beast.EndStatus();
            beast.StatusChanges.Enqueue(beast.BeastBase.Name + " could move freely!");
            return true;
        }

        beast.StatusChanges.Enqueue(beast.BeastBase.Name + " couldn't move!");
        return false;
    }

    static void PoisonEffect(Beast beast)
    {
        beast.UpdateHP(beast.MaxHP / 16);
        beast.UpdateSP(beast.MaxSP / 16);
        beast.StatusChanges.Enqueue(beast.BeastBase.Name + " was weakened by its poison!");
    }

    static bool ShockEffect(Beast beast)
    {
        UpdateStatusTime(beast);

        if (Random.Range(1, 5) == 1)
        {
            beast.StatusChanges.Enqueue(beast.BeastBase.Name + " couldn't move!");
            return false;
        }

        return true;
    }

    static void BlessingEffect(Beast beast)
    {
        UpdateStatusTime(beast);
    }

    static bool CurseEffect(Beast beast)
    {
        UpdateStatusTime(beast);

        return true;
    }

    static void SetStatusTime(Beast beast)
    {
        beast.StatusTime = 3;
    }

    static void UpdateStatusTime(Beast beast)
    {
        beast.StatusTime--;
        if (beast.StatusTime <= 0)
        {
            beast.EndStatus();
            beast.StatusChanges.Enqueue(beast.BeastBase.Name + "'s status effect ran out!");
        }
    }

    public static float GetStatusBonus(StatusEffect statusEffect)
    {
        if (statusEffect == null) { return 1f; }
        else { return 2f; }
    }
}
