using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     An abstract class to handle Unity input for camera control.
///     Clipping is also to the input
///
///     The final result
///     would be handled differently for 
///     simulation robot or physical robot
/// </summary>
public class CameraController : MonoBehaviour
{
    // Control parameters
    [SerializeField] protected float angularSpeedMultiplier = 1.0f;
    [SerializeField] protected float maxAngularSpeed = 1.0f;
    [SerializeField] protected float angleLowerLimit = -60.0f * Mathf.Deg2Rad;
    [SerializeField] protected float angleUpperLimit = 60.0f * Mathf.Deg2Rad;

    // Camera control mode
    public enum ControlMode { Speed, Position }
    protected ControlMode controlMode = ControlMode.Speed;

    // Variable to hold speed & target angles
    [SerializeField, ReadOnly] protected Vector3 angularVelocity;
    [SerializeField, ReadOnly] protected Vector3 angles;

    void Start() {}

    void Update() {}

    // Velocity control
    public virtual void SetVelocity(Vector3 angular)
    {
        // Clipping and setting target velocity
        angularVelocity = Utils.ClampVector3(
            angular * angularSpeedMultiplier,
            -maxAngularSpeed,
            maxAngularSpeed
        );
    }

    // Position control
    public virtual void SetPosition(Vector3 ang)
    {
        angles = Utils.ClampVector3(ang, angleLowerLimit, angleUpperLimit);
    }

    public virtual void SetControlMode(ControlMode mode)
    {
        controlMode = mode;
    }

    // Stop
    public virtual void StopCamera() {}

    // Pre-defined position
    public virtual void HomeCamera() {}
}
