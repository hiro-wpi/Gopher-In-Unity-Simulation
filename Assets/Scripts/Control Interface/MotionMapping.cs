using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
// TODO
// Add damping to the output pose
/// </summary>
public class MotionMapping : MonoBehaviour
{
    // Tracking State
    [SerializeField, ReadOnly] private bool tracking = false;

    // Control Mode
    //      Full        - Follow both the position and orientation
    //      Position    - Follow only the position
    public enum ControlMode {Full, Position}
    [SerializeField, ReadOnly]
    private ControlMode controlMode = ControlMode.Full;

    // Compensate for the orientation of the input device
    // or use absolute orientation
    [SerializeField] private bool compensateOrientation = true;

    // Orientation input offset
    // This is used to adjust the orientation of the input device
    // When the orientation compensation is not used
    [SerializeField] private Vector3 orientationOffset = Vector3.zero;

    // The Maximum allowable change in the input, 
    // Input will be rejected if it is greater than this value
    [SerializeField] private float maxPositionChangeAllowed = 0.1f;
    [SerializeField] private float maxRotationChangeAllowed = 0.2f;

    // The input Pose
    private Vector3 inputPosition;
    private Quaternion inputRotation;
    // The output / last pose
    [SerializeField, ReadOnly] 
    private Vector3 outputPosition = Vector3.zero;
    [SerializeField, ReadOnly]
    private Quaternion outputRotation = Quaternion.identity;
    // compensate pose
    private Vector3 diffPosition;
    private Quaternion diffRotation;
    private Vector3 compensatedPosition;
    private Quaternion compensatedRotation;

    // Unsafe input event
    public delegate void UnsafeInputHandler();
    public event UnsafeInputHandler UnsafeInputEvent;

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

        // Store as the output pose
        outputPosition = compensatedPosition;
        outputRotation = compensatedRotation;
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
        //
        // It stores the difference between the current pose and the 
        // last commanded output pose
        // Will be used to compensate for the misalighment
        diffPosition = inputPosition - outputPosition;
        diffRotation = Quaternion.Inverse(inputRotation) * outputRotation;
    }

    // Set current tracking input
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
