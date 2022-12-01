using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;
    private List<Text> menuLabels;

    private int currentLabel;

    public event Action<int> OnSelected;
    public event Action OnBack;

    private void Awake()
    {
        menuLabels = menu.GetComponentsInChildren<Text>().ToList();
    }

    public void OpenMenu()
    {
        currentLabel = 0;

        menu.SetActive(true);
        UpdateLabelSelection();
    }

    public void HandleUpdate()
    {
        int prevSelected = currentLabel;

        if (Input.GetKeyDown(KeyCode.RightArrow)) { currentLabel++; }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) { currentLabel--; }

        if (currentLabel > menuLabels.Count - 1) { currentLabel = 0; }
        else if (currentLabel < 0) { currentLabel = menuLabels.Count - 1; }
        currentLabel = Mathf.Clamp(currentLabel,0, menuLabels.Count - 1);

        if (prevSelected != currentLabel) { UpdateLabelSelection(); }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            OnSelected?.Invoke(currentLabel);
            CloseMenu();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            OnBack?.Invoke();
            CloseMenu();
        }
    }

    private void UpdateLabelSelection()
    {
        for (int i = 0; i < menuLabels.Count; i++)
        {
            if (i == currentLabel) { menuLabels[i].color = Color.magenta; }
            else { menuLabels[i].color = Color.black; }
        }
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
    }
}
