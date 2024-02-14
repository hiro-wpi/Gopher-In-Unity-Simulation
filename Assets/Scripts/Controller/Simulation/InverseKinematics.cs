using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Abstract class to provide util functions 
///     to compute inverse kinematics
///     
///     BaseTransform is the transform of the base of the robot
///     It should be the same as the base transform of the 
///     forward kinematics in most cases.
///     
///     Note: SolveIK() takes input pose in world frame
/// </summary>
public abstract class InverseKinematics : MonoBehaviour
{
    [field:SerializeField]
    public Transform BaseTransform { get; private set; }
    // Forward kinematic solver
    [SerializeField] protected ForwardKinematics forwardKinematics;

    // void Start() {}

    // void Update() {}

    // May be useful for some IK methods
    public virtual void SetJointAnglesAsHome(float[] jointAngles) {}
    public virtual void SetIterations(int iterations) {}
    public virtual void SetTolerances(
        float positionTolerance = -1, float rotationTolerance = -1
    ) {}

    public abstract float[] SolveIK(
        float[] jointAngles, Vector3 targetPosition, Quaternion targetRotation
    );
}
