using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;


/// <summary>
///     This script handles Unity input for Arm control.
/// </summary>
public class ArmControl : MonoBehaviour
{
    // Local controller
    [SerializeField] private ArmController armController;

    // Container for the speed vector
    private Vector3 linearVelocity = Vector3.zero;
    private Vector3 angularVelocity = Vector3.zero;
    private float gripperSpeed = 0f;

    void Start() { }
    
    // TODO //
    // PRESET
    public void OnHome(InputAction.CallbackContext context)
    {
        if (context.performed)
            armController.MoveToPreset(0);
    }
    public void OnPreset1(InputAction.CallbackContext context)
    {
        if (context.performed)
            armController.MoveToPreset(1);
    }
    public void OnPreset2(InputAction.CallbackContext context)
    {
        if (context.performed)
            armController.MoveToPreset(2);
    }
    public void OnPreset3(InputAction.CallbackContext context)
    {
        if (context.performed)
            armController.MoveToPreset(3);
    }
    public void OnPreset4(InputAction.CallbackContext context)
    {
        if (context.performed)
            armController.MoveToPreset(4);
    }
    public void OnPreset5(InputAction.CallbackContext context)
    {
        if (context.performed)
            armController.MoveToPreset(5);
    }

    // End effector control
    public void OnTranslate(InputAction.CallbackContext context)
    {
        // Read input
        linearVelocity = context.ReadValue<Vector3>();
        // Set velocity
        armController.SetLinearVelocity(linearVelocity);
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        // Read input
        angularVelocity = context.ReadValue<Vector3>();
        // Set velocity
        armController.SetAngularVelocity(angularVelocity);
    }    

    // Gripper
    public void OnGripper(InputAction.CallbackContext context)
    {
        // Read input
        gripperSpeed = context.ReadValue<Vector2>().y;
        // Set velocity
        armController.SetGripperSpeed(gripperSpeed);
    }

    // Stop
    public void StopArm()
    {
        linearVelocity = Vector3.zero;
        angularVelocity = Vector3.zero;
        armController.SetLinearVelocity(linearVelocity);
        armController.SetAngularVelocity(angularVelocity);
    }

    public void StopGripper()
    {
        gripperSpeed = 0f;
        armController.SetGripperSpeed(gripperSpeed);
    }

    // Change mode
    public void OnModeChange(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            armController.SwitchMode();
        }
    }
    
    // TODO //
    // Handle autonomy input
    public void OnTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // armController.MoveToTarget(
            //    null, automationSpeed, false, false
            // );
        }
    }
}
