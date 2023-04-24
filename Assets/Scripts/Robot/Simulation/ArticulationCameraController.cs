using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script controls the active camera joints
/// </summary>
public class ArticulationCameraController : CameraController
{
    // Camera joints
    [SerializeField] private ArticulationBody cameraYawJoint;
    [SerializeField] private ArticulationBody cameraPitchJoint;

    void Start()
    {
        HomeCamera();
    }

    void FixedUpdate()
    {
        // Speed control
        if (controlMode == ControlMode.Speed)
        {
            ArticulationBodyUtils.SetJointSpeedStep(
                cameraYawJoint, angularVelocity.y * Mathf.Rad2Deg
            );
            ArticulationBodyUtils.SetJointSpeedStep(
                cameraPitchJoint, angularVelocity.z * Mathf.Rad2Deg
            );
        }
        // Position control
        else
        {
            ArticulationBodyUtils.SetJointTargetStep(
                cameraYawJoint, angles.y, maxAngularSpeed * Mathf.Rad2Deg
            );
            ArticulationBodyUtils.SetJointTargetStep(
                cameraPitchJoint, angles.z, maxAngularSpeed * Mathf.Rad2Deg
            );
        }
    }
    
    // Home joints
    public override void HomeCamera()
    {
        StartCoroutine(HomeCameraCoroutine());
    }
    
    private IEnumerator HomeCameraCoroutine()
    {
        // Move to given position
        controlMode = ControlMode.Position;
        SetPosition(Vector3.zero);
        // Check if reached
        yield return new WaitUntil(() => CheckPositionReached(Vector3.zero) == true);
        // Switch back to velocity control
        controlMode = ControlMode.Speed;
    }

    private bool CheckPositionReached(Vector3 position)
    {
        // Check if current joint target is set to the position
        if ((Mathf.Abs(cameraYawJoint.xDrive.target - position.y) > 0.00001f) || 
            (Mathf.Abs(cameraPitchJoint.xDrive.target - position.z) > 0.00001f))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
