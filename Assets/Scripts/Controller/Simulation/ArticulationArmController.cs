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
///     Manual: directly control the end effector using IK
///     Auto: send joints to a target positions autonomously
/// </summary>
public class ArticulationArmController : ArmController
{
    // Emergency Stop
    [SerializeField] private bool emergencyStop = false;

    // Arm component controller
    [Header("Local Controller")]
    [SerializeField]
    private ArticulationEndEffectorController endEffectorController;
    [SerializeField] private ArticulationJointController jointController;
    [SerializeField] private ArticulationGripperController gripperController;

    [Header("Autonomy")]
    [SerializeField] private AutoManipulation autoManipulation;

    // Arm control mode
    // manual & auto
    private enum ControlMode { Manual, Auto }
    [Header("Control Mode")]
    [SerializeField, ReadOnly]
    private ControlMode controlMode = ControlMode.Manual;

    // Manual control mode
    public enum ManualControlMode { Position, Velocity }
    [Header("Manual Control Mode")]
    [SerializeField]
    private ManualControlMode manualControlMode = ManualControlMode.Velocity;
    // manual control event (triggered after switching to manual control)
    public delegate void ManualControlHandler();
    public event ManualControlHandler OnManualControl;
    // Manual IK input - velocity control
    private float[] jointAngles;

    // Autonomy motion with IK solver
    [Header("Autonomy Planning")]
    [SerializeField, ReadOnly] private Vector3 targetPosition;
    [SerializeField, ReadOnly] private Quaternion targetRotation;
    // autonomy event (triggered after switching to manual control)
    public delegate void AutonomControlHandler();
    public event AutonomControlHandler OnAutonomy;
    // autonomy event (triggered if valid trajectory generated)
    public delegate void AutonomyTrajectoryHandler();
    public event AutonomyTrajectoryHandler OnAutonomyTrajectory;
    // autonomy event (triggered if valid trajectory generated)
    public delegate void AutonomyCompleteHandler();
    public event AutonomyCompleteHandler OnAutonomyComplete;
    // planning coroutine
    private Coroutine currentCoroutine;

    // Pre-defined joint angles
    [Header("Pre-defined Poses")]
    [SerializeField] private bool flipPresetAngles;  // To be removed
    // default home position
    [SerializeField] private JointAngles homePositions = new(new float[] {
        -1f, -Mathf.PI/2, -Mathf.PI/2, 2.2f, 0.0f, -1.2f, Mathf.PI
    });
    // Math.Infinity means no change
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
            Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, Mathf.Infinity,
            Mathf.Infinity, Mathf.Infinity, Mathf.PI
        }),
        new JointAngles(new float[] {
            Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, Mathf.Infinity,
            Mathf.Infinity, Mathf.Infinity, Mathf.PI/2
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
        if (controlMode == ControlMode.Manual)
        {
            ProcessManualControl();
        }
    }

    // Manual mode
    public void SwitchToManualControl()
    {
        controlMode = ControlMode.Manual;

        // Reset EE target to track the current pose
        endEffectorController.SetHome(
            jointController.GetCurrentJointTargets()
        );

        OnManualControl?.Invoke();
    }

    public void SwitchToManualVelocityControl()
    {
        SwitchToManualControl();
        manualControlMode = ManualControlMode.Velocity;
    }

    public void SwitchToManualPositionControl()
    {
        SwitchToManualControl();
        manualControlMode = ManualControlMode.Position;
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
        // Delta pose as position and angular error in next timestep
        if (manualControlMode == ManualControlMode.Velocity)
        {
            endEffectorController.SetTargetDeltaPose(
                linearVelocity * Time.fixedDeltaTime, 
                angularVelocity * Time.fixedDeltaTime
            );
        }
        else if (manualControlMode == ManualControlMode.Position)
        {
            endEffectorController.SetTargetPose(position, rotation);
        }
        endEffectorController.MoveToTargetStep();
    }

    // Autonomous mode
    public void SwitchToAutonomy()
    {
        controlMode = ControlMode.Auto;
        OnAutonomy?.Invoke();
    }

    public override void SetAutonomyTarget(
        Vector3 position, Quaternion rotation = new Quaternion()
    )
    {
        // If rotation is not provided, use current
        if (Quaternion.Dot(rotation, rotation) < Mathf.Epsilon)
        {
            rotation = GetEETargetPose().Item2;
        }
        targetPosition = position;
        targetRotation = rotation;

        // Try to plan a path to the target
        Debug.Log("Sending request to move to the target.");
        jointAngles = jointController.GetCurrentJointTargets();
        autoManipulation.PlanTrajectory(
            jointAngles, targetPosition, targetRotation, TrajectoryGenerated
        );
    }

    public override void CancelAutonomyTarget()
    {
        targetPosition = Vector3.zero;
        targetRotation = Quaternion.identity;

        if (controlMode == ControlMode.Auto)
        {
            autoManipulation.StopManipulation();
            SwitchToManualControl();
        }
    }

    public override void MoveToAutonomyTarget()
    {
        autoManipulation.StartManipulation(OnAutonomyDone);
    }

    private void TrajectoryGenerated(
        float[] timeSteps,
        float[][] angles,
        float[][] velocities,
        float[][] accelerations
    )
    {
        // check validity of the path
        if (timeSteps == null || timeSteps.Length <= 1)
        {
            return;
        }

        OnAutonomyTrajectory?.Invoke();
    }

    private void OnAutonomyDone()
    {
        SwitchToManualControl();
        OnAutonomyComplete?.Invoke();
    }

    public (float[], float[][], float[][], float[][]) GetAutonomyTrajectory()
    {
        return (
            autoManipulation.TimeSteps,
            autoManipulation.Angles,
            autoManipulation.Velocities,
            autoManipulation.Accelerations
        );
    }

    // Move to joint angles
    public override void SetJointAngles(float[] jointAngles)
    {
        // Move to given position
        SwitchToAutonomy();
        jointController.SetJointTargets(
            jointAngles, false, SwitchToManualControl
        );
    }

    // Move the joint following a trajectory
    public void SetJointTrajectory(
        float[] timeSteps, float[][] angles, 
        float[][] velocities, float[][] accelerations
    )
    {
        SwitchToAutonomy();
        jointController.SetJointTrajectory(
            timeSteps,
            angles, 
            velocities,
            accelerations,
            SwitchToManualControl
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
        SwitchToAutonomy();
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
            if (angles[i] == Mathf.Infinity)
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

    // Some getter functions
    // Get the current joint angles
    // TODO do it in ArmController
    public override float[] GetJointAngles()
    {
        return jointController.GetCurrentJointTargets();
    }

    // Get the actual end effector pose
    public override (Vector3, Quaternion) GetEEPose()
    {
        GameObject ee = gripperController.GetEndEffector();
        return (ee.transform.position, ee.transform.rotation);
    }

    // Get the end effector target pose
    public override (Vector3, Quaternion) GetEETargetPose()
    {
        return endEffectorController.GetEETargetPose();
    }

    // Stop all joints
    public override void StopAllJoints()
    {
        jointController.StopJoints();
        gripperController.StopGripper();
    }
}
