using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Dialog
{
    [TextArea]
    [SerializeField] List<string> lines;

    public List<string> Lines { get { return lines; } }
}
