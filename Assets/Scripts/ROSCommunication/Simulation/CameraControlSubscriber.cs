using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;

/// <summary>
///     This script subscribes camera joints' control commands.
/// </summary>
public class CameraControlSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;

    // Variables required for ROS communication
    public string cameraYawControllerTopicName = "cam/yaw_pos_cmd";
    public string cameraPitchControllerTopicName = "cam/pitch_pos_cmd";
    
    // Robot controller
    public ArticulationCameraController cameraController;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();

        // Subscribers
        ros.Subscribe<Float64Msg>(cameraYawControllerTopicName, moveYawJoint);
        ros.Subscribe<Float64Msg>(cameraPitchControllerTopicName, movePitchJoint);
    }

    void Update()
    {
    }

    // Callback functions
    private void moveYawJoint(Float64Msg target)
    {
        var (yaw, pitch) = cameraController.GetCameraJoints();
        cameraController.SetCameraJoints((float)target.data, pitch);
    }
    private void movePitchJoint(Float64Msg target)
    {
        var (yaw, pitch) = cameraController.GetCameraJoints();
        cameraController.SetCameraJoints(yaw, (float)target.data);
    }
}