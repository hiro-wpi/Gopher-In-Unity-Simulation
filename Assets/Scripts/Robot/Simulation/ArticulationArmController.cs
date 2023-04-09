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
    public virtual void EmergencyStop(bool stop = true)
    {
        emergencyStop = stop;
    }

    // Arm component controller
    [SerializeField] private ArticulationJointController jointController;
    [SerializeField] private ArticulationGripperController gripperController;
    [SerializeField] private NewtonIK newtonIK;

    // Arm control mode
    private enum ControlMode { Control, Target }
    private ControlMode controlMode = ControlMode.Control;

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
        HomeJoints();
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

    // When gripper speed is set
    public override void SetGripperPosition(float position)
    {
        base.SetGripperPosition(position);
        gripperController.SetGripper(position);
    }

    private void ProcessManualControl()
    {
        // End effector position control
        if (linearVelocity != Vector3.zero || angularVelocity != Vector3.zero)
        {
            // Convert to position error and angular error in next timestep
            Vector3 linearError = linearVelocity * Time.fixedDeltaTime;
            Vector3 angularError = angularVelocity * Time.fixedDeltaTime;
            // Solve IK
            jointAngles = jointController.GetCurrentJointTargets();
            jointAngles = newtonIK.SolveVelocityIK(
                jointAngles, linearError, Quaternion.Euler(angularError)
            );
            // Set joint targets to IK solution
            jointController.SetJointTargets(jointAngles);
        }
    }

    // Move to Preset
    private void HomeJoints()
    {
        float[] angles = homePositions.Angles;
        if (flipPresetAngles)
        {
            angles = FlipAngles(angles);
        }
        currentCoroutine = StartCoroutine(MoveToPresetCoroutine(homePositions.Angles, true));
    }

    public override bool MoveToPreset(int presetIndex)
    {
        // Do not allow moving joints to preset when grasping heavy object
        if (gripperController.GetGraspedObjectMass() > 1.0f)
        {
            return false;
        }
        
        // Get preset angles
        float[] angles;
        if (presetIndex == 0)
        {
            angles = homePositions.Angles;
        }
        else
        {
            angles = presets[presetIndex-1].Angles;
        }
        // flip angles if needed (left vs right)
        if (flipPresetAngles)
        {
            angles = FlipAngles(angles);
        }
        
        // Move to presets
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(MoveToPresetCoroutine(angles, false));

        return true;
    }

    private float[] FlipAngles(float[] angles)
    {
        // Joint 1 is not flipped, but 180 degree offset
        angles[0] = angles[0] + Mathf.PI;
        // Other joints are flipped to negative
        for (int i = 1; i < angles.Length; ++i)
        {
            if (angles[i] == IGNORE_VAL)
            {
                continue;
            }
            angles[i] = -1 * angles[i];
        }
        return angles;
    }

    private IEnumerator MoveToPresetCoroutine(float[] angles, bool disableColliders)
    {
        controlMode = ControlMode.Target;
        jointController.SetJointTargets(angles, disableColliders);

        yield return new WaitUntil(() => CheckPositionReached(angles) == true);
        controlMode = ControlMode.Control;
    }

    private bool CheckPositionReached(float[] angles)
    {
        float[] currTargets = jointController.GetCurrentJointTargets();
        for (int i = 0; i < angles.Length; ++i)
        {
            if ((angles[i] != IGNORE_VAL) && 
                (Mathf.Abs(currTargets[i] - angles[i]) > 0.00001))
            {
                return false;
            }
        }
        return true;
    }
}
