using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
///     An abstract class to handle Unity input for arm control,
///     including both joint control and gripper control.
///
///     Two control modes are available: Slow, and Regular,
///     which correspond to 0.5, 1 of the max velocity.
///     Clipping is also applied to the input.
///     Smoothing is handled by actual arm controller 
///     // TODO: add smoothing
///
///     The final result
///     would be handled differently for 
///     simulation robot or physical robot
/// </summary>
public abstract class ArmController : MonoBehaviour
{
    // Control parameters
    [SerializeField] protected float maxLinearSpeed = 0.25f;
    [SerializeField] protected float maxAngularSpeed = 0.5f;
    [SerializeField] protected float gripperSpeed = 0.5f;

    // Control mode (Different velocities)
    public enum Mode { Slow = 0, Regular = 1 }
    [field: SerializeField] public Mode ControlMode { get; set; } = Mode.Regular;
    [SerializeField] protected float[] modeMultiplier = { 0.5f, 1.0f };

    // Variable to hold velocity
    [SerializeField, ReadOnly] protected Vector3 linearVelocity;
    [SerializeField, ReadOnly] protected Vector3 angularVelocity;
    [SerializeField, ReadOnly] protected float currentGripperSpeed;

    // Velocity update rate
    [SerializeField] protected int updateRate = 60;
    protected float deltaTime;

    protected virtual void Start()
    {
        // Use the velocity at a fixed rate
        deltaTime = 1.0f / updateRate;
    }

    void Update() { }

    // Set end effector velocity
    public virtual void SetLinearVelocity(Vector3 linear)
    {
        // Clipping and setting target linear velocity
        linearVelocity = Utils.ClampVector3(
            linear,
            -maxLinearSpeed * modeMultiplier[(int)ControlMode],
            maxLinearSpeed * modeMultiplier[(int)ControlMode]
        );
    }

    public virtual void SetAngularVelocity(Vector3 angular)
    {
        // Clipping and setting target angular velocity
        angularVelocity = Utils.ClampVector3(
            angular,
            -maxAngularSpeed * modeMultiplier[(int)ControlMode],
            maxAngularSpeed * modeMultiplier[(int)ControlMode]
        );
    }

    // Set gripper speed
    public virtual void SetGripperSpeed(float speed)
    {
        // Clipping and setting gripper speed
        currentGripperSpeed = Mathf.Clamp(
            speed, -gripperSpeed, gripperSpeed
        );
    }


    // Set robot control mode
    public void SetMode(Mode mode)
    {
        ControlMode = mode;
    }

    // Switch to the next robot control mode
    public void SwitchMode()
    {
        int numMode = System.Enum.GetNames(typeof(Mode)).Length;
        ControlMode = (Mode)(((int)ControlMode + 1) % numMode);
    }

    // Pre-defined position
    public virtual void MoveToPreset(int presetIndex) { }
}
