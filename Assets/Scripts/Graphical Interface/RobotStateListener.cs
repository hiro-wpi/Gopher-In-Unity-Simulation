using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     An abstract class that listens to the state of the robot,
///     and interacting with the robot visualization. 
/// </summary>
public abstract class RobotStateListener : MonoBehaviour
{
    // States to be read
    [field: SerializeField, ReadOnly]
    public Vector3 BasePosition { get; protected set;}
    [field: SerializeField, ReadOnly]
    public Vector3 BaseOrientationEuler { get; protected set;}
    [field: SerializeField, ReadOnly]
    public string[] JointNames { get; protected set;} = new string[0];
    [field: SerializeField, ReadOnly]
    public float[] JointPositions { get; protected set;} = new float[0];

    void Start() {}

    void Update() {}

    // Read and store the target values
    public abstract void ReadState();
}
