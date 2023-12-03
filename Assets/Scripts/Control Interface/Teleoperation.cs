using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

/// <summary>
/// Class <c>Teleoperation</c> Assuming that the Oculus Quest 2 is used, this system handles the position control of the Kinova arm's end-effector (EE).
/// </summary>
public class Teleoperation : MonoBehaviour
{
    /// <summary>
    /// Class <c>_Transform</c> Holds the position and rotation, simularly to Unity's Transform class.
    /// </summary>
    private class _Transform
    {
        public Vector3 position;
        public Quaternion rotation;

        public _Transform(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public _Transform()
        {
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
        }
    }

    // Object that will temperarily represent the position of the Kinova EE
    [SerializeField] private Transform trackingTransformObject;

    // Relevant Inputs from the Controller                           
    // [Below] is the relative button on the Oculus that corresponds to these input actions
    [SerializeField] private InputActionProperty select;             // select          = Grip Button
    [SerializeField] private InputActionProperty activate;           // activate        = Trigger Button
    [SerializeField] private InputActionProperty primaryButton;      // primaryButton   = A and X
    [SerializeField] private InputActionProperty controllerPosition;
    [SerializeField] private InputActionProperty controllerRotation;

    // Stored Location of Controller Values
    private int selectValue;
    private int primaryButtonValue;
    private Vector3 controllerPositionValue;
    private Quaternion controllerRotationValue;

    // The Pose Sent to the Kinova EE
    [SerializeField] private Vector3 outputPositionValue;
    [SerializeField] private Quaternion outputRotationValue;

    // _Transforms used to control the output pose
    private _Transform inputPose = new _Transform();
    private _Transform lastRelaxedIKPose = new _Transform();
    private _Transform inputRelaxedIKDifference = new _Transform();

    // Flag that handles when to follow the position of the controller, following when True, stopping when False
    [SerializeField] private bool poseTracking = false;

    // State Machine States
    private int trackingState = 0;
    private int modeState = 0;

    // Tracking Mode - Alternates between Holding or Toggle based on the trackkingState
    //      Hold    - Kinova's EE only moves while the select button (grip button) is being pressed, stopping otherwise
    //      Toggle  - Allows the user to switch between having Kinova's EE follow the controllers, and stoping, with the press of the select button (Grip Button)
    public enum TrackingMode
    {
        Hold,
        Toggle
    }
    // #TODO the tracking mode is something that is initcialized in the beginning
    //[SerializeField, ReadOnly] private TrackingMode trackingMode = TrackingMode.Toggle;
    [SerializeField] private TrackingMode trackingMode = TrackingMode.Toggle;

    // Mode State Machine - Controls the EE pose in respect to the controllers
    //      Full        - Kinova's EE tracks the pose of the controllers 
    //      Position    - Kinova's EE tracks ONLY the position of the controllers, orienation is ignored
    public enum ControlMode
    {
        Full,
        Position
    }
    [SerializeField, ReadOnly] private ControlMode controlMode = ControlMode.Full;

    // CONVENIENCE_COMPENSATION - An rotational offset [roll, pitch, yaw] that will be added to the output pose sent to the Kinova's EE.
    //      Was considered to make controlling the robot more comfortable to the user
    [SerializeField] private Vector3 CONVENIENCE_COMPENSATION = Vector3.zero;
        
    [SerializeField] private bool COMPENSATE_ORIENTATION = true;

    // MAXIMUM_INPUT_CHANGE - The Maximum allowable change in the input, before being rejected
    static private float MAXIMUM_INPUT_CHANGE = 0.1f;

    void Start() {}

    void Update()
    {
        // Read the value of the action inputs
        selectValue = (int)Mathf.Round(select.action.ReadValue<float>());
        primaryButtonValue = (int)Mathf.Round(primaryButton.action.ReadValue<float>());

        controllerPositionValue = controllerPosition.action.ReadValue<Vector3>();
        controllerRotationValue = controllerRotation.action.ReadValue<Quaternion>();

        // Pass Button Values to the State Machines to determine the behavior of the robot
        TrackingStateMachine(selectValue);
        ModeStateMachine(primaryButtonValue);

        inputPose.position = controllerPositionValue;
        inputPose.rotation = controllerRotationValue;

        if (poseTracking)
        { 
            PublishKinovaPose();
        }
        
    }

    public (Vector3, Quaternion) GetTrackingPose()
    {
        return (outputPositionValue, outputRotationValue);
    }

