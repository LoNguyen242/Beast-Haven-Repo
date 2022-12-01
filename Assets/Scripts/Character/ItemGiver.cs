using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour, ISavable
{
    [SerializeField] float money;

    [SerializeField] ItemBase item;
    [SerializeField] int count = 1;
    [SerializeField] Dialog dialog;

    public bool Given { get; set; } = false;

    public IEnumerator GiveItem(PlayerController player)
    {
        DialogManager.Instance.SetSpeaker(false, gameObject.name);
        yield return DialogManager.Instance.ShowDialog(dialog);

        if (money > 0)
        {
            Wallet.Instance.AddMoney(money);

            AudioManager.Instance.PlaySFX(AudioID.Obtain, true);
            DialogManager.Instance.SetSpeaker(true);
            yield return DialogManager.Instance.ShowDialogString
                ("The Protagonist received " + money + "G!");
        }

        if (item != null)
        {
            player.GetComponent<Bag>().AddItem(item, count);

            AudioManager.Instance.PlaySFX(AudioID.Obtain, true);
            DialogManager.Instance.SetSpeaker(true);
            yield return DialogManager.Instance.ShowDialogString
                ("The Protagonist received " + count + " " + item.Name + "!");
        }

        Given = true;
    }

    public bool CanBeGiven()
    {
        return !Given && count > 0;
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
