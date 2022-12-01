using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Sweet")]
public class Sweet : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("SP")]
    [SerializeField] int spAmount;
    [SerializeField] bool restoreMaxSP;

    [Header("Status Effect")]
    [SerializeField] StatusEffectID status;
    [SerializeField] bool cureAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Beast beast)
    {
        //Revive
        if (revive || maxRevive)
        {
            if (beast.HP > 0) { return false; }

            if (revive) { beast.UpdateHP(-beast.MaxHP / 4); }
            else if (maxRevive) { beast.UpdateHP(-beast.MaxHP); }

            beast.EndStatus();

            return true;
        }

        if (beast.HP <= 0) { return false; }

        //Restore HP
        if (restoreMaxHP || hpAmount > 0) 
        {
            if (beast.HP == beast.MaxHP) { return false; }

            if (restoreMaxHP) { beast.UpdateHP(-beast.MaxHP); }
            else { beast.UpdateHP(-hpAmount); }
        }

        //Restore SP
        if (restoreMaxSP || spAmount > 0)
        {
            if (beast.SP == beast.MaxSP) { return false; }

            if (restoreMaxSP) { beast.UpdateSP(-beast.MaxSP); }
            else { beast.UpdateSP(-spAmount); }
        }

        //Cure Status Effect
        if (cureAllStatus || status != StatusEffectID.None)
        {
            if (beast.Status == null) { return false; }
            
            if (cureAllStatus) { beast.EndStatus(); }
            else
            {
                if (beast.Status.ID == status) { beast.EndStatus(); }
                else { return false; }
            }
        }

        return true;
    }
}