    #region Tracking State Machines
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
                CalculateCompensation();
                poseTracking = true;
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
                    poseTracking = false;
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
                poseTracking = false;
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
    #endregion

    #region Mode State Machines
    private void ModeStateMachine(int button)
    {
        switch (modeState)
        {
            // State 0: Button is pressed
            case 0:
                if (button == 1)
                {
                    modeState = 1;
                    controlMode = ControlMode.Full;
                    CalculateCompensation();
                }    
                break;

            // State 1: Button is released.
            case 1:
                
                if (button == 0)
                {
                    modeState = 2;
                }
                break;

            // State 2: Button is pressed.
            case 2:
                if (button == 1)
                {
                    modeState = 3;
                    controlMode = ControlMode.Position; 
                }
                break;
            
            // State 3: Button is released.
            case 3:
                if(button == 0)
                {
                    modeState = 0;
                }
                break;
        }
    }
    #endregion

    ///////////////////////////////////////////////////
    // Other functions
    private void CalculateCompensation() 
    {
        // Calculates the compensation for the coodinate system misalignment
        inputRelaxedIKDifference.position = inputPose.position - lastRelaxedIKPose.position;
        inputRelaxedIKDifference.rotation = Quaternion.Inverse(inputPose.rotation) * lastRelaxedIKPose.rotation;
        Debug.Log("CalculateCompensation()");
        Debug.Log(inputPose.position.ToString("F3"));
        Debug.Log(lastRelaxedIKPose.position.ToString("F3"));
        Debug.Log(inputRelaxedIKDifference.position.ToString("F3"));
    }

    private void PublishKinovaPose()
    {
        // Handles protectiona gainst 0,0,0, controller input values
        // controller loses connection, goes into sleeep mode ect.

        if (inputPose.position == Vector3.zero && poseTracking)
        {
            // Stop tracking
            trackingState = 0;
            poseTracking = false;

            Debug.Log("Recieved (0,0,0) while actively tracking! Stopped input tracking. " + inputPose.position.ToString());

            return;
        }

        _Transform compensatedInputPose = new _Transform();

        compensatedInputPose.position = inputPose.position - inputRelaxedIKDifference.position;

        // Protection agianst too big positional input changes
        // Controller loses connection, out-of-sight, goes into a sleep mode etc.

        // Distance between the compensated input and the last record pose
        float inputPositionDifference = Vector3.Distance(compensatedInputPose.position, lastRelaxedIKPose.position);

        Debug.Log("");
        Debug.Log(inputPose.position.ToString("F3"));
        Debug.Log(inputRelaxedIKDifference.position.ToString("F3"));
        Debug.Log(compensatedInputPose.position.ToString("F3"));
        Debug.Log(lastRelaxedIKPose.position.ToString("F3"));

        if (inputPositionDifference > MAXIMUM_INPUT_CHANGE)
        {
            // Stop tracking.
            trackingState = 0;
            poseTracking = false;

            Debug.Log("MAXIMUM_INPUT_CHANGE in inputRelaxedIKDifference position exceeded maximum allowed value: " + inputPositionDifference.ToString());
            
            return;
        }

        // Use fixed (last commanded) orientation
        compensatedInputPose.rotation = lastRelaxedIKPose.rotation;

        // Use oculus orientation
        if (controlMode == ControlMode.Full)
        {
            compensatedInputPose.rotation = inputPose.rotation;

            if (COMPENSATE_ORIENTATION)
            {
                // This is incorrect
                compensatedInputPose.rotation = inputPose.rotation * inputRelaxedIKDifference.rotation;
            }
            else
            {
                // Conveniece orientation corrections
                Quaternion convenienceQuaternion = Quaternion.Euler(CONVENIENCE_COMPENSATION.x, CONVENIENCE_COMPENSATION.y, CONVENIENCE_COMPENSATION.z);
                compensatedInputPose.rotation = inputPose.rotation * convenienceQuaternion;
            }
        }

        // Store the output into these varibles for debugging
        outputPositionValue = compensatedInputPose.position;
        outputRotationValue = compensatedInputPose.rotation;

        // Send the output directly to the gameobject:
        trackingTransformObject.position = compensatedInputPose.position;
        trackingTransformObject.rotation = compensatedInputPose.rotation;

        // Save the last valid pose
        lastRelaxedIKPose = compensatedInputPose;
    }
}
