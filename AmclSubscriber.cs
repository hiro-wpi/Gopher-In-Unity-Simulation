using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Sensor;

/// <summary>
///     This script subscribes and display an image
///     sent from ROS.
/// </summary>
public class AmclSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    public string AmclTopicName = "amcl_pose";

    // Body to move
    public ArticulationBody articulationBody;
    
    // Message info
    private PoseWithCovarianceMsg message;
    private PoseMsg pose;
    private bool isMessageReceived;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();

        // Display
        message = new PoseWithCovarianceMsg();
        // meshRenderer.material = new Material(Shader.Find("Standard"));

        // Subscriber
        ros.Subscribe<PoseWithCovarianceMsg>(AmclTopicName, ReceiveAmcl);
        isMessageReceived = false;
    }

    void Update()
    {
        // Display image if received
        if (isMessageReceived)
        {
            Vector3Msg vec = new Vector3Msg(pose.position.x,pose.position.y,pose.position.z);
            //unity frame .To<FLU>() //teleport body
            articulationBody.TeleportRoot(vec.From<FLU>(),pose.orientation.From<FLU>());
            isMessageReceived = false;
        }
    }

    private void ReceiveAmcl(PoseWithCovarianceMsg amcl)
    {
        pose = amcl.pose;
        isMessageReceived = true; 
    }
}
