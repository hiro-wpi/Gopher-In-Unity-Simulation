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

    // Simulating input lagging (for simulation only)
    [SerializeField] private float simulationInputLagMean = 100f;  // ms
    [SerializeField] private float simulationInputLagStd = 25f;  // ms

    // Container for the angular speed
    private Vector3 angularVelocity;
    private Vector2 inputVelocity;

    void Start() 
    {
        // No need to simulate input lagging if controlling the real robot
        if (cameraController is PhysicalCameraController)
        {
            simulationInputLagMean = 0;
            simulationInputLagStd = 0;
        }
    }

    void Update() {}

    public void OnRotate(InputAction.CallbackContext context)
    {
        inputVelocity = context.ReadValue<Vector2>();
        angularVelocity = new Vector3(0.0f, -inputVelocity.x, inputVelocity.y);

        // Set velocity
        StartCoroutine(DelayAndSetVelocityCoroutine(angularVelocity));
    }

    IEnumerator DelayAndSetVelocityCoroutine(Vector2 angularVelocity)
    {
        // Simulate input lagging
        if (simulationInputLagMean > 0)
        {
            float delay = Utils.GenerateGaussianRandom(
                simulationInputLagMean, simulationInputLagStd
            ) / 1000f;
            yield return new WaitForSeconds(delay);
        }
        // Velocity
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
