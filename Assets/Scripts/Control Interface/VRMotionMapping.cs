using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Use events performed, canceled
/// Use isPressed() instead of state machine
/// </summary>
public class VRMotionMapping : MonoBehaviour
{
    // The motion mapping
    [SerializeField] private MotionMapping motionMapping;

    // Relevant Inputs from the VR Controller        
    // tracking change button
    [SerializeField] private InputActionProperty trackingButton;
    // control mode change button
    [SerializeField] private InputActionProperty modeButton;
    // controller position and rotation   
    [SerializeField] private InputActionProperty controllerPosition;
    [SerializeField] private InputActionProperty controllerRotation;
    // delegate for action recalled function
    private Action<InputAction.CallbackContext> changeTrackingStateDelegate;
    private Action<InputAction.CallbackContext> controlModeSwitchDelegate;
    private Action<InputAction.CallbackContext> stopTrackingDelegate;

    // Tracking Mode - Alternates between Holding or Toggle mode
    //      Hold    - Tracking while the select button is being pressed
    //      Toggle  - Toggle on and off the tracking with the select button
    public enum TrackingMode {Hold, Toggle}
    [SerializeField] private TrackingMode trackingMode = TrackingMode.Toggle;

    // Debug purpose
    [SerializeField] private Transform outputTransform;

    void Start() 
    {
        // Stop motion mapping at the beginning
        motionMapping.StopTracking();
    }

    void OnEnable() 
    {
        // Set up the delegate
        changeTrackingStateDelegate = ctx => ChangeTrackingState();
        controlModeSwitchDelegate = ctx => ControlModeSwitch();
        stopTrackingDelegate = ctx => StopTracking();

        // Set up the input action with the delegate function
        // when button pressed
        trackingButton.action.performed += changeTrackingStateDelegate;
        modeButton.action.performed += controlModeSwitchDelegate;
        // when button realeased
        if (trackingMode == TrackingMode.Hold)
        {
            trackingButton.action.canceled += stopTrackingDelegate;
        }
    }

    void OnDisable()
    {
        trackingButton.action.performed -= changeTrackingStateDelegate;
        modeButton.action.performed -= controlModeSwitchDelegate;
        trackingButton.action.canceled -= stopTrackingDelegate;
    }

    void Update()
    {
        // Update the input pose
        motionMapping.SetInputPose(
            controllerPosition.action.ReadValue<Vector3>(),
            controllerRotation.action.ReadValue<Quaternion>()
        );

        // For debug purpose
        if (outputTransform != null)
        {
            (outputTransform.position, outputTransform.rotation) = 
            motionMapping.GetOutputPose();
        }
    }

    // Tracking event
    private void ChangeTrackingState()
    {
        if (motionMapping.IsTracking())
        {
            motionMapping.StopTracking();
        }
        else
        {
            motionMapping.StartTracking();
        }
    }

    private void StopTracking()
    {
        motionMapping.StopTracking();
    }

    // Mode event
    private void ControlModeSwitch()
    {
        if (motionMapping.GetControlMode() == MotionMapping.ControlMode.Full)
        {
            motionMapping.SetControlMode(MotionMapping.ControlMode.Position);
        }
        else if (
            motionMapping.GetControlMode() 
            == MotionMapping.ControlMode.Position
        ) {
            motionMapping.SetControlMode(MotionMapping.ControlMode.Full);
        }
    }
}
