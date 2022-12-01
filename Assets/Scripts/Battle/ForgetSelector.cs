using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForgetSelector : MonoBehaviour
{
    [SerializeField] List<Text> forgetTexts;

    private int currentForget;

    public void EnableForgetSelector(bool enabled)
    {
        this.gameObject.SetActive(enabled);
    }

    public void SetForgetNames(List<SkillBase> currentSkills)
    {
        for (int i = 0; i < currentSkills.Count; i++)
        {
            forgetTexts[i].text = currentSkills[i].Name;
        }
    }

    public void UpdateForgetSelector(int forgetSelected)
    {
        for (int i = 0; i < forgetTexts.Count; i++)
        {
            if (i == forgetSelected) { forgetTexts[i].color = Color.magenta; }
            else { forgetTexts[i].color = Color.black; }
        }
    }

    public void HandleForgetSelection(Action<int> onChanged, Action<int> onSelected, Action onBack)
    {
        int prevForget = currentForget;

        if (Input.GetKeyDown(KeyCode.DownArrow)) { currentForget++; }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) { currentForget--; }

        if (currentForget > 3) { currentForget = 0; }
        else if (currentForget < 0) { currentForget = 3; }
        currentForget = Mathf.Clamp(currentForget, 0, 3);

        if (prevForget != currentForget) { onChanged?.Invoke(currentForget); }

        if (Input.GetKeyDown(KeyCode.Z)) { onSelected?.Invoke(currentForget); }
        else if (Input.GetKeyDown(KeyCode.X)) { onBack?.Invoke(); }
    }
}
