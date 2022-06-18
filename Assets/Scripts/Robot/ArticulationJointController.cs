using System.Linq;
using UnityEngine;

/// <summary>
///     This script allows robot joints to be controlled with 
///     SetJointTarget(), SetJointTargetStep(), SetJointSpeedStep().
///     It also provides joint pose intializaiton.
/// </summary>
public class ArticulationJointController : MonoBehaviour
{
    [SerializeField]
    private float jointMaxSpeed = 1f; 
    
    // Articulation Bodies
    [SerializeField]
    private float[] homePosition = {0f, 0f, 0f, 0f, 0f, 0f, 0f};
    
    [SerializeField]
    private ArticulationBody[] articulationChain;

    public int GetNumJoints()
    {
        // Get joints
        return articulationChain.Length;
    }

    public bool HomeAndCheck()
    {
        int count = 0;
        for (int i = 0; i < homePosition.Length; ++i)
        {
            // prevent conversion error deg->rad
            float current = articulationChain[i].xDrive.target * Mathf.Deg2Rad;
            if (Mathf.Abs(current - homePosition[i]) > 0.00001)
                SetJointTargetStep(i, homePosition[i]);
            else
                count += 1;
        }

        if (count == homePosition.Length)
            return true;
        return false;
    }

    public void SetJointTarget(int jointNum, float target)
    {
        SetJointTarget(articulationChain[jointNum], target);
    }
    
    public void SetJointTarget(ArticulationBody joint, float target)
    {
        if (float.IsNaN(target))
            return;
        target = target * Mathf.Rad2Deg;
        
        // Get drive
        ArticulationDrive drive = joint.xDrive;

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

    public void SetJointTargetStep(int jointNum, float target)
    {
        SetJointTargetStep(articulationChain[jointNum], target, jointMaxSpeed);
    }
    public void SetJointTargetStep(int jointNum, float target, float speed)
    {
        if (jointNum >= 0 && jointNum < articulationChain.Length)
            SetJointTargetStep(articulationChain[jointNum], target, speed);
    }
    public void SetJointTargetStep(ArticulationBody joint, float target)
    {
        SetJointTargetStep(joint, target, jointMaxSpeed);
    }
    public void SetJointTargetStep(ArticulationBody joint, float target, float speed)
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

    public void SetJointSpeedStep(int jointNum)
    {
        SetJointSpeedStep(jointNum, jointMaxSpeed);
    }
    public void SetJointSpeedStep(int jointNum, float speed)
    {
        if (jointNum >= 0 && jointNum < articulationChain.Length)
            SetJointSpeedStep(articulationChain[jointNum], speed);
    }
    public void SetJointSpeedStep(ArticulationBody joint)
    {
        SetJointSpeedStep(joint, jointMaxSpeed);
    }
    public void SetJointSpeedStep(ArticulationBody joint, float speed)
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

    public float[] GetCurrentJointTargets()
    {
        float[] targets = new float[articulationChain.Length];
        for (int i=0; i < articulationChain.Length; ++i)
        {
            targets[i] = articulationChain[i].xDrive.target;
        }

        targets = targets.Select(r => r * Mathf.Deg2Rad).ToArray();
        return targets;
    }

    public void SetJointTargets(float[] targets)
    {
        for (int i=0; i < articulationChain.Length; ++i)
        {
            SetJointTarget(articulationChain[i], targets[i]);
        }
    }
}
