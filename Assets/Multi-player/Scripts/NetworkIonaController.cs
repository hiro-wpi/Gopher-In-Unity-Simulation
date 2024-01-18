using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
///    
/// </summary>
public class NetworkIonaController : NetworkBehaviour
{
    // Network Input
    [SerializeField] private NetworkInputActions inputActions;

    // Controllers
    [SerializeField] private BaseController baseController;
    [SerializeField] private ArmController leftArmController;
    [SerializeField] private ArmController rightArmController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ChestController chestController;

    // Switches Arm Movement Between Rotation and Translation
    // This is used only when the control input
    // for rotation and translation are the same (OnTranslateRotate())
    public enum ArmControlMode { translation = 0, rotation = 1 }
    [field: SerializeField] public ArmControlMode leftControlMode = ArmControlMode.translation;
    [field: SerializeField] public ArmControlMode rightControlMode = ArmControlMode.translation;
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
        var (actions, actionValues) = inputActions.GetInputActionsState();
        
        for (int i = 0; i < actions.Length; i++)
        {
            if (actionValues[i] == "null")
            {
                continue;
            }

            if (actions[i].name == "BaseMove")
            {
                var (value, PF, RF) = ReadValue<Vector2>(actionValues[i]);
                var linearVelocity = new Vector3(0f, 0f, value.y);
                var angularVelocity = new Vector3(0f, -value.x, 0f);
                baseController.SetVelocity(linearVelocity, angularVelocity);
            }

            else if (actions[i].name == "LeftArmHome")
            {
                var (value, PF, RF) = ReadValue<bool>(actionValues[i]);
                if (PF)
                {
                    leftArmController.HomeJoints();
                }
            }

            else if (actions[i].name == "LeftArmPreset")
            {
                var (value, PF, RF) = ReadValue<bool>(actionValues[i]);
                if (PF)
                {
                    leftArmController.MoveToPreset(0);
                }
            }

            else if (actions[i].name == "LeftArmMove")
            {
                var (value, PF, RF) = ReadValue<Vector3>(actionValues[i]);
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
                var (value, PF, RF) = ReadValue<bool>(actionValues[i]);
                if (PF)
                {
                    leftArmController.ChangeGripperStatus();
                }
            }

            else if (actions[i].name == "LeftArmSwitch")
            {
                var (value, PF, RF) = ReadValue<bool>(actionValues[i]);
                if (PF)
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
                var (value, PF, RF) = ReadValue<bool>(actionValues[i]);
                if (PF)
                {
                    rightArmController.HomeJoints();
                }
            }

            else if (actions[i].name == "RightArmPreset")
            {
                var (value, PF, RF) = ReadValue<bool>(actionValues[i]);
                if (PF)
                {
                    rightArmController.MoveToPreset(0);
                }
            }

            else if (actions[i].name == "RightArmMove")
            {
                var (value, PF, RF) = ReadValue<Vector3>(actionValues[i]);
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
                var (value, PF, RF) = ReadValue<bool>(actionValues[i]);
                if (PF)
                {
                    rightArmController.ChangeGripperStatus();
                }
            }

            else if (actions[i].name == "RightArmSwitch")
            {
                var (value, PF, RF) = ReadValue<bool>(actionValues[i]);
                if (PF)
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
                var (value, PF, RF) = ReadValue<bool>(actionValues[i]);
                if (PF)
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

                var (value, PF, RF) = ReadValue<Vector2>(actionValues[i]);
                var velocity = new Vector3(0.0f, -value.x, value.y);
                cameraController.SetVelocity(velocity);
            }

            else if (actions[i].name == "CameraHome")
            {
                var (value, PF, RF) = ReadValue<bool>(actionValues[i]);
                if (PF)
                {
                    cameraController.HomeCamera();
                }
            }

            else if (actions[i].name == "ChestTranslate")
            {
                var (value, PF, RF) = ReadValue<float>(actionValues[i]);
                chestController.SetSpeedFraction(value);
            }
        }
    }

    // Helper function
    private (T, bool, bool) ReadValue<T>(string actionValue)
    {
        // Value, WasPressedThisFrame, WasReleasedThisFrame
        var (value, PF, RF) = inputActions.GetInputActionValueAsType<T>(
            actionValue
        );
        return (value, PF, RF);
    }
}