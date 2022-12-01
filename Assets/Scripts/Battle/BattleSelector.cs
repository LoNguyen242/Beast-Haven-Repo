using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSelector : MonoBehaviour
{
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject skillSelector;
    [SerializeField] GameObject choiceSelector;

    [SerializeField] List<Text> actionTexts;

    [SerializeField] List<Text> skillTexts;
    [SerializeField] List<Image> skillIcons;

    [SerializeField] Text yesText;
    [SerializeField] Text noText;

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableSkillSelector(bool enabled)
    {
        skillSelector.SetActive(enabled);
    }

    public void EnableChoiceSelector(bool enabled)
    {
        choiceSelector.SetActive(enabled);
    }

    public void UpdateActionSelector(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if (i == selectedAction) { actionTexts[i].color = Color.magenta; }
            else { actionTexts[i].color = Color.black; }
        }
    }

    public void SetSkill(List<Skill> skills)
    {
        for (int i = 0; i < skillTexts.Count; i++)
        {
            if (i < skills.Count)
            {
                skillTexts[i].text = skills[i].SkillBase.Name;
                skillIcons[i].enabled = true;
                skillIcons[i].sprite = skills[i].SkillBase.Icon;
            }
            else
            {
                skillTexts[i].text = "-";
                skillIcons[i].enabled = false;
            }
        }
    }

    public void UpdateSkillSelector(int selectedSkill)
    {
        for (int i = 0; i < skillTexts.Count; i++)
        {
            if (i == selectedSkill) { skillTexts[i].color = Color.magenta; }
            else { skillTexts[i].color = Color.black; }
        }
    }

    public void UpdateChoiceSelector(bool yesSelected)
    {
        if (yesSelected)
        {
            yesText.color = Color.magenta;
            noText.color = Color.black;
        }
        else
        {
            yesText.color = Color.black;
            noText.color = Color.magenta;
        }
    }
}
