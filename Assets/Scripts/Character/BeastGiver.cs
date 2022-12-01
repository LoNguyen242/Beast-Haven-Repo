using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastGiver : MonoBehaviour, ISavable
{
    [SerializeField] Beast beast;
    [SerializeField] Dialog dialog;

    public bool Given { get; set; } = false;

    public IEnumerator GiveBeast(PlayerController player)
    {
        DialogManager.Instance.SetSpeaker(false, gameObject.name);
        yield return DialogManager.Instance.ShowDialog(dialog);

        beast.Init();
        player.GetComponent<BeastParty>().AddBeast(beast);

        AudioManager.Instance.PlaySFX(AudioID.Obtain, true);
        DialogManager.Instance.SetSpeaker(true);
        string dialogAfter = "The Protagonist received " + beast.BeastBase.Name + "!";
        yield return DialogManager.Instance.ShowDialogString(dialogAfter);

        Given = true;
    }

    public bool CanBeGiven()
    {
        return beast != null && !Given;
    }

    public object CaptureState()
    {
        return Given;
    }

    public void RestoreState(object state)
    {
        Given = (bool)state;
    }
}
