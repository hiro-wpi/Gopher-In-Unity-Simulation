using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;

/// <summary>
///     This script subscribes camera joints' control commands.
/// </summary>
public class CameraControlSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;

    // Variables required for ROS communication
    public string cameraVelocityTopicName = "cam/cmd_vel";
    
    // Robot controller
    public ArticulationCameraController cameraController;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();

        // Subscribers
        ros.Subscribe<TwistMsg>(cameraVelocityTopicName, moveCamera);
    }

    void Update() {}

    // Callback functions
    private void moveCamera(TwistMsg velocity)
    {
        cameraController.SetVelocity(velocity.angular.From<FLU>());
    }
}
