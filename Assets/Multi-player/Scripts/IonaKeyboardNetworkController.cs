using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

/// <summary>
///     
/// </summary>
public class IonaKeyboardNetworkController : NetworkBehaviour
{
    // Network Input
    [SerializeField] private NetworkInputActions inputActions;

    // Controllers
    [Header("Robot Controllers")]
    [SerializeField] private BaseController baseController;
    [SerializeField] private ArmController leftArmController;
    [SerializeField] private ArmController rightArmController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ChestController chestController;

    // Switches Arm Movement Between Rotation and Translation
    // This is used only when the control input
    // for rotation and translation are the same (OnTranslateRotate())
    public enum ArmControlMode { translation = 0, rotation = 1 }
    [Header("Controller Modes")]
    [SerializeField, ReadOnly]
    private ArmControlMode leftControlMode = ArmControlMode.translation;
    [SerializeField, ReadOnly]
    private ArmControlMode rightControlMode = ArmControlMode.translation;
    [SerializeField, ReadOnly] private bool isCameraActive = false;

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

            else if (actions[i].name == "LeftArmPreset")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    leftArmController.MoveToPreset(0);
                }
            }

            else if (actions[i].name == "LeftArmMove")
            {
                if (actionValues[i] == "Null")
                {
                    leftArmController.SetLinearVelocity(Vector3.zero);
                    leftArmController.SetAngularVelocity(Vector3.zero);
                    continue;
                }

                var (value, P, R) = ReadValue<Vector3>(actionValues[i]);
                if(leftControlMode == ArmControlMode.translation)
                {
                    leftArmController.SetLinearVelocity(value);
                }
                else if(leftControlMode == ArmControlMode.rotation)
                {
                    // Rotation uses the same keys as translation
                    // Need to convert axis
                    value = new Vector3(-value.z, value.x, -value.y);
                    leftArmController.SetAngularVelocity(value);
                }
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
                    if(leftControlMode == ArmControlMode.translation)
                    {
                        leftControlMode = ArmControlMode.rotation;
                    }
                    else if(leftControlMode == ArmControlMode.rotation)
                    {
                        leftControlMode = ArmControlMode.translation;
                    }
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

            else if (actions[i].name == "RightArmPreset")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    rightArmController.MoveToPreset(0);
                }
            }

            else if (actions[i].name == "RightArmMove")
            {
                if (actionValues[i] == "Null")
                {
                    rightArmController.SetLinearVelocity(Vector3.zero);
                    rightArmController.SetAngularVelocity(Vector3.zero);
                    continue;
                }

                var (value, P, R) = ReadValue<Vector3>(actionValues[i]);
                if(rightControlMode == ArmControlMode.translation)
                {
                    rightArmController.SetLinearVelocity(value);
                }
                else if(rightControlMode == ArmControlMode.rotation)
                {
                    // Rotation uses the same keys as translation
                    // Need to convert axis
                    value = new Vector3(-value.z, value.x, -value.y);
                    rightArmController.SetAngularVelocity(value);
                }
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
                    if(rightControlMode == ArmControlMode.translation)
                    {
                        rightControlMode = ArmControlMode.rotation;
                    }
                    else if(rightControlMode == ArmControlMode.rotation)
                    {
                        rightControlMode = ArmControlMode.translation;
                    }
                }
            }

            else if (actions[i].name == "CameraToggle")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    isCameraActive = !isCameraActive;
                    if (!isCameraActive)
                    {
                        cameraController.SetVelocity(Vector3.zero);
                    }
                }
            }

            else if (actions[i].name == "CameraRotate")
            {
                if (!isCameraActive)
                {
                    continue;
                }
                if (actionValues[i] == "Null")
                {
                    cameraController.SetVelocity(Vector3.zero);
                    continue;
                }

                var (value, P, R) = ReadValue<Vector2>(actionValues[i]);
                var velocity = new Vector3(0.0f, -value.x, value.y);
                cameraController.SetVelocity(velocity);
            }

            else if (actions[i].name == "CameraHome")
            {
                if (actionValues[i] == "Null")
                {
                    continue;
                }

                var (value, P, R) = ReadValue<bool>(actionValues[i]);
                if (P)
                {
                    cameraController.HomeCamera();
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