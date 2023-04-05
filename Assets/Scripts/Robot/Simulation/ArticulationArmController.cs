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
    [SerializeField] private bool emergencyStop = false;

    public virtual void EmergencyStop(bool stop = true)
    {
        emergencyStop = stop;
    }

    // Arm component controller
    [SerializeField] private ArticulationJointController jointController;
    [SerializeField] private ArticulationGripperController gripperController;
    [SerializeField] private NewtonIK newtonIK;

    // Presets
    private static float IGNORE_VAL = ArticulationJointController.IGNORE_VAL;
    [SerializeField] private JointAngles[] presets = 
    {
        // preset 1 is the default joint home position
        // preset 2 vertical grasping pose
        new JointAngles(new float[] {
            0.7f, -Mathf.PI/2, -Mathf.PI/2f, -1.2f, 0f, -1.0f, Mathf.PI/2
        }),
        // preset 3 is for narrow pose
        new JointAngles(new float[] {
            -2.1f, -1.9f, -1.8f, 2f, -0.4f, -1f, Mathf.PI/2
        }),
        // preset 4 and 5 only read the last joint
        new JointAngles(new float[] {
            IGNORE_VAL, IGNORE_VAL, IGNORE_VAL, IGNORE_VAL,IGNORE_VAL, IGNORE_VAL, Mathf.PI
        }),
        new JointAngles(new float[] {
            IGNORE_VAL, IGNORE_VAL, IGNORE_VAL, IGNORE_VAL,IGNORE_VAL, IGNORE_VAL, Mathf.PI/2
        }),
    };

    void Start() { }

    void FixedUpdate()
    {
        if (!emergencyStop)
        {
            // jointController.SetEndEffectorVelocity(linearVelocity, angularVelocity);
            gripperController.SetGripper(gripperPosition);
        }
        else
        {
            StopEndEffector();
        }
    }
}
