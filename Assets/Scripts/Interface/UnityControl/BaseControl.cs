using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class BaseControl : MonoBehaviour
{
    public ArticulationWheelController wheelController;
    public float linearSpeed = 1.5f;
    public float angularSpeed = 1.5f;
    private Vector2 _driveDir = Vector2.zero;

    public void OnDrive(InputAction.CallbackContext context)
    {
        _driveDir = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        wheelController.SetRobotVelocity(_driveDir.y * linearSpeed, _driveDir.x * -angularSpeed);
    }
}
