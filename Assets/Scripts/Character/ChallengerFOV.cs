using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengerFOV : MonoBehaviour, ITriggerableObject
{
    public bool TriggerRepeatedly { get { return false; } }

    public void OnTriggered(PlayerController player)
    {
        player.Character.Anim.SetBool("isMoving", false);
        GameController.Instance.StartCutscene(GetComponentInParent<ChallengerController>());
    }
}
