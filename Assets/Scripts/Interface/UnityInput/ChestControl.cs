using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class ChestControl : MonoBehaviour
{
    public ChestController chestController;
    private float driveDirection = 0.0f;

    void Start() {}

    // Moves the base up and down ("velocity" controller)
    public void OnMove(InputAction.CallbackContext context)
    {
        // Debug.Log("ChestControl << OnMove Function");
        driveDirection = context.ReadValue<Vector3>().z;
        // Debug.Log(driveDirection);
        chestController.SetSpeedFraction(driveDirection);
    }   

    // Home the chest
    public void OnHome(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Debug.Log("ChestContol << OnHome Function");
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

    // Recommended Emergency Stop 
    public void StopChest()
    {
        chestController.StopChest();
    }

    // Error with Intergration
    // Once the breaker connected to the aux port is turned of, the system can not be remotely be reactivated
    // public void OnChestBreaker(InputAction.CallbackContext context)
    // {
    //     if(context.performed)
    //     {
    //         float input = context.ReadValue<float>();
    //         chestController.ChestBreaker(input);
    //     }
        
    // }


}
