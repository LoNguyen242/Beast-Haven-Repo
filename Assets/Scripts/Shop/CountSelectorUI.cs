using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] Text countText;
    [SerializeField] Text priceText;

    private bool selected;

    private int maxCounnt;
    private int currentCount;
    private float pricePerUnit;

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit, Action<int> onSelected)
    {
        this.maxCounnt = maxCount;
        this.pricePerUnit = pricePerUnit;

        gameObject.SetActive(true);

        selected = false;
        currentCount = 1;

        SetValues();

        yield return new WaitUntil(() => selected == true);

        onSelected?.Invoke(currentCount);

        gameObject.SetActive(false);
    }

    private void Update()
    {
        int prevCount = currentCount;

        if (Input.GetKeyDown(KeyCode.RightArrow)) { currentCount++; }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) { currentCount--; }

        currentCount = Mathf.Clamp(currentCount, 1, maxCounnt);

        if (currentCount != prevCount) { SetValues(); }

        if (Input.GetKeyDown(KeyCode.Z)) { selected = true; }
    }

    private void SetValues()
    {
        countText.text = "+" + currentCount;
        priceText.text = "G" + pricePerUnit * currentCount;
    }
}
