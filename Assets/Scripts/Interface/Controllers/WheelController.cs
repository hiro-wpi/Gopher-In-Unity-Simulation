using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public ArticulationWheelController articulationWheelController;
    public float moveSpeed;
    public float rotateSpeed;
    private Vector2 driveDir = Vector2.zero;

    public void OnDrive(InputAction.CallbackContext context)
    {
        driveDir = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        articulationWheelController.SetRobotVelocity(driveDir.y * moveSpeed, driveDir.x * rotateSpeed);
    }
}
