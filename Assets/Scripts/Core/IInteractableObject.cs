using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractableObject
{
    IEnumerator OnInteracted(Transform player);
}
