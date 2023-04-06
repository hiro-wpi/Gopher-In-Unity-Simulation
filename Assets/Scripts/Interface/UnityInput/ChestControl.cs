using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class ChestControl : MonoBehaviour
{
    public ChestController chestController;
    private float driveDirection = 0.0f;

    void Start() {}

    public void OnMove(InputAction.CallbackContext context)
    {
        driveDirection = context.ReadValue<Vector3>().z;
        chestController.SetSpeedFraction(driveDirection);
    }   

    public void OnHome(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            chestController.HomeChest();
        }
    }

    public void OnPreset1(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            chestController.MoveToPreset(1);
        }
    }

    public void OnPreset2(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            chestController.MoveToPreset(2);
        }
    }

    public void OnPreset3(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            chestController.MoveToPreset(3);
        }
    }

    public void StopChest()
    {
        chestController.StopChest();
    }
}
