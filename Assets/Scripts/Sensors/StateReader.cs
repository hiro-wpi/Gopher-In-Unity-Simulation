using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.UrdfImporter;

/// <summary>
///     This script reads robot jointPositions, jointVelocities,
///     joint states, etc.
/// </summary>
public class StateReader : MonoBehaviour
{
    [SerializeField] private int updateRate = 10;
    private float deltaTime;
    private Timer timer = new Timer(10);

    // Robot
    [SerializeField] private GameObject robot;

    // Reading
    // Running time
    private float startTime;
    [field:SerializeField, ReadOnly] 
    public float DurationTime { get; private set; }
    
    // Position, Rotation & Velocity
    private Transform tf;
    private Rigidbody rb;
    [field:SerializeField, ReadOnly] 
    public Vector3 Position { get; private set; }
    [field:SerializeField, ReadOnly] 
    public Vector3 RotationEuler { get; private set; }
    [field:SerializeField, ReadOnly] 
    public Vector3 LinearVelocity { get; private set; }
    [field:SerializeField, ReadOnly] 
    public Vector3 AngularVelocity { get; private set; }

    // Joint states
    private UrdfJoint[] jointChain;
    [field:SerializeField, ReadOnly] 
    public string[] JointNames { get; private set; }
    [field:SerializeField, ReadOnly] 
    public float[] JointPositions { get; private set; }
    [field:SerializeField, ReadOnly] 
    public float[] JointVelocities { get; private set; }
    [field:SerializeField, ReadOnly] 
    public float[] JointForces { get; private set; }

    void Start()
    {
        // Duration
        startTime = Time.time;

        // Get robot transform
        tf = robot.transform;
        rb = robot.GetComponentInChildren<Rigidbody>();
    
        // Get joints
        jointChain = robot.GetComponentsInChildren<UrdfJoint>();
        jointChain = jointChain.Where(
            joint => joint.JointType != UrdfJoint.JointTypes.Fixed
        ).ToArray();
        int jointStateLength = jointChain.Length;

        JointNames = new string[jointStateLength];
        JointPositions = new float[jointStateLength];
        JointVelocities = new float[jointStateLength];
        JointForces = new float[jointStateLength];
        for (int i = 0; i < jointStateLength; ++i)
        {
            JointNames[i] = jointChain[i].jointName;
        }

        // Rate
        timer = new Timer(updateRate);
    }

    void FixedUpdate() 
    {
        timer.UpdateTimer(Time.fixedDeltaTime);
        if (timer.ShouldProcess)
        {
            ReadState();
            timer.ShouldProcess = false;
        }
    }

    private void ReadState()
    {
        // Duration
        DurationTime = Time.time - startTime;
        
        // Pose and Velocity
        if (rb != null)
        {
            // lienar and angular velocity from rigidbody
            LinearVelocity = rb.velocity;
            AngularVelocity = rb.angularVelocity;
        }
        else
        {
            // lienar and angular velocity from transform
            LinearVelocity = (tf.position - Position) / deltaTime;
            AngularVelocity = (tf.rotation.eulerAngles - RotationEuler) / deltaTime;
        }
        // transfer to local frame
        LinearVelocity = tf.InverseTransformDirection(LinearVelocity);
        AngularVelocity = tf.InverseTransformDirection(AngularVelocity);

        // Position and orientation
        Position = tf.position;
        RotationEuler = tf.rotation.eulerAngles;
        
        // Joint states
        for (int i = 0; i < jointChain.Length; ++i)
        {   
            JointPositions[i] = jointChain[i].GetPosition();
            JointVelocities[i] = jointChain[i].GetVelocity();
            JointForces[i] = jointChain[i].GetEffort();
        }
    }
}
