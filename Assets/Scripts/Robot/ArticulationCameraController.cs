using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script controls the active camera joints
/// </summary>
public class ArticulationCameraController : MonoBehaviour
{
    public ArticulationBody cameraYawJoint;
    public ArticulationBody cameraPitchJoint;

    public float jointSpeed = 1.0f;
    public float angleLimit = 60f * Mathf.Deg2Rad;

    public float yawOffset = 0f;
    public float pitchOffset = 0f;

    // Use this for initialization
    void Start()
    {
        HomeCameraJoints();
    }

    void Update()
    {
    }

    public (float, float) GetCameraJoints()
    {
        return (cameraYawJoint.xDrive.target * Mathf.Deg2Rad, 
                cameraPitchJoint.xDrive.target * Mathf.Deg2Rad);
    }
    public void SetCameraJoints(float yawRotation, float pitchRotation, float speed = 0)
    {
        // Default joint speed
        if (speed <= 0)
            speed = jointSpeed;
        // Clamp
        yawRotation = Mathf.Clamp(yawRotation, 
                                  yawOffset - angleLimit, yawOffset + angleLimit);
        pitchRotation = Mathf.Clamp(pitchRotation, 
                                    pitchOffset - angleLimit, pitchOffset + angleLimit);
        // Move joint
        SetJointTargetStep(cameraYawJoint, yawRotation, speed);
        SetJointTargetStep(cameraPitchJoint, pitchRotation, speed);
    }
    
    // Home joints
    public void HomeCameraJoints()
    {
        StartCoroutine(HomeCameraJointsCoroutine());
    }
    private IEnumerator HomeCameraJointsCoroutine()
    {
        yield return new WaitUntil(() => HomeCameraAndCheck() == true);
    }
    private bool HomeCameraAndCheck()
    {
        bool yawHomed = Mathf.Abs(cameraYawJoint.xDrive.target - yawOffset) < 0.001;
        bool pitchHomed = Mathf.Abs(cameraPitchJoint.xDrive.target - pitchOffset) < 0.001;
        
        if (!yawHomed)
            SetJointTargetStep(cameraYawJoint, yawOffset, jointSpeed);
        if (!pitchHomed)
            SetJointTargetStep(cameraPitchJoint, pitchOffset, jointSpeed);
        if (yawHomed && pitchHomed)
            return true;
        return false;
    }


    // Utils
    private void SetJointTargetStep(ArticulationBody joint, float target, float speed)
    {
        if (float.IsNaN(target))
            return;
        target *= Mathf.Rad2Deg;
        
        // Get drive
        ArticulationDrive drive = joint.xDrive;
        float currentTarget = drive.target;

        // Speed limit
        float deltaPosition = speed*Mathf.Rad2Deg * Time.fixedDeltaTime;
        if (Mathf.Abs(currentTarget - target) > deltaPosition)
            target = currentTarget + deltaPosition * Mathf.Sign(target-currentTarget);

        // Joint limit
        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            if (target > drive.upperLimit)
                target = drive.upperLimit;
            else if (target < drive.lowerLimit)
                target = drive.lowerLimit;
        }

        // Set target
        drive.target = target;
        joint.xDrive = drive;
    }
}
