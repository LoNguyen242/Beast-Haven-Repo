using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Choice : MonoBehaviour
{
    private Text choiceText;

    public Text ChoiceText { get { return choiceText; } }

    private void Awake()
    {
        choiceText = GetComponentInChildren<Text>();
    }

    public void UpdateCurrentChoice(bool selected)
    {
        choiceText.color = (selected) ? Color.magenta : Color.black;
    }
}
