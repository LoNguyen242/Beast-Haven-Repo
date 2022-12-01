using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour, IInteractableObject, ISavable
{
    [SerializeField] int money;

    [SerializeField] ItemBase item;
    [SerializeField] int count = 1;

    public bool Opened { get; set; } = false;

    public IEnumerator OnInteracted(Transform player)
    {
        if (!Opened)
        {
            DialogManager.Instance.SetSpeaker(true);
            yield return DialogManager.Instance.ShowDialogString
                ("There is nothing in it... ");

            if (money > 0)
            {
                Wallet.Instance.AddMoney(money);

                AudioManager.Instance.PlaySFX(AudioID.Obtain, true);
                yield return DialogManager.Instance.ShowDialogString
                    ("The Protagonist found " + money + "G!");

                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<BoxCollider2D>().enabled = false;
            }

            if (item != null)
            {
                player.GetComponent<Bag>().AddItem(item, count);

                AudioManager.Instance.PlaySFX(AudioID.Obtain, true);
                yield return DialogManager.Instance.ShowDialogString
                    ("The Protagonist found " + count + " " + item.Name + "!");

                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<BoxCollider2D>().enabled = false;
            }

            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
            Opened = true;
        }
    }

    public object CaptureState()
    {
        return Opened;
    }

    public void RestoreState(object state)
    {
        Opened = (bool)state;
        if (Opened)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
