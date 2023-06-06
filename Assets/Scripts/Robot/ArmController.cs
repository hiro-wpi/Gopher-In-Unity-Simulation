using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     An abstract class to handle Unity input for arm control,
///     including both joint control and gripper control.
///
///     Two speed modes are available: Slow, and Regular,
///     which correspond to 0.5, 1 of the max velocity.
///     Clipping is also applied to the input.
///     Smoothing is handled by low level joint controller
///
///     The final result
///     would be handled differently for 
///     simulation robot or physical robot
/// </summary>
public abstract class ArmController : MonoBehaviour
{
    // Control parameters
    [SerializeField] protected float linearSpeedMultiplier = 0.25f;
    [SerializeField] protected float angularSpeedMultiplier = 0.5f;
    [SerializeField] protected float gripperPositionMultiplier = 1.0f;
    [SerializeField] protected float maxLinearSpeed = 0.25f;
    [SerializeField] protected float maxAngularSpeed = 0.5f;

    // Control mode (Different velocities)
    public enum Mode { Slow = 0, Regular = 1 }
    [field: SerializeField] public Mode SpeedMode { get; set; } = Mode.Regular;
    [SerializeField] protected float[] modeMultiplier = { 0.5f, 1.0f };

    // Variable to hold velocity
    [SerializeField, ReadOnly] protected Vector3 linearVelocity;
    [SerializeField, ReadOnly] protected Vector3 angularVelocity;
    [SerializeField, ReadOnly] protected float gripperPosition;

    void Start() {}

    void Update() {}

    // Set joint positions (in radians)
    public virtual void SetJointAngles(float[] jointAngles) {}

    // Set end effector velocity
    public virtual void SetLinearVelocity(Vector3 linear)
    {
        // Clipping and setting target linear velocity
        linearVelocity = Utils.ClampVector3(
            linear * linearSpeedMultiplier * modeMultiplier[(int)SpeedMode],
            -maxLinearSpeed,
            maxLinearSpeed
        );
    }

    public virtual void SetAngularVelocity(Vector3 angular)
    {
        // Clipping and setting target angular velocity
        angularVelocity = Utils.ClampVector3(
            angular * angularSpeedMultiplier * modeMultiplier[(int)SpeedMode],
            -maxAngularSpeed,
            maxAngularSpeed
        );
    }

    public virtual void StopEndEffector()
    {
        linearVelocity = Vector3.zero;
        angularVelocity = Vector3.zero;
    }

    // Set gripper position
    public virtual void SetGripperPosition(float position)
    {
        // Clipping and setting gripper position
        gripperPosition = Mathf.Clamp(
            position * gripperPositionMultiplier,
            0.0f,
            1.0f
        );
    }

    public virtual void ChangeGripperStatus()
    {
        gripperPosition = 1.0f - gripperPosition;
    }

    // Autonomy function
    public virtual void SetTarget(Vector3 position, Quaternion rotation) {}

    public virtual void CancelTarget() {}

    public virtual void MoveToTarget() {}

    // Set robot control mode
    public void SetMode(Mode mode)
    {
        SpeedMode = mode;
    }

    // Switch to the next robot control mode
    public void SwitchMode()
    {
        int numMode = System.Enum.GetNames(typeof(Mode)).Length;
        SpeedMode = (Mode)(((int)SpeedMode + 1) % numMode);
    }

    // Pre-defined positions
    public virtual void HomeJoints() {}

    public virtual void MoveToPreset(int presetIndex) {}

    // Emergency stop
    public virtual void EmergencyStop() {}

    public virtual void EmergencyStopResume() {}
}
