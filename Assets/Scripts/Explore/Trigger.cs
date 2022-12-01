using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour, ITriggerableObject
{
    [SerializeField] Dialog dialog;

    public bool TriggerRepeatedly { get { return false; } }

    public void OnTriggered(PlayerController player)
    {
        player.Character.Anim.SetBool("isMoving", false);

        DialogManager.Instance.SetSpeaker(true);
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
    }
}
