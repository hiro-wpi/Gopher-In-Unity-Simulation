using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class BaseControl : MonoBehaviour
{
    public ArticulationWheelController wheelController;
    public AutoNavigation autoNavigation;
    public float linearSpeed = 1.5f;
    public float angularSpeed = 1.5f;
    private Vector3 _driveDir = Vector3.zero;

    public void OnDrive(InputAction.CallbackContext context)
    {
        _driveDir = context.ReadValue<Vector3>();
        wheelController.SetRobotVelocity(_driveDir.z * linearSpeed, _driveDir.x * -angularSpeed);
    }

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

    public void StopBase()
    {
        _driveDir = Vector2.zero;
        wheelController.SetRobotVelocity(0f, 0f);
    }
}
