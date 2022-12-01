using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastParty : MonoBehaviour
{
    [SerializeField] List<Beast> beasts;

    public event Action OnUpdated;

    public List<Beast> Beasts
    {
        get { return beasts; }
        set
        {
            beasts = value;
            UpdateParty();
        }
    }

    private void Awake()
    {
        foreach (var beast in beasts) { beast.Init(); }
    }

    public static BeastParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<BeastParty>();
    }

    public Beast GetHealthyBeast()
    {
        for (int i = 0; i < beasts.Count; i++)
        {
            if (beasts[i].HP > 0) { return beasts[i]; }
        }

        return null;
    }

    public void AddBeast(Beast newBeast)
    {
        if (beasts.Count < 6)
        {
            beasts.Add(newBeast);
            UpdateParty();
        }
        else { }
    }

    public void UpdateParty()
    {
        OnUpdated?.Invoke();
    }
}
