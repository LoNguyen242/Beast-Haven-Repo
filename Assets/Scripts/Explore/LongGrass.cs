using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, ITriggerableObject
{
    public bool TriggerRepeatedly { get { return true; } }

    public void OnTriggered(PlayerController player)
    {
        if (Random.Range(1, 101) <= 7.5) 
        {
            player.Character.Anim.SetBool("isMoving", false);
            GameController.Instance.StartBattle(); 
        }
    }
}
