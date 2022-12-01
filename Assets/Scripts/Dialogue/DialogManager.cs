using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] GameObject speaker;

    [SerializeField] ChoiceBox choiceBox;

    public event Action OnOpenDialog;
    public event Action OnDialogFinished;

    public bool IsShowing { get; private set; }

    public static DialogManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    public IEnumerator ShowDialog(Dialog dialog, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        yield return new WaitForEndOfFrame();

        OnOpenDialog?.Invoke();

        IsShowing = true;
        dialogBox.SetActive(true);

        foreach (var line in dialog.Lines)
        {
            AudioManager.Instance.PlaySFX(AudioID.UISelect);
            yield return TypeDialog(line);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }

        if (choices != null && choices.Count > 1) { yield return choiceBox.ShowChoices(choices, onChoiceSelected); }

        CloseDialog();

        OnDialogFinished?.Invoke();
    }

    public IEnumerator ShowDialogString
        (String dialog, bool waitForInput = true, bool autoClose = true, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        OnOpenDialog?.Invoke();

        IsShowing = true;
        dialogBox.SetActive(true);

        AudioManager.Instance.PlaySFX(AudioID.UISelect);
        yield return TypeDialog(dialog);
        if (waitForInput) { yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z)); }

        if (choices != null && choices.Count > 1) { yield return choiceBox.ShowChoices(choices, onChoiceSelected); }

        if (autoClose) { CloseDialog(); }

        OnDialogFinished?.Invoke();
    }

    public IEnumerator TypeDialog(string line)
    {
        dialogText.text = "";

        foreach (var letter in line.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / 30);
        }
    }

    public void CloseDialog()
    {
        dialogBox.SetActive(false);
        IsShowing = false;
    }

    public void SetSpeaker(bool isNarrated = true, string name = null)
    {
        if (isNarrated) 
        {
            speaker.gameObject.SetActive(false);
            return;
        }

        speaker.gameObject.SetActive(true);
        speaker.GetComponentInChildren<Text>().text = name;
    }
}
