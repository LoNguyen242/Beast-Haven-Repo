using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceBox : MonoBehaviour
{
    [SerializeField] Choice choicePrefab;

    private List<Choice> choiceTexts;
    private int currentChoice;
    private bool choiceSelected = false;

    public IEnumerator ShowChoices(List<string> choices, Action<int> onChoiceSelected)
    {
        currentChoice = 0;
        choiceSelected = false;

        gameObject.SetActive(true);

        foreach (Transform child in transform) { Destroy(child.gameObject); }

        choiceTexts = new List<Choice>();
        foreach (var choice in choices)
        {
            var choiceTextObj = Instantiate(choicePrefab, transform);
            choiceTextObj.ChoiceText.text = choice;
            choiceTexts.Add(choiceTextObj);
        }

        yield return new WaitUntil(() => choiceSelected == true);

        onChoiceSelected?.Invoke(currentChoice);

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow)) { currentChoice++; }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) { currentChoice--; }

        if (currentChoice > choiceTexts.Count - 1) { currentChoice = 0; }
        else if (currentChoice < 0) { currentChoice = choiceTexts.Count - 1; }
        currentChoice = Mathf.Clamp(currentChoice, 0, choiceTexts.Count - 1);

        for (int i = 0; i < choiceTexts.Count; i++) { choiceTexts[i].UpdateCurrentChoice(i == currentChoice); }

        if (Input.GetKeyDown(KeyCode.Z)) { choiceSelected = true; }
    }
}
