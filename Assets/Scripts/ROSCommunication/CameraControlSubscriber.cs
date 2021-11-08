using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;


public class CameraControlSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;

    // Variables required for ROS communication
    public string cameraYawControllerTopicName = "main_cam_yaw_controller/command";
    public string cameraPitchControllerTopicName = "main_cam_pitch_controller/command";
    
    // Robot object
    public GameObject cameraJoints;
    // Articulation Bodies
    private ArticulationBody[] articulationChain;

    // Start is called before the first frame update
    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.instance;

        // Get joints
        articulationChain = cameraJoints.GetComponentsInChildren<ArticulationBody>();
        articulationChain = articulationChain.Where(joint => joint.jointType 
                                                    != ArticulationJointType.FixedJoint).ToArray();

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
        for (int i = 0; i < articulationChain.Length; ++i)
            if (articulationChain[i].xDrive.target != 0f)
            {
                moveJoint(i, 0f);
            }
    }

    // Callback functions
    public void moveJoint(int jointNum, float target)
    {
        ArticulationDrive drive = articulationChain[jointNum].xDrive;
        drive.target = target;
        articulationChain[jointNum].xDrive = drive;
    }

    private void moveYawJoint(Float64Msg target)
    {
        moveJoint(0, (float)target.data);
    }

    private void movePitchJoint(Float64Msg target)
    {
        moveJoint(1, (float)target.data);
    }
}