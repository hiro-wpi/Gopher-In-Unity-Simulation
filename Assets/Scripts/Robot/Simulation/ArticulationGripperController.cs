using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script is used to control the robotic
///     end-effector. This script is originally used 
///     for robotic 2F-85, with modification into prismatic gripper. 
//      Others can also be used with minor modification.
/// </summary>
public class ArticulationGripperController : MonoBehaviour
{

    // New class to hold the articulation body and close/open values
    // Assume prismatic
    public ArticulationBody leftFinger;
    public ArticulationBody rightFinger;

    void Start()
    {
    }

    public void CloseGripper()
    {
        SetGripper(1f);
    }

    public void OpenGripper()
    {
        SetGripper(0f);
    }

    public void SetGripper(float value)
    {
        float leftValue = Mathf.Lerp(leftFinger.xDrive.lowerLimit, leftFinger.xDrive.upperLimit, value);
        SetTarget(leftFinger, leftValue);

        float rightValue = Mathf.Lerp(rightFinger.xDrive.lowerLimit, rightFinger.xDrive.upperLimit, value);
        SetTarget(rightFinger, rightValue);
    }

    void SetTarget(ArticulationBody joint, float target)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.target = target;
        joint.xDrive = drive;
    }
}