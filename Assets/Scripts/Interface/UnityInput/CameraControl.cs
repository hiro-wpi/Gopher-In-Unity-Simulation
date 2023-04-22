using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

/// <summary>
///     This script handles Unity input for Base control.
/// </summary>
public class CameraControl : MonoBehaviour
{
    // Local controller
    [SerializeField] private CameraController cameraController;

    // Container for the angular speed
    private Vector3 angularVelocity;
    private Vector2 velocityInput;

    void Start() {}

    public void OnRotate(InputAction.CallbackContext context)
    {
        velocityInput = context.ReadValue<Vector2>();
        angularVelocity = new Vector3(0.0f, -velocityInput.x, velocityInput.y);

        // Set velocity
        cameraController.SetVelocity(angularVelocity);
    }

    public void OnHome(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            cameraController.HomeCamera();
        }
    }
}
