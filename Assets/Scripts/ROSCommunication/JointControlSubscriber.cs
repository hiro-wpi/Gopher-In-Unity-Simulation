using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;

/// <summary>
///     This script subscribes joints' control commands.
/// </summary>
public class JointControlSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;

    // Variables required for ROS communication
    public string joint1ControllerTopicName = "j1_pos_cmd";
    public string joint2ControllerTopicName = "j2_pos_cmd";
    public string joint3ControllerTopicName = "j3_pos_cmd";
    public string joint4ControllerTopicName = "j4_pos_cmd";
    public string joint5ControllerTopicName = "j5_pos_cmd";
    public string joint6ControllerTopicName = "j6_pos_cmd";
    public string joint7ControllerTopicName = "j7_pos_cmd";
    
    // Robot controller
    public ArticulationJointController jointController;

    // Start is called before the first frame update
    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();

        // Subscribers
        ros.Subscribe<Float64Msg>(joint1ControllerTopicName, moveJoint1);
        ros.Subscribe<Float64Msg>(joint2ControllerTopicName, moveJoint2);
        ros.Subscribe<Float64Msg>(joint3ControllerTopicName, moveJoint3);
        ros.Subscribe<Float64Msg>(joint4ControllerTopicName, moveJoint4);
        ros.Subscribe<Float64Msg>(joint5ControllerTopicName, moveJoint5);
        ros.Subscribe<Float64Msg>(joint6ControllerTopicName, moveJoint6);
        ros.Subscribe<Float64Msg>(joint7ControllerTopicName, moveJoint7);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void moveJoint1(Float64Msg target)
    {
        jointController.SetJointTarget(0, (float)target.data);
    }

    private void moveJoint2(Float64Msg target)
    {
        jointController.SetJointTarget(1, (float)target.data);
    }

    private void moveJoint3(Float64Msg target)
    {
        jointController.SetJointTarget(2, (float)target.data);
    }

    private void moveJoint4(Float64Msg target)
    {
        jointController.SetJointTarget(3, (float)target.data);
    }

    private void moveJoint5(Float64Msg target)
    {
        jointController.SetJointTarget(4, (float)target.data);
    }

    private void moveJoint6(Float64Msg target)
    {
        jointController.SetJointTarget(5, (float)target.data);
    }

    private void moveJoint7(Float64Msg target)
    {
        jointController.SetJointTarget(6, (float)target.data);
    }
}
