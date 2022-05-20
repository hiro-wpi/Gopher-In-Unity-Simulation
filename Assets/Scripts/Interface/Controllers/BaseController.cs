using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    public ArticulationWheelController articulationWheelController;
    public float moveSpeed;
    public float rotateSpeed;
    private Vector2 _driveDir = Vector2.zero;

    public void OnDrive(InputAction.CallbackContext context)
    {
        _driveDir = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        articulationWheelController.SetRobotVelocity(_driveDir.y * moveSpeed, _driveDir.x * -rotateSpeed);
    }
}
