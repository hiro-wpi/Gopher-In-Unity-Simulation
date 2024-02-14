using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script sends commands to robot joints
///     and move the end effector to a given pose 
///     using inverse kinematics.
///     
///     SetHome() should be called first to initialize the target
///     pose before calling SetTargetPose() and SetTargetDeltaPose()
///     
///     SetTargetPose() takes world pose as input, while 
///     SetTargetDeltaPose() takes local delta pose w.r.t.
///     the RelativeTransform as input.
///     
///     SetTargetPose() and SetTargetDeltaPose() will only update
///     the target joint angles. To actually move the end effector,
///     call MoveToTargetStep() in FixedUpdate().
/// </summary>
public class ArticulationEndEffectorController : MonoBehaviour
{
    // Arm component controller
    [SerializeField] private ArticulationJointController jointController;
    [SerializeField] private ForwardKinematics forwardKinematics;
    [SerializeField] private InverseKinematics inverseKinematics;

    // Tolerance (Could be different from the IK solver tolerance)
    // Used to evaluate if the target pose is acceptable
    [SerializeField] private float positionTolerance = 0.1f;
    [SerializeField] private float rotationTolerance = 0.2f;

    // Target pose is in world frame
    // It gets updated based on local pose
    [SerializeField, ReadOnly] private Vector3 targetPosition;
    [SerializeField, ReadOnly] private Quaternion targetRotation;
    // The local pose is stored w.r.t. 
    // the base transform of the forward kinematics
    private Transform baseTransform;
    private Vector3 targetLocalPosition;
    private Quaternion targetLocalRotation;

    // IK solution
    // If IK fails, the end effector will stop moving
    // unless moveWhenIKFailed is set to true
    // This could be useful when control interface is motion mapping kind
    [SerializeField] private bool moveWhenIKFailed = false;
    [SerializeField, ReadOnly] private bool succeed;
    [SerializeField, ReadOnly] private float[] targetJointAngles;

    // Transform to apply relative pose delta
    public Transform RelativeTransform;

    // Visualize target pose
    [SerializeField] private bool visualizeTargetPose = false;

    void Awake() 
    {
        baseTransform = forwardKinematics.BaseTransform;
    }

    void Start() {}

    void FixedUpdate()
    {
        (targetPosition, targetRotation) = Utils.LocalToWorldPose(
            baseTransform, targetLocalPosition, targetLocalRotation
        );
    }

    // Move the end effector to the target pose
    public void MoveToTargetStep()
    {
        // Set joint targets to IK solution
        if (succeed || moveWhenIKFailed)
        {
            jointController.SetJointTargetsStep(targetJointAngles);
        }
    }

    public void SetHome(float[] jointAngles)
    {
        // Compute target pose
        float[] currJointAngles = jointAngles.Clone() as float[];
        (targetPosition, targetRotation) = GetJointTargetPose(currJointAngles);

        // Set target
        succeed = true;
        targetJointAngles = currJointAngles;
        // Update target local pose
        (targetLocalPosition, targetLocalRotation) = Utils.WorldToLocalPose(
            baseTransform, targetPosition, targetRotation
        );

        // Set home for IK
        inverseKinematics.SetJointAnglesAsHome(targetJointAngles);
    }

    public void SetTargetDeltaPose(Vector3 linearDelta, Vector3 angularDelta) 
    {
        if (linearDelta == Vector3.zero && angularDelta == Vector3.zero)
        {
            return;
        }

        // Update current target pose in world frame
        (targetPosition, targetRotation) = Utils.LocalToWorldPose(
            baseTransform, targetLocalPosition, targetLocalRotation
        );

        // Compute next target pose 
        // given delta pose in relative transform
        // linear
        Vector3 position = (
            targetPosition 
            + RelativeTransform.TransformDirection(linearDelta)
        );
        // angular
        Quaternion rotation = Quaternion.Euler(
            RelativeTransform.TransformDirection(angularDelta) * Mathf.Rad2Deg
        ) * targetRotation;

        // Solve IK
        SetTargetPose(position, rotation);
    }

    public void SetTargetPose(Vector3 position, Quaternion rotation)
    {
        // Solve IK
        SolveIK(position, rotation);

        // Updaet target local pose if IK succeed
        if (succeed || moveWhenIKFailed)
        {
            (targetLocalPosition, targetLocalRotation) = Utils.
                WorldToLocalPose(
                    baseTransform, position, rotation
                );
        }
    }

    private void SolveIK(Vector3 position, Quaternion rotation)
    {
        // Solve IK
        float[] currJointAngles = jointController.GetCurrentJointTargets();
        targetJointAngles = inverseKinematics.SolveIK(
            currJointAngles, position, rotation
        );
        if (targetJointAngles == null)
        {
            succeed = false;
            return;
        }

        // Get solution pose
        var (solvedPosition, solvedRotation) = GetJointTargetPose(
            targetJointAngles
        );

        // Check convergence
        if ((Vector3.Distance(solvedPosition, position) < positionTolerance)
            && (Quaternion.Angle(solvedRotation, rotation) * Mathf.Deg2Rad 
                < rotationTolerance)
        ) {
            succeed = true;
        }
        else
        {
            succeed = false;
        }
    }

    // Getter
    // Get the end effector target pose
    public (Vector3, Quaternion) GetEETargetPose()
    {
        return (targetPosition, targetRotation);
    }

    // Get the actual end effector pose (from joint targets)
    public (Vector3, Quaternion) GetJointTargetPose()
    {
        return GetJointTargetPose(jointController.GetCurrentJointTargets());
    }

    // Compute the expected end effector pose given joint angle
    public (Vector3, Quaternion) GetJointTargetPose(float[] angles)
    {
        forwardKinematics.SolveFK(angles);
        return forwardKinematics.GetPose(
            forwardKinematics.NumJoint
        );
    }

    // Debug
    void OnDrawGizmos() 
    {
        if (visualizeTargetPose)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPosition, 0.02f);
            // draw axis to indicate direction
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                targetPosition, 
                targetPosition + targetRotation * Vector3.forward * 0.1f
            );
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                targetPosition, 
                targetPosition + targetRotation * Vector3.up * 0.1f
            );
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                targetPosition,
                 targetPosition + targetRotation * Vector3.right * 0.1f
            );
        }
    }
}
