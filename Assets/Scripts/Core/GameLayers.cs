using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask player;
    [SerializeField] LayerMask collision;
    [SerializeField] LayerMask longGrass;
    [SerializeField] LayerMask interactable;
    [SerializeField] LayerMask fov;
    [SerializeField] LayerMask portal;
    [SerializeField] LayerMask trigger;
    [SerializeField] LayerMask triggerableLayers;

    public static GameLayers Instance { get; set; }

    public LayerMask Player { get { return player; } }
    public LayerMask Collision { get { return collision; } }
    public LayerMask LongGrass { get { return longGrass; } }
    public LayerMask Interactable { get { return interactable; } }
    public LayerMask FOV { get { return fov; } }
    public LayerMask Portal { get { return portal; } }
    public LayerMask Trigger { get { return trigger; } }
    public LayerMask TriggerableLayers { get { return triggerableLayers; } }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }
}
