using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script allows robot joints to be 
///     controlled and initialized.
/// </summary>
public class ArticulationJointController : MonoBehaviour
{
    // Robot object
    public GameObject jointRoot;

    public float jointSpeed = 1f; 
    
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

    public void SetJointTarget(int jointNum, float target, float speed=0f)
    {
        if (speed == 0f)
            speed = jointSpeed;
        SetJointTarget(articulationChain[jointNum], target, speed);
    }

    public void SetJointTarget(ArticulationBody joint, float target, float speed=0f)
    {
        // If speed not given, set to default speed
        if (speed == 0f)
            speed = jointSpeed;

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

    public void StopJoint(int jointNum)
    {
        StopJoint(articulationChain[jointNum]);
    }

    public void StopJoint(ArticulationBody joint)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.target = joint.jointPosition[0];
        joint.xDrive = drive;
    }
}
