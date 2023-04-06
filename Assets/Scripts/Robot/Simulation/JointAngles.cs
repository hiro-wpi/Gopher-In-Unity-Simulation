using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A simple class that lets
///     joint angles to be visualizable in the editor
/// </summary>                               
[System.Serializable]
public class JointAngles
{
    [field: SerializeField]
    public float[] Angles { get; set; }

    public JointAngles(float[] angles)
    {
        Angles = angles;
    }
}
