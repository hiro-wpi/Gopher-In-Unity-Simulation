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
    private Vector2 inputVelocity;
    private Vector3 linearVelocity = Vector3.zero;
    private Vector3 angularVelocity = Vector3.zero;

    void Start() {}

    // Change mode
    public void OnModeChange(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            baseController.SwitchMode();
        }
    }

    // Handle control input
    public void OnTranslate(InputAction.CallbackContext context)
    {
        // Read input
        inputVelocity = context.ReadValue<Vector2>();
        linearVelocity = new Vector3(0f, 0f, inputVelocity.y);
        angularVelocity = new Vector3(0f, -inputVelocity.x, 0f);
        // Set velocity
        baseController.SetVelocity(linearVelocity, angularVelocity);
    }

    // TODO
    // Handle autonomy input
    public void OnTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            /*
            if (!autoNavigation.active)
            {
                autoNavigation.EnableAutonomy();
            }
            else
            {
                autoNavigation.DisableAutonomy();
            }
            */
        }
    }

    // Stop base
    public void StopBase()
    {
        linearVelocity = Vector3.zero;
        angularVelocity = Vector3.zero;
        baseController.SetVelocity(linearVelocity, angularVelocity);
    }

    // Emergency Stop 
    public void EmergencyStop(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            baseController.EmergencyStop();
        }
    }

    public void EmergencyStopResume(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            baseController.EmergencyStopResume();
        }
    }
}
