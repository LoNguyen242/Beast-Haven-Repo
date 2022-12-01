using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialLoader : MonoBehaviour
{
    [SerializeField] GameObject EssentialObjects;

    private void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObjects>();
        if (existingObjects.Length == 0)
        {
            var spawnPos = Vector3.zero;
            var grid = FindObjectOfType<Grid>();
            if (grid != null ) { spawnPos= grid.transform.position; }

            Instantiate(EssentialObjects, spawnPos, Quaternion.identity);
        }
    }
}
