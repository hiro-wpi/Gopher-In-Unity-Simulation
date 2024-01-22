using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;
using UnityEngine.XR;

/// <summary>
///     
/// </summary>
public class IonaXRNetworkController : NetworkBehaviour
{
    // Network Input
    [SerializeField] private NetworkInputActions inputActions;

    // Controllers
    [SerializeField] private BaseController baseController;
    [SerializeField] private ArmController leftArmController;
    [SerializeField] private ArmController rightArmController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ChestController chestController;

    // Motion Mapping
    [SerializeField] private VRMotionMapping leftVRMotionMapping;
    [SerializeField] private VRMotionMapping rightVRMotionMapping;

    public override void OnNetworkSpawn()
    {
        // Only non-owner server needs this to control the robot
        if (!IsServer || IsOwner)
        {
            this.enabled = false;
        }
    }

    void Update()
    {
        // New data not ready yet
        if (!inputActions.DataReceived)
        {
            return;
        }

        // Get the input actions and values
        var (actions, actionValues) = inputActions.GetInputActionsState();

        for (int i = 0; i < actions.Length; i++)
        {
            if (actions[i].name == "BaseMove")
            {
                if (actionValues[i] == "Null")
                {
                    baseController.SetVelocity(Vector3.zero, Vector3.zero);
                    continue;
                }

                var (value, P, R) = ReadValue<Vector2>(actionValues[i]);
                var linearVelocity = new Vector3(0f, 0f, value.y);
                var angularVelocity = new Vector3(0f, -value.x, 0f);
                baseController.SetVelocity(linearVelocity, angularVelocity);
            }

            else if (actions[i].name == "LeftArmHome")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    leftArmController.HomeJoints();
                }
            }

            else if (actions[i].name == "LeftArmTranslate")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<Vector3>(actionValues[i]);
                leftVRMotionMapping.SetInputPosition(value);
            }

            else if (actions[i].name == "LeftArmRotate")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<Quaternion>(actionValues[i]);
                leftVRMotionMapping.SetInputRotation(value);
            }

            else if (actions[i].name == "LeftArmGrasp")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    leftArmController.ChangeGripperStatus();
                }
            }

            else if (actions[i].name == "LeftArmSwitch")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    leftVRMotionMapping.ChangeTrackingState();
                }
                if (R
                    && leftVRMotionMapping.Mode == 
                        VRMotionMapping.TrackingMode.Hold
                ) {
                    leftVRMotionMapping.StopTracking();
                }
            }

            else if (actions[i].name == "LeftArmPrimaryAction")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    leftVRMotionMapping.ControlModeSwitch();
                }
            }

            else if (actions[i].name == "RightArmHome")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    rightArmController.HomeJoints();
                }
            }

            else if (actions[i].name == "RightArmTranslate")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<Vector3>(actionValues[i]);
                rightVRMotionMapping.SetInputPosition(value);
            }

            else if (actions[i].name == "RightArmRotate")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<Quaternion>(actionValues[i]);
                rightVRMotionMapping.SetInputRotation(value);
            }

            else if (actions[i].name == "RightArmGrasp")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    rightArmController.ChangeGripperStatus();
                }
            }

            else if (actions[i].name == "RightArmSwitch")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    rightVRMotionMapping.ChangeTrackingState();
                }
                if (R
                    && rightVRMotionMapping.Mode == 
                        VRMotionMapping.TrackingMode.Hold
                ) {
                    rightVRMotionMapping.StopTracking();
                }
            }

            else if (actions[i].name == "RightArmPrimaryAction")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    rightVRMotionMapping.ControlModeSwitch();
                }
            }

            else if (actions[i].name == "CameraToggle")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }
            }

            else if (actions[i].name == "CameraRotate")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }
            }

            else if (actions[i].name == "ChestTranslate")
            {
                if (actionValues[i] == "Null")
                {
                    chestController.SetSpeedFraction(0);
                    continue;
                }

                var (value, P, R) = ReadValue<float>(actionValues[i]);
                chestController.SetSpeedFraction(value);
            }
        }
    }

    // Helper function
    private (T, bool, bool) ReadValue<T>(string actionValue)
    {
        // Value, WasPressedThisFrame, WasReleasedThisFrame
        var (value, P, R) = inputActions.GetInputActionValueAsType<T>(
            actionValue
        );
        return (value, P, R);
    }
}
