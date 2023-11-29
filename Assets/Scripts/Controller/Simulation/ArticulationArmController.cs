using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script sends commands to robot joints and gripper.
///     
///     Two speed modes are available: Slow, and Regular,
///     which correspond to 0.5, 1 of the max velocity.
///     Clipping is also applied to the input.
///
///     Two control modes are used to control the arm
///     Control: directly control the end effector using IK
///     Target: send joints to a target positions
/// </summary>
public class ArticulationArmController : ArmController
{
    // Emergency Stop
    [SerializeField] private bool emergencyStop = false;

    // Arm component controller
    [SerializeField] 
    private ArticulationEndEffectorController endEffectorController;
    [SerializeField] private ArticulationJointController jointController;
    [SerializeField] private ArticulationGripperController gripperController;
    [SerializeField] private AutoManipulation autoManipulation;

    // Arm control mode
    private enum ControlMode { Control, Target }
    private ControlMode controlMode = ControlMode.Control;

    // Manual IK input - velocity control
    private float[] jointAngles;
    // Automatic grasping with IK solver
    private Coroutine currentCoroutine;
    [SerializeField, ReadOnly] private bool targetSet = false;
    [SerializeField, ReadOnly] private Vector3 targetPosition;
    [SerializeField, ReadOnly] private Quaternion targetRotation;

    // Presets
    private static float IGNORE_VAL = ArticulationJointController.IGNORE_VAL;
    [SerializeField] private bool flipPresetAngles;
    // default home position
    [SerializeField] private JointAngles homePositions = new(new float[] {
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
        InitializeJoints();
        gripperController.OpenGripper();
    }

    void FixedUpdate()
    {
        // Emergency stop
        if (emergencyStop)
        {
            jointController.StopJoints();
            gripperController.StopGripper();
            return;
        }

        // If in manual controlMode
        if (controlMode == ControlMode.Control)
        {
            ProcessManualControl();
        }
    }

    // Control mode
    public void SwitchToManualControl()
    {
        controlMode = ControlMode.Control;
    }

    // Gripper
    public override void SetGripperPosition(float position)
    {
        base.SetGripperPosition(position);
        gripperController.SetGripper(position);
    }

    public override void ChangeGripperStatus()
    {
        gripperController.ChangeGripperStatus();
    }

    // Manual real-time control
    private void ProcessManualControl()
    {
        // End effector position control
        if (linearVelocity != Vector3.zero || angularVelocity != Vector3.zero)
        {
            // Convert to position error and angular error in next timestep
            Vector3 linearError = linearVelocity * Time.fixedDeltaTime;
            Vector3 angularError = angularVelocity * Time.fixedDeltaTime;

            endEffectorController.SetTargetDeltaPose(linearError, angularError);
            endEffectorController.MoveToTargetStep();
        }
    }

    // Autonomous mode
    public override void SetTarget(Vector3 position, Quaternion rotation)
    {
        targetSet = true;
        targetPosition = position;
        targetRotation = rotation;
    }

    public override void CancelTarget()
    {
        targetSet = false;
        targetPosition = Vector3.zero;
        targetRotation = Quaternion.identity;
    }

    public override void MoveToTarget()
    {
        if (!targetSet)
        {
            Debug.Log("Target not set.");
            return;
        }

        // Try to plan a path to the target
        Debug.Log("Sending request to move to the target.");
        jointAngles = jointController.GetCurrentJointTargets();
        autoManipulation.PlanTrajectory(
            jointAngles, targetPosition, targetRotation, TrajectoryGenerated
        );
    }

    public void TrajectoryGenerated(
        float[] timeSteps, float[][] angles, float[][] velocities, float[][]accelerations
    )
    {
        // check validity of the path
        if (timeSteps.Length <= 1)
        {
            Debug.Log("No path found");
            return;
        }

        // Execute the path
        SetJointTrajectory(timeSteps, angles, velocities, accelerations);
    }

    // Move to joint angles
    public override void SetJointAngles(float[] jointAngles)
    {
        // Move to given position
        controlMode = ControlMode.Target;
        endEffectorController.SetJointAsTarget(jointAngles);
        jointController.SetJointTargets(jointAngles, false, SwitchToManualControl);
    }

    // Move the joint following a trajectory
    public void SetJointTrajectory(
        float[] timeSteps, float[][] angles, float[][] velocities, float[][] accelerations
    )
    {
        controlMode = ControlMode.Target;
        jointController.SetJointTrajectory(
            timeSteps, angles, velocities, accelerations, SwitchToManualControl
        );
    }

    // Move to Preset
    private void InitializeJoints()
    {
        // Home angles
        float[] angles = homePositions.Angles;
        if (flipPresetAngles)
        {
            angles = FlipAngles(angles);
        }
        // Move to the position
        controlMode = ControlMode.Target;
        endEffectorController.SetJointAsTarget(angles);
        jointController.SetJointTargets(angles, true, SwitchToManualControl);
    }

    public override void HomeJoints()
    {
        MoveToPreset(-1);
    }

    public override void MoveToPreset(int presetIndex)
    {
        // Get preset angles
        float[] angles;
        if (presetIndex == -1)
        {
            angles = homePositions.Angles;
        }
        else
        {
            angles = presets[presetIndex].Angles;
        }
        // flip angles if needed (left vs right)
        if (flipPresetAngles)
        {
            angles = FlipAngles(angles);
        }
        
        // Move to presets
        SetJointAngles(angles);
    }

    private float[] FlipAngles(float[] angles)
    {
        float[] flippedAngles = new float[angles.Length];
        // Joint 1 is not flipped, but 180 degree offset
        flippedAngles[0] = -angles[0] + Mathf.PI;
        // Other joints are flipped to negative
        for (int i = 1; i < angles.Length; ++i)
        {
            if (angles[i] == IGNORE_VAL)
            {
                flippedAngles[i] = angles[i];
            }
            else
            {
                flippedAngles[i] = -angles[i];
            }
        }
        return flippedAngles;
    }

    // Emergency Stop
    public override void EmergencyStop()
    {
        emergencyStop = true;
    }

    public override void EmergencyStopResume()
    {
        emergencyStop = false;
    }
}
