using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.KortexDriver;

/// <summary>
///     This script sends a service request to set gripper position
/// </summary>
public class GripperCommandService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string gripperCommandServiceName = "send_gripper_command";

    // Message
    private SendGripperCommandRequest sendGripperCommand;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<SendGripperCommandRequest, SendGripperCommandResponse>(
            gripperCommandServiceName
        );

        // Initialize service and related messages
        sendGripperCommand = new SendGripperCommandRequest
        {
            input = new GripperCommandMsg
            {
                mode = 3,  // 1: force, 2: velocity, 3: position
                gripper = new GripperMsg
                {
                    finger = new FingerMsg[] {}
                },
                duration = 0
            }
        };
    }

    void Update() {}

    // Request service to set gripper position
    public void SendGripperCommandService(float position)
    {
        // Update value
        sendGripperCommand.input.gripper.finger = new FingerMsg[]
        {
            new FingerMsg
            {
                finger_identifier = 0,
                value = position
            }
        };

        // Request service
        ros.SendServiceMessage<SendGripperCommandResponse>(
            gripperCommandServiceName, sendGripperCommand, GripperCommandCallback
        );
    }

    // Callback function for service response
    private void GripperCommandCallback(SendGripperCommandResponse response)
    {
        // Debug.Log("Gripper command response: " + response.output);
    }
}
