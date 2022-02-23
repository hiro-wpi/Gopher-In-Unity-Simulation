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
    
    // Robot object
    public ArticulationBody cameraYawJoint;
    public ArticulationBody cameraPitchJoint;

    // Start is called before the first frame update
    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();

        // Initialize robot position
        HomeRobot();

        // Subscribers
        ros.Subscribe<Float64Msg>(cameraYawControllerTopicName, moveYawJoint);
        ros.Subscribe<Float64Msg>(cameraPitchControllerTopicName, movePitchJoint);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void HomeRobot()
    {
        if (cameraYawJoint.xDrive.target != 0f)
            moveJoint(cameraYawJoint, 0f);
        if (cameraPitchJoint.xDrive.target != 0f)
            moveJoint(cameraPitchJoint, 0f);
    }

    // Callback functions
    public void moveJoint(ArticulationBody joint, float target)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.target = target;
        joint.xDrive = drive;
    }
    private void moveYawJoint(Float64Msg target)
    {
        moveJoint(cameraYawJoint, (float)target.data);
    }
    private void movePitchJoint(Float64Msg target)
    {
        moveJoint(cameraPitchJoint, (float)target.data);
    }
}