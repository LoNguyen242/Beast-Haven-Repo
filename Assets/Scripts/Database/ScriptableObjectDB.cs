using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectDB<T> : MonoBehaviour where T : ScriptableObject
{
    public static Dictionary<string, T> Objects { get; set; }

    public static void Init()
    {
        Objects = new Dictionary<string, T>();

        var objectArray = Resources.LoadAll<T>("");
        foreach (var obj in objectArray)
        {
            if (Objects.ContainsKey(obj.name))
            {
                Debug.Log("There are two objects with the name " + obj.name);
                continue;
            }

            Objects[obj.name] = obj;
        }
    }

    public static T GetObjectByName(string name)
    {
        if (!Objects.ContainsKey(name))
        {
            Debug.Log("There is no objects with the name " + name);
            return null;
        }

        return Objects[name];
    }
}
