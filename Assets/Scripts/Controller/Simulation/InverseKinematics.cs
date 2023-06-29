using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Abstract class to provide util functions 
///     to compute inverse kinematics
/// </summary>
public abstract class InverseKinematics : MonoBehaviour
{
    // One more transform to adjust the velocity IK coordinate
    [field:SerializeField] public Transform BaseTransform { get; set; }
    // Forward kinematic solver
    [SerializeField] protected ForwardKinematics forwardKinematics;

    // void Start() {}

    // void Update() {}

    public abstract (bool, float[]) SolveIK(
        float[] jointAngles, Vector3 targetPosition, Quaternion targetRotation
    );

    public abstract float[] SolveVelocityIK(
        float[] jointAngles, Vector3 positionError, Quaternion rotationError
    );
}