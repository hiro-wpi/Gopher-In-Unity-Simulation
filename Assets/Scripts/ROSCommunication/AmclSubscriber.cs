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
    public string AmclTopicName = "/amcl_pose";

    // Body to move
    public ArticulationBody articulationBody;
    
    // Message info
    private PoseWithCovarianceStampedMsg message;
    private PoseMsg pose;
    private bool isMessageReceived;

    private GameObject test_sphere;
    private Vector3 unity_position;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();

        // Display
        message = new PoseWithCovarianceStampedMsg();
        // meshRenderer.material = new Material(Shader.Find("Standard"));

        unity_position = new Vector3(0.0f, 0.0f, 0.0f);
        test_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Subscriber
        ros.Subscribe<PoseWithCovarianceStampedMsg>(AmclTopicName, ReceiveAmcl);
        isMessageReceived = false;
        
    }

    void Update()
    {
        // Display image if received
        if (isMessageReceived)
        {
            Vector3Msg vec = new Vector3Msg(pose.position.x,pose.position.y,pose.position.z);
            // unity frame .To<FLU>() 
            //teleport body
            unity_position = vec.From<FLU>();
            unity_position.x = 1.08f*unity_position.x + 14.11f;    //13.37f;
            unity_position.z = 1.56f*unity_position.z + 13.48f; //8.75f*0.5f;
            unity_position.y = 0.0f;

            // articulationBody.TeleportRoot(vec.From<FLU>(),pose.orientation.From<FLU>());
            articulationBody.TeleportRoot(unity_position,pose.orientation.From<FLU>());

            isMessageReceived = false;

        }
    }

    private void ReceiveAmcl(PoseWithCovarianceStampedMsg amcl)
    {
        pose = amcl.pose.pose;
        isMessageReceived = true; 
    }
}
