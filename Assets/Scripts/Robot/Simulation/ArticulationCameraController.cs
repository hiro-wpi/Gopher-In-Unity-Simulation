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

    // Home positions
    [SerializeField] private Vector3 homeAngles;

    void Start()
    {
        HomeCamera();
    }

    void FixedUpdate()
    {
        // Speed controlhomeAngles
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
                cameraYawJoint, 
                angles.y * Mathf.Rad2Deg, 
                maxAngularSpeed * Mathf.Rad2Deg
            );
            ArticulationBodyUtils.SetJointTargetStep(
                cameraPitchJoint, 
                angles.z * Mathf.Rad2Deg, 
                maxAngularSpeed * Mathf.Rad2Deg
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
        SetPosition(homeAngles);
        // Check if reached
        yield return new WaitUntil(
            () => CheckPositionReached(homeAngles) == true
        );
        // Switch back to velocity control
        controlMode = ControlMode.Speed;
    }

    private bool CheckPositionReached(Vector3 position)
    {
        // Check if current joint target is set to the position
        if ((new Vector3(
                0.0f, cameraYawJoint.xDrive.target, cameraPitchJoint.xDrive.target
             ) * Mathf.Deg2Rad - homeAngles
            ).magnitude > 0.00001f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
