using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script sends commands to robot joints
///     and move the end effector to a given pose 
///     using inverse kinematics.
///     
///     Home pose is defined in the absolute world frame.
///     Target pose is defined w.r.t. the home pose frame.
/// </summary>
public class EndEffectorController : ArmController
{
    // Arm component controller
    [SerializeField] private ArticulationJointController jointController;
    [SerializeField] private InverseKinematics inverseKinematics;
    [SerializeField, ReadOnly] private Vector3 homePosition;
    [SerializeField, ReadOnly] private Quaternion homeRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        (homePosition, homeRotation) = inverseKinematics.GetHomePose();
    }

    void Update() {}

    public void MoveTowardsTargetStep()
    {
        // Solve IK
        jointAngles = jointController.GetCurrentJointTargets();
        jointAngles = inverseKinematics.SolveVelocityIK(
            jointAngles, linearError, Quaternion.Euler(angularError)
        );

        // Set joint targets to IK solution
        jointController.SetJointTargetsStep(jointAngles);
    }

    // Target Pose in local frame
    public (Vector3, Quaternion) GetCurrentTargetPose()
    {
        return (targetPosition, targetRotation);
    }

    public void SetTargetPose(Vector3 position, Quaternion rotation)
    {
        targetPosition = position;
        targetRotation = rotation;
    }

    // Target Pose in world frame
    public (Vector3, Quaternion) GetCurrentTargetWorldPose()
    {
        // Transform to the world frame
        return (
            homeRotation * targetPosition + homePosition,
            homeRotation * targetRotation
        );
    }

    public void SetTargetWorldPose(Vector3 position, Quaternion rotation)
    {
        // Transform to the home pose frame
        SetTargetPose(
            Quaternion.Inverse(homeRotation) * (position - homePosition),
            Quaternion.Inverse(homeRotation) * rotation
        );
    }

    // Home Pose
    public void ResetCurrentPoseAsHome()
    {
        inverseKinematics.SetHomePose(
            jointController.GetCurrentJointTargets()
        );
        homePosition, homeRotation = inverseKinematics.GetHomePose();
    }

    public (Vector3, Quaternion) GetHomePose()
    {
        return (homePosition, homeRotation);
    }
}
