using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public ArticulationCameraController cameraController;

    public float mouseSensitivity = 2.5f;
    public float speed = 1.0f;

    private Vector2 controlDelta = Vector2.zero;
    private float yawRotation;
    private float pitchRotation;

    public void OnRotate(InputAction.CallbackContext context)
    {
        controlDelta = context.ReadValue<Vector2>();

        if (controlDelta.x == 0 && controlDelta.y == 0)
            return;
        // Delta joint movement
        (yawRotation, pitchRotation) = cameraController.GetCameraJoints();
        yawRotation -= controlDelta.x * mouseSensitivity * Time.fixedDeltaTime;
        pitchRotation += controlDelta.y * mouseSensitivity * Time.fixedDeltaTime;
        
        // Move camera
        cameraController.SetCameraJoints(yawRotation, pitchRotation, speed);
    }

    public void OnCenter(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            cameraController.HomeCameraJoints();
        }
    }
}
