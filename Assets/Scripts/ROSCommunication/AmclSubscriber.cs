using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
            unity_position.x = 1.07f*unity_position.x + 13.79f ;    //through linear regression
            // unity_position.x = (float)(0.0100*(Math.Pow(unity_position.x,2)) + 1.2367*unity_position.x + 12.0938);
            // unity_position.x = (float)(-0.0002*(Math.Pow(unity_position.x,4)) - 0.0113*(Math.Pow(unity_position.x,3)) - 0.1475*(Math.Pow(unity_position.x,2)) + 0.5778*unity_position.x + 13.8851);
            unity_position.z = (float)(-0.0166*(Math.Pow(unity_position.z, 3)) - 0.4529*(Math.Pow(unity_position.z, 2)) - 2.2691*unity_position.z + 1.0724);    
            // unity_position.z = (float)(-0.00015*(Math.Pow(unity_position.z, 2)) + 1.1267*unity_position.z + 7.3452);  
            // unity_position.z = (float)(+ 1.1257*unity_position.z + 7.452); 
            // unity_position.z = (float)(0.0004*(Math.Pow(unity_position.z,5)) + 0.015*(Math.Pow(unity_position.z,4)) + 0.2135*(Math.Pow(unity_position.z,3)) + 1.1681*(Math.Pow(unity_position.z,2)) + 3.0117*unity_position.z + 9.6754);
            // unity_position.z = (float)(0.0002*(Math.Pow(unity_position.z,8) + 0.0130*(Math.Pow(unity_position.z,7) +0.0004*(Math.Pow(unity_position.z,6) + 0.0004*(Math.Pow(unity_position.z,5) + 0.015*(Math.Pow(unity_position.z,4)) + 0.2135*(Math.Pow(unity_position.z,3)) + 1.1681*(Math.Pow(unity_position.z,2)) + 3.0117*unity_position.z + 9.6754)); 
            // unity_position.z = (float)(0.0002*(Math.Pow(unity_position.z,6)) + 0.0130*(Math.Pow(unity_position.z,5)) + 0.2809*(Math.Pow(unity_position.z,4)) + 2.9642*(Math.Pow(unity_position.z,3)) + 15.5783*(Math.Pow(unity_position.z,2)) + 37.8398*unity_position.z + 38.1231); 
  
  
            
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
