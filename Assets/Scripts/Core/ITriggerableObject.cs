using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITriggerableObject
{
    bool TriggerRepeatedly { get; }

    void OnTriggered(PlayerController player);
}
