using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script is used to control the robotic end-effector. 
///     with simple two-finger prismatics grippers.
/// </summary>
public class ArticulationGripperController : MonoBehaviour
{
    // Assume two-finger prismatic joints
    [SerializeField] private ArticulationBody leftFinger;
    [SerializeField] private ArticulationBody rightFinger;

    void Start() { }

    public ArticulationBody[] GetGripperJoints()
    {
        return new ArticulationBody[] { leftFinger, rightFinger };
    }

    public void CloseGripper()
    {
        SetGripper(1.0f);
    }

    public void OpenGripper()
    {
        SetGripper(0.0f);
    }

    public void StopGripper()
    {
        ArticulationBodyUtils.StopJoint(leftFinger);
        ArticulationBodyUtils.StopJoint(rightFinger);
    }

    public void SetGripper(float value)
    {
        // Get absolute position values
        float leftValue = Mathf.Lerp(
            leftFinger.xDrive.lowerLimit, leftFinger.xDrive.upperLimit, value
        );
        float rightValue = Mathf.Lerp(
            rightFinger.xDrive.lowerLimit, rightFinger.xDrive.upperLimit, value
        );

        // Set values
        ArticulationBodyUtils.SetJointTarget(leftFinger, leftValue);
        ArticulationBodyUtils.SetJointTarget(rightFinger, rightValue);
    }
}