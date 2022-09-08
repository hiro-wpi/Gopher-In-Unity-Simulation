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
    public int updateRate = 10;
    private float deltaTime;

    // Robot
    public GameObject robot;
    public float durationTime;
    private float startTime;

    // Position, Rotation & Velocity
    private Transform tf;
    private Rigidbody rb;
    public Vector3 position;
    private Quaternion rotation;
    public Vector3 rotationEuler;
    public Vector3 linearVelocity;
    public Vector3 angularVelocity;

    // Joint states
    private UrdfJoint[] jointChain;
    private int jointStateLength;
    public string[] jointNames;
    public float[] jointPositions;
    public float[] jointVelocities;
    public float[] jointForces;

    // Extra
    public GameObject[] extraObjects;
    public Vector3[] objectPositions;
    public Vector3[] objectRotations;

    void Start()
    {
        startTime = Time.time;

        // Get robot transform
        tf = robot.transform;
        rb = robot.GetComponentInChildren<Rigidbody>();
    
        // Get joints
        jointChain = robot.GetComponentsInChildren<UrdfJoint>();
        jointChain = jointChain.Where(joint => 
                        (joint.JointType != UrdfJoint.JointTypes.Fixed)).ToArray();
        jointStateLength = jointChain.Length;

        jointNames = new string[jointStateLength];
        jointPositions = new float[jointStateLength];
        jointVelocities = new float[jointStateLength];
        jointForces = new float[jointStateLength];
        for (int i = 0; i < jointStateLength; ++i)
            jointNames[i] = jointChain[i].jointName;

        // Get extra object's position and rotation
        if (extraObjects == null)
            extraObjects = new GameObject[0];
        objectPositions = new Vector3[extraObjects.Length];
        objectRotations = new Vector3[extraObjects.Length];

        // Update
        deltaTime = 1f / updateRate;
        InvokeRepeating("ReadState", 1f, deltaTime);
    }

    void Update()
    {
    }

    void ReadState()
    {
        // Duration
        durationTime = Time.time - startTime;
        
        // Pose and Velocity
        if (rb != null)
        {
            // lienar and angular velocity from rigidbody
            linearVelocity = rb.velocity;
            angularVelocity = rb.angularVelocity;
        }
        else
        {
            // lienar and angular velocity from transform
            linearVelocity = (tf.position - position) / deltaTime;
            angularVelocity = (tf.rotation.eulerAngles - rotationEuler) / deltaTime;
        }
        // transfer to local frame
        linearVelocity = tf.InverseTransformDirection(linearVelocity);
        angularVelocity = tf.InverseTransformDirection(angularVelocity);

        // position and orientation
        position = tf.position;
        rotation = tf.rotation;
        rotationEuler = rotation.eulerAngles;
        
        // Joint states
        for (int i = 0; i < jointStateLength; ++i)
        {   
            jointPositions[i] = jointChain[i].GetPosition();
            jointVelocities[i] = jointChain[i].GetVelocity();
            jointForces[i] = jointChain[i].GetEffort();
        }
        
        // Extra objects
        for (int i = 0; i < extraObjects.Length; ++i)
        {   
            objectPositions[i] = extraObjects[i].transform.position;
            objectRotations[i] = extraObjects[i].transform.rotation.eulerAngles;
        } 
    }
}
