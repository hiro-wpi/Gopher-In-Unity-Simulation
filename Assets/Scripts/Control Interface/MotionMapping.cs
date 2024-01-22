using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Motion mapping script that maps the input pose to the output pose
/// - The mapping could be stopped and resumed in a different input pose
/// - The mapping could be set to follow only the position 
/// or both the position and orientation
/// - The mapping orientation could be set to be relative to the input device
/// or absolute with an pre-defined offset
/// 
/// - The maximum allowable change in the input is for safety concerns
/// - The input pose is damped / smoothed before output
/// 
/// Use SetInputPose() to set the input pose
/// Use GetOutputPose() to get the output pose
/// </summary>
public class MotionMapping : MonoBehaviour
{
    // Tracking State
    // Use StartTracking() and StopTracking() to change the state
    // Use IsTracking() to check the state
    [SerializeField, ReadOnly] private bool tracking = false;

    // Control Mode
    //      Full        - Follow both the position and orientation
    //      Position    - Follow only the position
    public enum ControlMode {Full, Position}
    // Use SetControlMode() to change the mode
    // Use GetControlMode() to check the mode
    [SerializeField, ReadOnly]
    private ControlMode controlMode = ControlMode.Full;

    // Compensate for the orientation of the input device (relative mapping)
    // or use absolute orientation mapping with an offset
    [SerializeField] private bool compensateOrientation = true;
    // Orientation input offset
    // This is used to adjust the orientation of the input device
    // When the orientation compensation is not used
    [SerializeField] private Vector3 orientationOffset = Vector3.zero;

    // The Maximum allowable change in the input,
    // Input will be rejected if it is greater than this value
    [SerializeField] private float maxPositionChangeAllowed = 0.4f;
    [SerializeField] private float maxRotationChangeAllowed = 1.0f;
    // Unsafe input event
    // The event gets triggered when the input exceeds the maximum allowable
    public delegate void UnsafeInputHandler();
    public event UnsafeInputHandler UnsafeInputEvent;

    // Input and Output
    // Damping
    [SerializeField] private float positionSmoothDampTime = 0.1f;
    [SerializeField] private float rotationSmoothDampTime = 0.2f;
    private Vector3 positionSmoothDampVelocity = Vector3.zero;
    private Quaternion rotationSmoothDampVelocity = Quaternion.identity;

    // The input Pose
    [SerializeField, ReadOnly]
    private Vector3 inputPosition = Vector3.zero;
    [SerializeField, ReadOnly]
    private Quaternion inputRotation = Quaternion.identity;
    // The output / last pose
    [SerializeField, ReadOnly]
    private Vector3 outputPosition = Vector3.zero;
    [SerializeField, ReadOnly]
    private Quaternion outputRotation = Quaternion.identity;
    // to compensate pose
    private Vector3 diffPosition;
    private Quaternion diffRotation;
    private Vector3 compensatedPosition;
    private Quaternion compensatedRotation;

    void Start() {}

    void FixedUpdate() 
    {
        if (tracking)
        {
            ProcessTracking();
        }
    }

    private void ProcessTracking()
    {
        // Ouput Position
        // New position output after compensation
        compensatedPosition = inputPosition - diffPosition;

        // Protection agianst too big positional input changes
        // Distance between the compensated input and the last record
        float positionChange = Vector3.Distance(
            compensatedPosition, outputPosition
        );
        if (positionChange > maxPositionChangeAllowed)
        {
            // Stop tracking
            StopTracking();
            UnsafeInputEvent?.Invoke();

            Debug.Log(
                $"Position change {positionChange} exceeded maximum value."
            );
            return;
        }

        // Ouput Rotation
        if (controlMode == ControlMode.Position)
        {
            // Use fixed (last commanded) orientation
            compensatedRotation = outputRotation;
        }

        else if (controlMode == ControlMode.Full)
        {
            // Use the compensated rotation
            if (compensateOrientation)
            {
                compensatedRotation = inputRotation * diffRotation;
            }

            // Use the absolute orientation with offset
            else
            {
                compensatedRotation = Quaternion.Euler(
                    orientationOffset
                ) * inputRotation;
            }

            // Protection agianst too big rotational input changes
            // Distance between the compensated input and the last record
            float rotationChange = Quaternion.Angle(
                compensatedRotation, outputRotation
            ) * Mathf.Deg2Rad;
            if (rotationChange > maxRotationChangeAllowed)
            {
                // Stop tracking
                StopTracking();
                UnsafeInputEvent?.Invoke();

                Debug.Log(
                    $"Rotation change {rotationChange} exceeded maximum value."
                );
                return;
            }
        }

        // Compute the output pose with smoothing
        outputPosition = Vector3.SmoothDamp(
            outputPosition,
            compensatedPosition, 
            ref positionSmoothDampVelocity,
            positionSmoothDampTime
        );
        outputRotation = Utils.QuaternionSmoothDamp(
            outputRotation,
            compensatedRotation,
            ref rotationSmoothDampVelocity,
            rotationSmoothDampTime
        );
    }

    // Tracking
    public void StartTracking()
    {
        if (!tracking)
        {
            CalculateCompensation();
            tracking = true;
        }
    }

    public void StopTracking()
    {
        if (tracking)
        {
            tracking = false;
        }
    }

    // Compensation
    private void CalculateCompensation()
    {
        // This function should only be called when tracking gets started
        // Or when the mode is switched to Full
        // Stores the difference between the current pose and the 
        // last commanded output pose
        // Will be used to compensate for the misalighment
        diffPosition = inputPosition - outputPosition;
        diffRotation = Quaternion.Inverse(inputRotation) * outputRotation;
    }

    // Set current tracking input
    public void SetInputPosition(Vector3 position)
    {
        inputPosition = position;
    }

    public void SetInputRotation(Quaternion rotation)
    {
        inputRotation = rotation;
    }

    public void SetInputPose(Vector3 position, Quaternion rotation)
    {
        inputPosition = position;
        inputRotation = rotation;
    }

    // Force the output pose to a specific position and rotation
    // Could be used to home the output pose
    public void ResetOutputPose(Vector3 position, Quaternion rotation)
    {
        outputPosition = position;
        outputRotation = rotation;
        StopTracking();
    }

    // Mode change
    public void SetControlMode(ControlMode mode)
    {
        if (controlMode != mode)
        {
            // Calculate the compensation when switched to full mode
            if (mode == ControlMode.Full)
            {
                CalculateCompensation();
            }

            controlMode = mode;
        }
    }

    // Getter
    public (Vector3, Quaternion) GetOutputPose()
    {
        return (outputPosition, outputRotation);
    }

    public bool IsTracking()
    {
        return tracking;
    }

    public ControlMode GetControlMode()
    {
        return controlMode;
    }
}
