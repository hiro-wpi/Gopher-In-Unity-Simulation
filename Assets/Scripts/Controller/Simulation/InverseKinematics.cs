using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Abstract class to provide util functions 
///     to compute inverse kinematics
/// </summary>
public abstract class InverseKinematics : MonoBehaviour
{
    [field:SerializeField]
    public Transform BaseTransform { get; private set; }
    // Forward kinematic solver
    [SerializeField] protected ForwardKinematics forwardKinematics;

    // void Start() {}

    // void Update() {}

    public abstract float[] SolveIK(
        float[] jointAngles, Vector3 targetPosition, Quaternion targetRotation
    );
}
