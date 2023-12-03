using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

/// <summary>
/// Use events performed, canceled
/// Use isPressed() instead of state machine
/// </summary>
public class VRMotionMapping : MotionMapping
{
    // Relevant Inputs from the VR Controller        
    // select = Grip Button
    [SerializeField] private InputActionProperty select;  
    // activate = Trigger Button
    // [SerializeField] private InputActionProperty activate;
    // primaryButton = A and X
    [SerializeField] private InputActionProperty primaryButton;      
    [SerializeField] private InputActionProperty controllerPosition;
    [SerializeField] private InputActionProperty controllerRotation;

    // Stored controller Values
    private int selectValue;
    // private int activateValue;
    private int primaryButtonValue;

    // State Machine States
    private int trackingState = 0;
    // Tracking Mode - Alternates between Holding or Toggle mode
    //      Hold    - Tracking while the select button is being pressed
    //      Toggle  - Toggle on and off the tracking with the select button
    public enum TrackingMode {Hold, Toggle}
    [SerializeField] private TrackingMode trackingMode = TrackingMode.Toggle;

    // Debug
    public Transform outputTransform;

    void Start() 
    {
        UnsafeInputEvent += UnsafeInputCallback;
    }

    void OnDestroy()
    {
        UnsafeInputEvent -= UnsafeInputCallback;
    }

    void Update()
    {
        // Read the value of the action inputs
        selectValue = select.action.IsPressed() ? 1 : 0;
        primaryButtonValue = primaryButton.action.IsPressed() ? 1 : 0;
        TrackingStateMachine(selectValue);
        ControlModeSwitch(primaryButtonValue);

        // Update the input pose
        SetInputPose(
            controllerPosition.action.ReadValue<Vector3>(),
            controllerRotation.action.ReadValue<Quaternion>()
        );

        (outputTransform.position, outputTransform.rotation) = GetOutputPose();
    }

    // Mode change
    private void ControlModeSwitch(int button)
    {
        // Flip mode if button pressed
        if (button == 1)
        {
            if (GetControlMode() == ControlMode.Full)
            {
                SetControlMode(ControlMode.Position);
            }
            else if (GetControlMode() == ControlMode.Position)
            {
                SetControlMode(ControlMode.Full);
            }
        }
    }

    // Tracking quit
    private void UnsafeInputCallback()
    {
        trackingState = 0;
    }

    // Tracking State Machines
    // State 0 - Button is released. Not tracking.
    //           If button is pressed, move to state 1, start tracking, 
    // State 1 - Button is pressed. Tracking.
    //           - If in Hold mode and button is released,
    //             move to state 0, stop tracking,
    //           - If in Toggle mode and button is released,
    //             move to state 2, keep tracking, 
    // State 2 - Button is released. Tracking.
    //           If button is pressed again, move to state 3, stop tracking, 
    // State 3 - Button is pressed. Not tracking.
    //           If button is released, move back to state 0
    private void TrackingStateMachine(int button)
    {
        // State 0 - Button is released. Not tracking.
        //           If button is pressed, move to state 1, start tracking, 
        if (trackingState == 0)
        {
            if (button == 1)
            {
                // Move to state 1
                trackingState = 1;

                // Start tracking
                StartTracking();
            }
        }

        // State 1 - Button is pressed. Tracking.
        //           - If in Hold mode and button is released,
        //             move to state 0, stop tracking,
        //           - If in Toggle mode and button is released,
        //             move to state 2, keep tracking, 
        else if (trackingState == 1)
        {
            if (button == 0)
            {
                if (trackingMode == TrackingMode.Hold)
                {
                    // Move to state 0
                    trackingState = 0;

                    // Stop tracking
                    StopTracking();
                }
                else if(trackingMode == TrackingMode.Toggle)
                {
                    // Move to state 2
                    trackingState = 2;
                }
            }
        }
        
        // State 2 - Button is released. Tracking.
        //           If button is pressed again, move to state 3, stop tracking,  
        else if (trackingState == 2)
        {
            if (button == 1)
            {
                // Move to state 3
                trackingState = 3;

                // Stop tracking
                StopTracking();
            }
        }

        // State 3 - Button is pressed. Not tracking.
        //           If button is released, move back to state 0
        else if (trackingState == 3)
        {
            if (button == 0)
            {
                trackingState = 0;
            }
        }
    }
}
