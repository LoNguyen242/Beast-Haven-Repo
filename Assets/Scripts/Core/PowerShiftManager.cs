using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerShiftManager : MonoBehaviour
{
    [SerializeField] GameObject powerShiftUI;
    [SerializeField] Image beastImage;

    [SerializeField] AudioClip powerShiftMusic;

    public event Action OnStartShift;
    public event Action OnCompleteShift;

    public static PowerShiftManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    public IEnumerator Shift(Beast beast, PowerShift powerShift)
    {
        OnStartShift?.Invoke();

        powerShiftUI.SetActive(true);
        beastImage.sprite = beast.BeastBase.BattleSprite;

        var size = beastImage.GetComponent<RectTransform>();
        size.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, beastImage.sprite.rect.height * 1.5f);
        size.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, beastImage.sprite.rect.width * 1.5f);

        AudioManager.Instance.PlayMusic(powerShiftMusic);
        DialogManager.Instance.SetSpeaker(true);
        yield return DialogManager.Instance.ShowDialogString(beast.BeastBase.Name + " is limit breaking!");

        var oldStage = beast.BeastBase;

        beast.ShiftIntoNextStage(powerShift);
        beastImage.sprite = beast.BeastBase.BattleSprite;
        size.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, beastImage.sprite.rect.height * 1.5f);
        size.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, beastImage.sprite.rect.width * 1.5f);

        yield return DialogManager.Instance.ShowDialogString
            (oldStage.Name + " has reached a new stage, " + powerShift.NextStage.Name + "!");

        powerShiftUI.SetActive(false);

        OnCompleteShift?.Invoke();
    }
}
