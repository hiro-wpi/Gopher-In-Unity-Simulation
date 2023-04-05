using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script sends commands to robot joints and gripper.
///     
///     Two control modes are available: Slow, and Regular,
///     which correspond to 0.5, 1 of the max velocity.
///     Clipping is also applied to the input.
/// </summary>
public class ArticulationArmController : ArmController
{
    // Emergency Stop
    [SerializeField] protected bool emergencyStop = false;

    // Arm component controller
    public ArticulationJointController jointController;
    public ArticulationGripperController gripperController;

    public virtual void EmergencyStop(bool stop = true)
    {
        emergencyStop = stop;
    }

    void Start() { }

    void FixedUpdate()
    {
        if (!emergencyStop)
        {
            jointController.SetEndEffectorVelocity(linearVelocity, angularVelocity);
            gripperController.SetGripper(gripperPosition);
        }
        else
        {
            StopEndEffector();
        }
    }
}
