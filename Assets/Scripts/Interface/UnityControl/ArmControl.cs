using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;

public class ArmControl : MonoBehaviour
{
    public ArmControlManager armControlManager;

    // For CONTROL - velocity control
    public float translateSpeed = 0.004f; // TODO what is this speed?
    public float rotateSpeed = 0.2f;

    // For TARGET - Automatic grasping
    public float automationSpeed = 0.05f;

    void Start()
    {}

    
    // PRESET
    public void OnHome(InputAction.CallbackContext context)
    {
        if (context.performed)
            armControlManager.MoveToPreset(0);
    }
    public void OnPreset1(InputAction.CallbackContext context)
    {
        if (context.performed)
            armControlManager.MoveToPreset(1);
    }
    public void OnPreset2(InputAction.CallbackContext context)
    {
        if (context.performed)
            armControlManager.MoveToPreset(2);
    }
    public void OnPreset3(InputAction.CallbackContext context)
    {
        if (context.performed)
            armControlManager.MoveToPreset(3);
    }
    public void OnPreset4(InputAction.CallbackContext context)
    {
        if (context.performed)
            armControlManager.MoveToPreset(4);
    }
    public void OnPreset5(InputAction.CallbackContext context)
    {
        if (context.performed)
            armControlManager.MoveToPreset(5);
    }


    // IK
    public void OnTranslate(InputAction.CallbackContext context)
    {
        armControlManager.deltaPosition = -context.ReadValue<Vector3>() * 
                                           translateSpeed;
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        armControlManager.deltaRotation = -context.ReadValue<Vector3>() * 
                                           rotateSpeed;
    }    
    
    public void OnGrasp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            armControlManager.ChangeGripperStatus();
        }
    }

    public void OnTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            armControlManager.MoveToTarget(null, automationSpeed, 
                                           false, false);
        }
    }


    public void StopArm()
    {
        armControlManager.deltaPosition = Vector3.zero;
        armControlManager.deltaRotation = Vector3.zero;
    }
}