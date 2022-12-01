using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    private PartyMember[] memberSlots;
    private BeastParty playerParty;
    private List<Beast> beasts;

    [SerializeField] Image beastImage;
    [SerializeField] Text beastText;

    private int currentMember;
    public Beast CurrentMember { get { return beasts[currentMember]; } }

    public BattleState? CalledFrom { get; set; }

    public void SetParty()
    {
        memberSlots = GetComponentsInChildren<PartyMember>(true);
        playerParty = BeastParty.GetPlayerParty();
        UpdatePartyScreen();

        playerParty.OnUpdated += UpdatePartyScreen;
    }

    public void UpdatePartyScreen()
    {
        beasts = playerParty.Beasts;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < beasts.Count) 
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(beasts[i]);
            }
            else { memberSlots[i].gameObject.SetActive(false); }
        }

        UpdatePartyMember(currentMember);
    }

    public void HandlePartyScreen(Action onSelected, Action onBack)
    {
        int prevMember = currentMember;

        if (Input.GetKeyDown(KeyCode.DownArrow)) { currentMember++; }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) { currentMember--; }

        if (currentMember > beasts.Count - 1) { currentMember = 0; }
        else if (currentMember < 0) { currentMember = beasts.Count - 1; }
        currentMember = Mathf.Clamp(currentMember, 0, beasts.Count - 1);

        if (prevMember != currentMember) { UpdatePartyMember(currentMember); }

        if (Input.GetKeyDown(KeyCode.Z)) { onSelected?.Invoke(); }
        else if (Input.GetKeyDown(KeyCode.X)) { onBack?.Invoke(); }
    }

    public void UpdatePartyMember(int selectedMember)
    {
        for (int i = 0; i < beasts.Count; i++)
        {
            if (i == selectedMember) 
            { 
                memberSlots[i].UpdateCurrentMember(true);

                beastImage.sprite = beasts[currentMember].BeastBase.BattleSprite;
                var size = beastImage.GetComponent<RectTransform>();
                size.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, beastImage.sprite.rect.height * 1.5f);
                size.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, beastImage.sprite.rect.width * 1.5f);

                beastText.text = beasts[currentMember].BeastBase.Name + "\n" + beasts[currentMember].BeastBase.Title;
            }
            else { memberSlots[i].UpdateCurrentMember(false); }
        }
    }

    public void ShowMessage(Artifact artifact)
    {
        for (int i = 0; i < beasts.Count; i++)
        {
            string message = artifact.CheckForLearn(beasts[i]) ? "Able!" : "Not Able!";
            memberSlots[i].SetMessage(message);
        }
    }

    public void ShowMessage(ShiftGem shiftGem)
    {
        for (int i = 0; i < beasts.Count; i++)
        {
            string message = beasts[i].CheckForPowerShift(shiftGem) != null ? "Able!" : "Not Able!";
            memberSlots[i].SetMessage(message);
        }
    }

    public void ClearMessage()
    {
        for (int i = 0; i < beasts.Count; i++) { memberSlots[i].SetMessage(""); }
    }
}
