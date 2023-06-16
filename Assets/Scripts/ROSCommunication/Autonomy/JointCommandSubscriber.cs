using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;
using RosMessageTypes.KortexDriver;


/// <summary>
/// Fast prototyping code.
/// This class is used to subscribe to the Relaxed IK joint commands.
/// </summary>
public class JointCommandSubscriber : MonoBehaviour
{
    /*
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string jointTopicName = "arm/relaxed_ik/joint_angle_solutions";
    [SerializeField] private string gripperTopicName = "arm/teleoperation/gripper_button";
    private bool prevGripperState = false;

    // Message poseStampedTopicName
    [SerializeField] private int jointLength = 7;
    private JointAnglesMsg jointAnglesMsg;

    // Local controller
    [SerializeField] private ArticulationArmController armController;
    [SerializeField] private ArticulationJointController jointController;
    [SerializeField] private ArticulationGripperController gripperController;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<JointAnglesMsg>(jointTopicName, jointCommandCallback);
        ros.Subscribe<BoolMsg>(gripperTopicName, gripperCommandCallback);

        // Local controller to autonomy mode
        armController.SwitchToAutonomyControl();
    }

    void Update() {}

    void jointCommandCallback(JointAnglesMsg msg)
    {
        // Set joint values
        float[] joint_values = new float[jointLength];
        for (int i = 0; i < jointLength; i++)
        {
            joint_values[i] = msg.joint_angles[i].value;
        }

        // Send command to local controller
        jointController.SetJointTargetsStep(joint_values);
    }

    void gripperCommandCallback(BoolMsg msg)
    {
        // Set joint values
        if ((prevGripperState == false) && (msg.data == true))
        {
            gripperController.ChangeGripperStatus();
        }

        prevGripperState = msg.data;
    }
    */
}
