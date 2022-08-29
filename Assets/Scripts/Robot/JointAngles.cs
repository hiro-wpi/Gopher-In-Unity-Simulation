using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A simple arm angle array that is 
///     visualizable in the editor
/// </summary>                               
[System.Serializable]
public class JointAngles
{
    public float[] jointAngles;
    public JointAngles(float[] angles)
    {
        jointAngles = angles;
    }
}
