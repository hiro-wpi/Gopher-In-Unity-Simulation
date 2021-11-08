using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;


public class JointControlSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;

    // Variables required for ROS communication
    public string joint1ControllerTopicName = "joint_1_position_controller/command";
    public string joint2ControllerTopicName = "joint_2_position_controller/command";
    public string joint3ControllerTopicName = "joint_3_position_controller/command";
    public string joint4ControllerTopicName = "joint_4_position_controller/command";
    public string joint5ControllerTopicName = "joint_5_position_controller/command";
    public string joint6ControllerTopicName = "joint_6_position_controller/command";
    public string joint7ControllerTopicName = "joint_7_position_controller/command";
    
    // Robot object
    public GameObject robot;
    // Articulation Bodies
    public float[] homePosition = {0f, 0f, 0f, 0f, 0f, 0f, 0f};
    private ArticulationBody[] articulationChain;

    // Start is called before the first frame update
    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.instance;

        // Get joints
        articulationChain = robot.GetComponentsInChildren<ArticulationBody>();
        articulationChain = articulationChain.Where(joint => joint.jointType 
                                                    != ArticulationJointType.FixedJoint).ToArray();

        // Initialize robot position
        HomeRobot();

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

    public void HomeRobot()
    {
        for (int i = 0; i < homePosition.Length; ++i)
            if (articulationChain[i].xDrive.target != homePosition[i])
            {
                moveJoint(i, homePosition[i]);
            }
    }

    // Callback functions
    public void moveJoint(int jointNum, float target)
    {
        ArticulationDrive drive = articulationChain[jointNum].xDrive;
        drive.target = target;
        articulationChain[jointNum].xDrive = drive;
    }

    private void moveJoint1(Float64Msg target)
    {
        moveJoint(0, (float)target.data);
    }

    private void moveJoint2(Float64Msg target)
    {
        moveJoint(1, (float)target.data);
    }

    private void moveJoint3(Float64Msg target)
    {
        moveJoint(2, (float)target.data);
    }

    private void moveJoint4(Float64Msg target)
    {
        moveJoint(3, (float)target.data);
    }

    private void moveJoint5(Float64Msg target)
    {
        moveJoint(4, (float)target.data);
    }

    private void moveJoint6(Float64Msg target)
    {
        moveJoint(5, (float)target.data);
    }

    private void moveJoint7(Float64Msg target)
    {
        moveJoint(6, (float)target.data);
    }
}
