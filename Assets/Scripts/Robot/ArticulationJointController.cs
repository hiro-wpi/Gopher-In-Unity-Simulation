using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script allows robot joints to be 
///     controlled with SetJointTarget(), SetJointSpeed().
///     It also provides joint pose intializaiton.
/// </summary>
public class ArticulationJointController : MonoBehaviour
{
    // Robot object
    public GameObject jointRoot;

    public float jointMaxSpeed = 1f; 
    
    // Articulation Bodies
    public float[] homePosition = {0f, 0f, 0f, 0f, 0f, 0f, 0f};
    private ArticulationBody[] articulationChain;

    void Start()
    {
        // Get joints
        articulationChain = jointRoot.GetComponentsInChildren<ArticulationBody>();
        articulationChain = articulationChain.Where(joint => joint.jointType 
                                                    != ArticulationJointType.FixedJoint).ToArray();

        HomeJoints();
    }

    void Update()
    {
    }

    public void HomeJoints()
    {
        StartCoroutine(HomeJointsCoroutine());
    }
    private IEnumerator HomeJointsCoroutine()
    {
        yield return new WaitUntil(() => HomeAndCheck() == true);
    }
    private bool HomeAndCheck()
    {
        int count = 0;
        for (int i = 0; i < homePosition.Length; ++i)
        {
            float targetPosition = homePosition[i] * Mathf.Rad2Deg;
            if (articulationChain[i].xDrive.target != targetPosition)
                SetJointTarget(i, targetPosition);
            else
                count += 1;
        }

        if (count == homePosition.Length)
            return true;
        return false;
    }

    public void SetJointTarget(int jointNum, float target)
    {
        SetJointTarget(articulationChain[jointNum], target, jointMaxSpeed);
    }
    public void SetJointTarget(int jointNum, float target, float speed)
    {
        if (jointNum >= 0 && jointNum < articulationChain.Length)
            SetJointTarget(articulationChain[jointNum], target, speed);
    }
    public void SetJointTarget(ArticulationBody joint, float target)
    {
        SetJointTarget(joint, target, jointMaxSpeed);
    }
    public void SetJointTarget(ArticulationBody joint, float target, float speed)
    {
        if (float.IsNaN(target))
            return;
        
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

    public void SetJointSpeed(int jointNum)
    {
        SetJointSpeed(jointNum, jointMaxSpeed);
    }
    public void SetJointSpeed(int jointNum, float speed)
    {
        if (jointNum >= 0 && jointNum < articulationChain.Length)
            SetJointSpeed(articulationChain[jointNum], speed);
    }
    public void SetJointSpeed(ArticulationBody joint)
    {
        SetJointSpeed(joint, jointMaxSpeed);
    }
    public void SetJointSpeed(ArticulationBody joint, float speed)
    {
        // Get drive
        ArticulationDrive drive = joint.xDrive;
        float currentTarget = drive.target;

        // Speed limit
        float deltaPosition = speed*Mathf.Rad2Deg * Time.fixedDeltaTime;
        float target = currentTarget + deltaPosition;

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

    public void StopJoint(int jointNum)
    {
        if (jointNum >= 0 && jointNum < articulationChain.Length)
            StopJoint(articulationChain[jointNum]);
    }
    public void StopJoint(ArticulationBody joint)
    {
        float currPosition =  joint.jointPosition[0] * Mathf.Rad2Deg;
        ArticulationDrive drive = joint.xDrive;

        if (Mathf.Abs(drive.target - currPosition) > 1)
        {
            drive.target = currPosition;
            joint.xDrive = drive;
        }
    }
}
