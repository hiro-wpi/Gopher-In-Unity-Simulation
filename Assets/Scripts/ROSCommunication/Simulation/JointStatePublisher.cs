using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.UrdfImporter;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

/// <summary>
///     This script publishes all the non-fixed joint states -
///     name, position, velocity and effort
/// </summary>
public class JointStatePublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string jointStateTopicName = "joint_states";
    [SerializeField] private string frameId = "base_link";

    // Joints
    [SerializeField] private GameObject jointRoot;
    private UrdfJoint[] jointChain;
    private int jointStateLength;
    string[] names;
    float[] positions;
    float[] velocities;
    float[] forces;

    // Message
    private JointStateMsg jointState;
    // rate
    [SerializeField] private int publishRate = 10;
    private Timer timer = new Timer(10);

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<JointStateMsg>(jointStateTopicName);

        // Get joints
        // Use UrdfJoint because ArticulationBody provides
        // link names but not joint names
        jointChain = jointRoot.GetComponentsInChildren<UrdfJoint>();
        jointChain = jointChain.Where(
            joint => (joint.JointType != UrdfJoint.JointTypes.Fixed)
        ).ToArray();
        jointStateLength = jointChain.Length;
        
        positions = new float[jointStateLength];
        velocities = new float[jointStateLength];
        forces = new float[jointStateLength];
        names = new string[jointStateLength];

        // Initialize message
        for (int i = 0; i < jointStateLength; ++i)
        {
            names[i] = jointChain[i].jointName;

            // TEMP Fix
            // names[i] = jointChain[i].jointName;
            string name = jointChain[i].jointName;
            name = name.Substring(name.IndexOf('/') + 1);
            names[i] = name;
        }
        
        jointState = new JointStateMsg
        {
            header = new HeaderMsg(
                0, new TimeStamp(Clock.time), frameId
            ),
            name = names,
            position = new double[jointStateLength],
            velocity = new double[jointStateLength],
            effort = new double[jointStateLength]
        };

        // Rate
        timer = new Timer(publishRate);
    }

    void FixedUpdate() 
    {
        timer.UpdateTimer(Time.fixedDeltaTime);

        if (timer.ShouldProcess)
        {
            PublishJointStates();
            timer.ShouldProcess = false;
        }
    }

    private void PublishJointStates()
    {
        // Update joint state values
        for (int i = 0; i < jointStateLength; ++i)
        {   
            positions[i] = jointChain[i].GetPosition();
            velocities[i] = jointChain[i].GetVelocity();
            forces[i] = jointChain[i].GetEffort();
        }

        // Update ROS message
        jointState.header.Update();
        jointState.position = Array.ConvertAll(positions, x => (double)x);
        jointState.velocity = Array.ConvertAll(velocities, x => (double)x);
        jointState.effort = Array.ConvertAll(forces, x => (double)x);

        ros.Publish(jointStateTopicName, jointState);
    }
}
