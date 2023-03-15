using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
///     An abstract class to handle Unity input for wheel control
///
///     Two control modes are available: Slow, and Regular,
///     which correspond to 0.5, 1 of the max velocity.
///     Clipping and smoothing are also applied to the input.
///
///     The final result (linearVeclocity, angularVelocity) 
///     would be handled differently for 
///     simulation robot or physical robot
/// </summary>
public abstract class WheelController : MonoBehaviour
{
    // Control parameters
    [SerializeField] protected float maxLinearSpeed = 1.0f;
    [SerializeField] protected float maxAngularSpeed = Mathf.PI/2.0f;
    [SerializeField] protected float linearAcceleration = 2.0f;
    [SerializeField] protected float angularAcceleration = Mathf.PI;
    [SerializeField] protected float backwardMultiplier = 0.5f;

    // Control mode (Different velocities)
    public enum Mode { Slow = 0, Regular = 1 }
    [field: SerializeField] public Mode ControlMode { get; set; } = Mode.Regular;
    [SerializeField] protected float[] modeMultiplier = { 0.5f, 1.0f };

    // Variable to hold velocity
    [SerializeField, ReadOnly] protected Vector3 linearVelocity;
    [SerializeField, ReadOnly] protected Vector3 angularVelocity;
    protected Vector3 targetLinearVelocity;
    protected Vector3 targetAngularVelocity;

    // Velocity update rate
    [SerializeField] protected int updateRate = 60;
    protected float deltaTime;

    protected virtual void Start() 
    {
        // Keep updating the velocity at a fixed rate
        deltaTime = 1.0f / updateRate;
        InvokeRepeating("UpdateVelocities", 1.0f, deltaTime);
    }

    void Update() { }

    // Set robot velocity
    public virtual void SetVelocity(Vector3 linear, Vector3 angular)
    {
        // Clipping and setting target velocity
        targetLinearVelocity = Utils.ClampVector3(
            linear,
            -maxLinearSpeed * modeMultiplier[(int)ControlMode] * backwardMultiplier,
            maxLinearSpeed * modeMultiplier[(int)ControlMode]
        );
        targetAngularVelocity = Utils.ClampVector3(
            angular,
            -maxAngularSpeed * modeMultiplier[(int)ControlMode],
            maxAngularSpeed * modeMultiplier[(int)ControlMode]
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

    // Update both the linear and angular velocity
    private void UpdateVelocities()
    {
        // Update velocity based on acceleration
        linearVelocity = UpdateVelocity(
            linearVelocity, targetLinearVelocity, linearAcceleration
        );
        angularVelocity = UpdateVelocity(
            angularVelocity, targetAngularVelocity, angularAcceleration
        );
    }

    // Update the velocity based on the target velocity and acceleration
    private Vector3 UpdateVelocity(
        Vector3 currVelocity, Vector3 targetVelocity, float acceleration)
    {
        Vector3 newVelocity = currVelocity;

        // Difference between current and target velocity
        Vector3 velocityDiff = targetVelocity - currVelocity;
        // The avaliable change in velocity based on acceleration
        Vector3 velocityChange = Vector3.one * acceleration * deltaTime;

        for (int i = 0; i < 3; ++i)
        {
            // If the change in velocity is greater than the difference, 
            // then the target velocity is reached
            if (Mathf.Abs(velocityDiff[i]) < velocityChange[i])
            {
                newVelocity[i] = targetVelocity[i];
            }
            // Else the velocity is updated based on the sign of the difference
            // and the avaliable change in velocity
            else
            {
                newVelocity[i] += Mathf.Sign(velocityDiff[i]) * velocityChange[i];
            }
        }

        return newVelocity;
    }
}
