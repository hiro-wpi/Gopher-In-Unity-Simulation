using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

/// <summary>
///     This script handles Unity input for Chest control.
/// </summary>
public class ChestControl : MonoBehaviour
{
    [SerializeField] private ChestController chestController;
    private float driveDirection;

    // Simulating input lagging (for simulation only)
    [SerializeField] private float simulationInputLagMean = 100f;  // ms
    [SerializeField] private float simulationInputLagStd = 25f;  // ms

    void Start() 
    {
        // No need to simulate input lagging if controlling the real robot
        if (chestController is PhysicalChestController)
        {
            simulationInputLagMean = 0;
            simulationInputLagStd = 0;
        }
    }

    void Update() {}

    // Moves the base up and down - "velocity" controller
    public void OnTranslate(InputAction.CallbackContext context)
    {
        driveDirection = context.ReadValue<float>();
        // Set velocity
        StartCoroutine(DelayAndSetSpeedCoroutine(driveDirection));
    }

    IEnumerator DelayAndSetSpeedCoroutine(float driveDirection)
    {
        // Simulate input lagging
        if (simulationInputLagMean > 0)
        {
            float delay = Utils.GenerateGaussianRandom(
                simulationInputLagMean, simulationInputLagStd
            ) / 1000f;
            yield return new WaitForSeconds(delay);
        }
        // Set velocity
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
