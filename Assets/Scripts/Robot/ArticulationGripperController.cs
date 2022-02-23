using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script is used to control the robotic
///     end-effector. This script is specifically used 
///     for robotic 2F-85. But others can also be used
///     with minor modification.
/// </summary>
public class ArticulationGripperController : MonoBehaviour
{
    
    public ArticulationBody[] leftFingerChain;
    public ArticulationBody[] rightFingerChain;
    public float closeValue = 48f;
    public float openValue = 0f;

    void Start()
    {
    }

    public void SetGrippers(float closeValue) 
    {
        for (int i=0; i < leftFingerChain.Length; ++i)
        {
            if (i == 2) // left inner finger
            {
                SetTarget(leftFingerChain[i], -closeValue);
                SetTarget(rightFingerChain[i], -closeValue);
            }
            else
            {
                SetTarget(leftFingerChain[i], closeValue);
                SetTarget(rightFingerChain[i], closeValue);
            }
        }
    }
    
    public void CloseGrippers() 
    {
        SetGrippers(closeValue); // Deg
    }

    public void OpenGrippers() 
    {
        SetGrippers(openValue); // Deg
    }

    void SetTarget(ArticulationBody joint, float target)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.target = target;
        joint.xDrive = drive;
    }
}