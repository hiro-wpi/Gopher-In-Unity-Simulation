using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class ChestControl : MonoBehaviour
{
    public ChestController chestController;
    public float movementDirection = 0.0f;
    // // public float controlDelta = 0.0f;
    // public float jointSpeed = 0.1f;

    // private float chestPosition;

    public void OnMove(InputAction.CallbackContext context)
    {
        movementDirection = context.ReadValue<float>();
        // Debug.Log(movementDirection);
        // chestController.translate( movementDirection );

        chestController.VelocityControl(movementDirection);
        // chestController.testPositionControl(movementDirection);

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
