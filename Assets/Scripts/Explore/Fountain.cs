using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fountain : MonoBehaviour, IInteractableObject
{
    [SerializeField] Dialog dialog;

    public IEnumerator OnInteracted(Transform player)
    {
        int currentChoice = 0;

        DialogManager.Instance.SetSpeaker(true);
        yield return DialogManager.Instance.ShowDialog
            (dialog, new List<string>() { "Yes", "No" }, (choice) => currentChoice = choice);

        if (currentChoice == 0) 
        {
            yield return Fader.Instance.FadeIn(0.5f);
            yield return new WaitForSeconds(1f);

            var playerParty = player.GetComponent<BeastParty>();
            playerParty.Beasts.ForEach(b => b.Heal());
            playerParty.UpdateParty();

            yield return Fader.Instance.FadeOut(0.5f);
            yield return DialogManager.Instance.ShowDialogString("May The Stranger be with you!");
        }
        else if (currentChoice == 1)
        {
            yield return DialogManager.Instance.ShowDialogString("May The Stranger be with you!");
        }
    }
}
