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

    // Arm control mode
    // Control: directly control the end effector
    // Target: send joints to target positions
    private enum Mode { Control, Target }
    private Mode mode;

    // Manual IK input - velocity control
    private float[] jointAngles;
    // Automatic grasping with IK solver
    private Coroutine currentCoroutine;
    public AutoGraspable target;

    // Presets
    private static float IGNORE_VAL = ArticulationJointController.IGNORE_VAL;
    [SerializeField] private bool flipPresetAngles;
    // default home position
    [SerializeField] private JointAngles homePositions = new JointAngles(new float[] {
        -1f, -Mathf.PI/2, -Mathf.PI/2, 2.2f, 0.0f, -1.2f, Mathf.PI
    });
    [SerializeField] private JointAngles[] presets = 
    {
        // preset 1 vertical grasping pose
        new JointAngles(new float[] {
            0.7f, -Mathf.PI/2, -Mathf.PI/2f, -1.2f, 0f, -1.0f, Mathf.PI/2
        }),
        // preset 2 is for narrow pose
        new JointAngles(new float[] {
            -2.1f, -1.9f, -1.8f, 2f, -0.4f, -1f, Mathf.PI/2
        }),
        // preset 3 and 4 only change the last joint
        new JointAngles(new float[] {
            IGNORE_VAL, IGNORE_VAL, IGNORE_VAL, IGNORE_VAL,IGNORE_VAL, IGNORE_VAL, Mathf.PI
        }),
        new JointAngles(new float[] {
            IGNORE_VAL, IGNORE_VAL, IGNORE_VAL, IGNORE_VAL,IGNORE_VAL, IGNORE_VAL, Mathf.PI/2
        }),
    };

    void Start() 
    { 
        // Home joints at the beginning
        jointController.SetJointTargets(homePositions, ture);
        gripperController.OpenGripper();

        // Init as Control mode and automation params
        mode = Mode.Control;

  }

    void FixedUpdate()
    {
        // Emergency stop
        if (emergencyStop)
        {
            jointController.StopJoints();
            return;
        }

        // If in manual mode
        if (mode == Mode.Control)
        {
            UpdateManualControl();
        }

        // jointController.SetEndEffectorVelocity(linearVelocity, angularVelocity);
        gripperController.SetGripper(gripperPosition);
    }

    private void UpdateManualControl()
    {
        // End effector position control
        if (deltaPosition != Vector3.zero || deltaRotation != Vector3.zero)
        {
            Quaternion deltaRotationQuaternion = Quaternion.Euler(deltaRotation);
            jointAngles = jointController.GetCurrentJointTargets();
            jointAngles = newtonIK.SolveVelocityIK(jointAngles, deltaPosition, deltaRotationQuaternion);
            jointController.SetJointTargets(jointAngles);
        }
        // Fixing joints when not controlling
        else
        {
            jointAngles = jointController.GetCurrentJointTargets();
            jointController.SetJointTargets(jointAngles);
        }
    }

    // Move to Preset
    public bool MoveToPreset(int presetIndex)
    {
        // Do not allow auto moving when grasping heavy object
        if (grasping.isGrasping && grasping.GetGraspedObjectMass() > 1)
            return false;

        // Home Position
        if (presetIndex == 0)
            return false;
            // MoveToJointPosition(jointController.homePositions);
        // Presets
        else
            if (flipPresetAngles)
            {
                float[] angles = new float[presets[presetIndex-1].jointAngles.Length];
                for (int i = 0; i < angles.Length; ++i)
                {
                    int multiplier = -1;
                    if (presets[presetIndex-1].jointAngles[i] == IGNORE_VAL)
                        multiplier = 1;
                    angles[i] = multiplier * presets[presetIndex-1].jointAngles[i];
                }
                return false;
                // MoveToJointPosition(angles);
            }
            else
                return false;
                // MoveToJointPosition(presets[presetIndex-1].jointAngles);
        return true;
    }
}
