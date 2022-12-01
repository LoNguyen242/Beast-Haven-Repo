using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    public StatusEffectID ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Message { get; set; }

    public Func<Beast, bool> OnBeforeTurn { get; set; }
    public Action<Beast> OnStartTurn { get; set; }
    public Action<Beast> OnAfterTurn { get; set; }
}
