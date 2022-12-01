using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetails : MonoBehaviour
{
    [SerializeField] Image typeIcon;

    [SerializeField] Text nameText;
    [SerializeField] Text costText;
    [SerializeField] Text catText;
    [SerializeField] Text powerText;
    [SerializeField] Text accuracyText;
    [SerializeField] Text desText;

    public void EnableSkillDetails(bool enabled)
    {
        gameObject.SetActive(enabled);
    }

    public void UpdateSkillDetails(Skill skill)
    {
        typeIcon.sprite = skill.SkillBase.Icon;
        nameText.text = skill.SkillBase.Name;

        int cost = 0;
        if (skill.SkillBase.SP != 0)
        {
            cost = skill.SkillBase.SP;
            costText.text = cost + " SP";
        }
        if (skill.SkillBase.HP != 0)
        {
            cost = skill.SkillBase.HP;
            costText.text = cost + " HP";
        }

        catText.text = skill.SkillBase.Category.ToString(); ;
        powerText.text = skill.SkillBase.Power.ToString();
        accuracyText.text = skill.SkillBase.Accuracy.ToString();
        desText.text = skill.SkillBase.Description;
    }
}
