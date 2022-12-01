using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Text dialogText;

    public IEnumerator TypeDialog(string dialog)
    {
        this.gameObject.SetActive(true);

        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / 30);
        }

        yield return new WaitForSeconds(1f);

        this.gameObject.SetActive(false);
    }
}
