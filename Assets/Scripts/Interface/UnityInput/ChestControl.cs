using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

/// <summary>
///     This script handles Unity input for Chest control.
/// </summary>
public class ChestControl : MonoBehaviour
{
    public ChestController chestController;
    private float driveDirection;

    void Start() {}

    // Moves the base up and down - "velocity" controller
    public void OnTranslate(InputAction.CallbackContext context)
    {
        driveDirection = context.ReadValue<float>();
        chestController.SetSpeedFraction(driveDirection);
    }

    public void StopChest()
    {
        chestController.StopChest();
    }

    // Home the chest
    public void OnHome(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            chestController.HomeChest();
        }
    }

    // Send chest to preset positions
    public void OnPreset1(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            chestController.MoveToPreset(0);
        }
    }

    public void OnPreset2(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            chestController.MoveToPreset(1);
        }
    }

    public void OnPreset3(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            chestController.MoveToPreset(2);
        }
    }

    // Emergency Stop 
    public void EmergencyStop(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            chestController.EmergencyStop();
        }
    }

    public void EmergencyStopResume(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            chestController.EmergencyStopResume();
        }
    }
}
