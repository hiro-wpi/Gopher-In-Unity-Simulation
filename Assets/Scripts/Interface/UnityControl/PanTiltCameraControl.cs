using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PanTiltCameraControl : MonoBehaviour
{
    public ArticulationCameraController articulationCameraController;

    public float mouseSensitivity = 2.5f;
    public float speed = 1.0f;

    private Vector2 controlDelta = Vector2.zero;
    private float yawRotation;
    private float pitchRotation;

    public void OnRotate(InputAction.CallbackContext context)
    {
        controlDelta = context.ReadValue<Vector2>();
    }

    public void OnCenter(InputAction.CallbackContext context)
    {
        articulationCameraController.HomeCameraJoints();
    }

    void FixedUpdate()
    {
        if (controlDelta.x == 0 && controlDelta.y == 0)
            return;
        
        // Delta joint movement
        (yawRotation, pitchRotation) = articulationCameraController.GetCameraJoints();
        yawRotation -= controlDelta.x * mouseSensitivity * Time.fixedDeltaTime;
        pitchRotation += controlDelta.y * mouseSensitivity * Time.fixedDeltaTime;
        
        // Move camera
        articulationCameraController.SetCameraJoints(yawRotation, pitchRotation, speed);
    }
}
