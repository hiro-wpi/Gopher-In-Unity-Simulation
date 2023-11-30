using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script sends commands to robot joints
///     and move the end effector to a given pose 
///     using inverse kinematics.
///     
///     SetCurrentPoseAsTarget() should be called first to initialize
///     the target pose before calling SetTargetDeltaPose()
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
    [SerializeField] private float positionTolerance = 0.01f;
    [SerializeField] private float rotationTolerance = 0.05f;

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
    [SerializeField, ReadOnly] private bool succeed;
    [SerializeField, ReadOnly] private float[] targetJointAngles;

    // Transform to apply relative pose delta
    public Transform RelativeTransform;

    // Visualize target pose
    [SerializeField] private bool visualizeTargetPose = false;

    void Start() 
    {
        baseTransform = forwardKinematics.BaseTransform;
    }

    void FixedUpdate()
    {
        (targetPosition, targetRotation) = LocalToWorldPose(
            targetLocalPosition, targetLocalRotation
        );
    }

    // Move the end effector to the target pose
    public void MoveToTargetStep()
    {
        // Set joint targets to IK solution
        if (succeed)
        {
            jointController.SetJointTargetsStep(targetJointAngles);
        }
    }

    public void SetJointAsTarget(float[] jointAngles)
    {
        // Compute target pose
        float[] currJointAngles = jointAngles.Clone() as float[];
        forwardKinematics.SolveFK(currJointAngles);
        (targetPosition, targetRotation) = forwardKinematics.GetPose(
            forwardKinematics.NumJoint
        );

        // Set target
        succeed = true;
        targetJointAngles = currJointAngles;
        // Update target local pose
        (targetLocalPosition, targetLocalRotation) = WorldToLocalPose(
            targetPosition, targetRotation
        );
    }

    public void SetTargetDeltaPose(Vector3 linearDelta, Vector3 angularDelta) 
    {
        // No need to run IK if delta are 0
        if (linearDelta == Vector3.zero && angularDelta == Vector3.zero)
        {
            return;
        }

        // Update target pose w.r.t. relative transform
        Vector3 position = (
            targetPosition 
            + RelativeTransform.TransformDirection(linearDelta)
        );
        Quaternion rotation = Quaternion.Euler(
            RelativeTransform.TransformDirection(angularDelta) * Mathf.Rad2Deg
        ) * targetRotation;

        // Solve IK
        SetTargetPose(position, rotation);
    }

    public void SetTargetPose(Vector3 position, Quaternion rotation)
    {
        // Store previous pose
        Vector3 prevPosition = targetPosition;
        Quaternion prevRotation = targetRotation;
        targetPosition = position;
        targetRotation = rotation;

        // Solve IK
        SolveIK();

        // Restore if failed
        if (!succeed)
        {
            targetPosition = prevPosition;
            targetRotation = prevRotation;
        }
        // Update target local pose if succeed
        else
        {
            (targetLocalPosition, targetLocalRotation) = WorldToLocalPose(
                targetPosition, targetRotation
            );
        }
    }

    private void SolveIK()
    {
        // Solve IK
        float[] currJointAngles = jointController.GetCurrentJointTargets();
        targetJointAngles = inverseKinematics.SolveIK(
            currJointAngles, targetPosition, targetRotation
        );

        // Get solution pose
        forwardKinematics.SolveFK(targetJointAngles);
        var (position, rotation) = forwardKinematics.GetPose(
            forwardKinematics.NumJoint
        );

        // Check convergence
        if ((Vector3.Distance(position, targetPosition) < positionTolerance)
            && (Quaternion.Angle(rotation, targetRotation) * Mathf.Deg2Rad 
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
    public (Vector3, Quaternion) GetTargetPose()
    {
        return (targetPosition, targetRotation);
    }

    // Helper
    private (Vector3, Quaternion) LocalToWorldPose(
        Vector3 position, Quaternion rotation
    )
    {
        return (
            baseTransform.TransformPoint(position),
            baseTransform.rotation * rotation
        );
    }

    private (Vector3, Quaternion) WorldToLocalPose(
        Vector3 position, Quaternion rotation
    )
    {
        return (
            baseTransform.InverseTransformPoint(position),
            Quaternion.Inverse(baseTransform.rotation) * rotation
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
