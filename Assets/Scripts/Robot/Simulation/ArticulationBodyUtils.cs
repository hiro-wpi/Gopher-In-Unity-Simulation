using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///    This Utils class contains some useful functions for ArticulationBody
///    All angles are in degree
/// </summary>
public static class ArticulationBodyUtils
{
    // Set joint target (in degree)
    public static void SetJointTarget(ArticulationBody joint, float target)
    {
        // Get drive
        ArticulationDrive drive = joint.xDrive;

        // Joint limit
        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            target = Mathf.Clamp(target, drive.lowerLimit, drive.upperLimit);
        }

        // Set target
        drive.target = target;
        joint.xDrive = drive;
    }

    public static void SetJointTargetStep(ArticulationBody joint, float target, float speed)
    {
        // Get drive
        ArticulationDrive drive = joint.xDrive;

        // Joint limit
        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            target = Mathf.Clamp(target, drive.lowerLimit, drive.upperLimit);
        }

        // Check if the target is reachable
        float diff = target - drive.target;
        // if not, set the target to the next timestep position given a speed
        if (Mathf.Abs(diff) > speed * Time.fixedDeltaTime)
        {
            SetJointSpeedStep(joint, speed * Mathf.Sign(diff));
        }
        // if yes, set the target in next timestep directly to target
        else
        {
            SetJointTarget(joint, target);
        }
    }

    // Set joint speed (in degree/s)
    // As directly setting articulatio body velocity is still unstable,
    // the better practice is still to set its target position at each time step,
    // to simulate velocity control by position control.
    public static void SetJointSpeedStep(ArticulationBody joint, float speed)
    {
        // Get drive
        ArticulationDrive drive = joint.xDrive;

        // Set target to the next timestep position given a speed
        float target = drive.target + speed * Time.fixedDeltaTime;

        // Joint limit
        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            target = Mathf.Clamp(target, drive.lowerLimit, drive.upperLimit);
        }

        drive.target = target;
        joint.xDrive = drive;
    }

    // Stop joint movement
    public static void StopJoint(ArticulationBody joint)
    {
        // Get current position
        float currPosition;

        // Revolute joint
        if (joint.jointType == ArticulationJointType.RevoluteJoint)
        {
            currPosition = joint.jointPosition[0] * Mathf.Rad2Deg;
        }

        // Prismatic joint has static error problem
        else
        {
            // If joint is still moving, use the current position
            if (joint.jointVelocity[0] > 0.01f)
            {
                currPosition = joint.jointPosition[0];
            }
            // If not, use the target position
            else
            {
                currPosition = joint.xDrive.target;
            }
        }

        ArticulationBodyUtils.SetJointTarget(joint, currPosition);
    }
}
