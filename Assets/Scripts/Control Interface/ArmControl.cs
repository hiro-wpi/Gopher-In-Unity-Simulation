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

    // Simulating input lagging (for simulation only)
    [SerializeField] private float simulationInputLagMean = 175f;  // ms
    [SerializeField] private float simulationInputLagStd = 25f;  // ms

    // Container for the speed vector
    private Vector3 linearVelocity = Vector3.zero;
    private Vector3 angularVelocity = Vector3.zero;

    // Switches Arm Movement Between Rotation and Translation
    // This is used only when the control input
    // for rotation and translation are the same (OnTranslateRotate())
    public enum ArmControlMode { translation = 0, rotation = 1 }
    [field: SerializeField] public ArmControlMode controlMode = ArmControlMode.translation;

    void Start() 
    {
        // No need to simulate input lagging if controlling the real robot
        if (armController is PhysicalArmController)
        {
            simulationInputLagMean = 0;
            simulationInputLagStd = 0;
        }
    }

    void Update() {}

    // Change arm controller mode
    public void OnModeChange(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            armController.SwitchMode();
        }
    }

    // End effector control
    public void OnTranslate(InputAction.CallbackContext context)
    {
        // Read input
        linearVelocity = context.ReadValue<Vector3>();
        // Set velocity
        StartCoroutine(DelayAndSetVelocityCoroutine("linear", linearVelocity));
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        // Rotation uses the same keys as translation
        // Need to convert axis
        angularVelocity = context.ReadValue<Vector3>();
        angularVelocity = new Vector3(
            -angularVelocity.z,
            angularVelocity.x,
            -angularVelocity.y
        );
        // Set velocity
        StartCoroutine(
            DelayAndSetVelocityCoroutine("angular", angularVelocity)
        );
    }

    // Switch between Translation and Rotation
    public void OnSwitch(InputAction.CallbackContext context)
    {

        if(context.performed)
        {
            if(controlMode == ArmControlMode.translation)
            {
                controlMode = ArmControlMode.rotation;
            }
            else if(controlMode == ArmControlMode.rotation)
            {
                controlMode = ArmControlMode.translation;
            }
        }
    }

    // Handles Translation and Rotation of the arm
    // Only important if the keys for rotation and translation are the same
    public void OnTranslateRotate(InputAction.CallbackContext context)
    {
        if(controlMode == ArmControlMode.translation)
        {
            OnTranslate(context);
        }

        if(controlMode == ArmControlMode.rotation)
        {
            OnRotate(context);
        }
    }

    // Delay input to simulate input lagging
    IEnumerator DelayAndSetVelocityCoroutine(string type, Vector3 inputVelocity)
    {
        // Simulate input lagging
        if (simulationInputLagMean > 0)
        {
            float delay = Utils.GenerateGaussianRandom(
                simulationInputLagMean, simulationInputLagStd
            ) / 1000f;
            yield return new WaitForSeconds(delay);
        }

        // Set velocity
        if (type == "linear")
        {
            armController.SetLinearVelocity(inputVelocity);
        }
        else if (type == "angular")
        {
            armController.SetAngularVelocity(inputVelocity);
        }
    }

    // Gripper
    public void OnGrasp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Set velocity
            armController.ChangeGripperStatus();
        }
    }

    // Handle autonomy input
    public void OnTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            armController.MoveToTarget();
        }
    }

    // Stop
    public void StopArm()
    {
        linearVelocity = Vector3.zero;
        angularVelocity = Vector3.zero;
        armController.SetLinearVelocity(linearVelocity);
        armController.SetAngularVelocity(angularVelocity);
    }

    // Preset
    public void OnHome(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            armController.HomeJoints();
        }
    }

    public void OnPreset1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            armController.MoveToPreset(0);
        }
    }

    public void OnPreset2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            armController.MoveToPreset(1);
        }
    }

    public void OnPreset3(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            armController.MoveToPreset(2);
        }
    }

    public void OnPreset4(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            armController.MoveToPreset(3);
        }
    }

    // Emergency Stop 
    public void EmergencyStop(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            armController.EmergencyStop();
        }
    }

    public void EmergencyStopResume(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            armController.EmergencyStopResume();
        }
    }
}
