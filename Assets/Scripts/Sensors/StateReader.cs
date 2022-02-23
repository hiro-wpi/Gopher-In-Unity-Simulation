using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.UrdfImporter;

/// <summary>
///     This script reads robot positions, velocities,
///     joint states, etc.
/// </summary>
public class StateReader : MonoBehaviour
{
    // Robot
    public GameObject robot;

    public int updateRate = 10;
    private float deltaTime;

    // Position, Rotation & Velocity
    private Transform tf;
    private Rigidbody rb;
    public Vector3 position;
    private Quaternion rotation;
    public Vector3 eulerRotation;
    public Vector3 linearVelocity;
    public Vector3 angularVelocity;

    // Joint states
    private UrdfJoint[] jointChain;
    private int jointStateLength;
    public string[] names;
    public float[] positions;
    public float[] velocities;
    public float[] forces;

    void Start()
    {
        // Get robot transform
        tf = robot.transform;
        rb = robot.GetComponent<Rigidbody>();
    
        // Get joints
        jointChain = robot.GetComponentsInChildren<UrdfJoint>();
        jointChain = jointChain.Where(joint => 
                        (joint.JointType == UrdfJoint.JointTypes.Revolute || 
                         joint.JointType == UrdfJoint.JointTypes.Continuous)).ToArray();
        
        jointStateLength = jointChain.Length;
        
        names = new string[jointStateLength];
        positions = new float[jointStateLength];
        velocities = new float[jointStateLength];
        forces = new float[jointStateLength];

        for (int i = 0; i < jointStateLength; ++i)
            names[i] = jointChain[i].jointName;

        // Update
        deltaTime = 1f/updateRate;
        InvokeRepeating("ReadState", 1f, deltaTime);
    }

    void Update()
    {
    }

    void ReadState()
    {
        // Pose and Velocity
        if (rb != null)
        {
            // lienar and angular velocity
            linearVelocity = rb.velocity;
            angularVelocity = rb.angularVelocity;
            // transfer to local frame
            linearVelocity = tf.InverseTransformDirection(linearVelocity);
            angularVelocity = tf.InverseTransformDirection(angularVelocity);

            // position and orientation
            position = rb.position;
            rotation = rb.rotation;
            eulerRotation = rotation.eulerAngles;
        }
        else
        {
            // lienar and angular velocity
            linearVelocity = (tf.position - position) / deltaTime;
            angularVelocity = (tf.rotation.eulerAngles - eulerRotation) / deltaTime;
            // transfer to local frame
            linearVelocity = tf.InverseTransformDirection(linearVelocity);
            angularVelocity = tf.InverseTransformDirection(angularVelocity);

            // position and orientation
            position = tf.position;
            rotation = tf.rotation;
            eulerRotation = rotation.eulerAngles;
        }
        
        // Joint states
        for (int i = 0; i < jointStateLength; ++i)
        {   
            positions[i] = jointChain[i].GetPosition();
            velocities[i] = jointChain[i].GetVelocity();
            forces[i] = jointChain[i].GetEffort();
        }
        
    }
}
