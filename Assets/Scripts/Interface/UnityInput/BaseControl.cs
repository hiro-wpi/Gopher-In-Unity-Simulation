using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

/// <summary>
///     This script handles Unity input for Base control.
/// </summary>
public class BaseControl : MonoBehaviour
{
    // Local controller and autonomy
    [SerializeField] private BaseController baseController;
    [SerializeField] private AutoNavigation autoNavigation;

    // Container for the speed vector
    private Vector3 linearVelocity = Vector3.zero;
    private Vector3 angularVelocity = Vector3.zero;

    void Start() {}

    // Handle control input
    public void OnDrive(InputAction.CallbackContext context)
    {
        // Read input
        Vector3 driveDir = context.ReadValue<Vector3>();
        linearVelocity = new Vector3(0f, 0f, driveDir.z);
        angularVelocity = new Vector3(0f, -driveDir.x, 0f);
        // Set velocity
        baseController.SetVelocity(linearVelocity, angularVelocity);
    }

    // Stop base
    public void StopBase()
    {
        linearVelocity = Vector3.zero;
        angularVelocity = Vector3.zero;
        baseController.SetVelocity(linearVelocity, angularVelocity);
    }

    // Change mode
    public void OnModeChange(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            baseController.SwitchMode();
        }
    }

    // TODO //
    // Handle autonomy input
    public void OnTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!autoNavigation.active)
            {
                autoNavigation.EnableAutonomy();
            }
            else
            {
                autoNavigation.DisableAutonomy();
            }
        }
    }
    
    // Breaker
    public void OnBaseBreaker(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            float input = context.ReadValue<float>();
            baseController.BaseBreaker(input);
        }
        
    }


}
