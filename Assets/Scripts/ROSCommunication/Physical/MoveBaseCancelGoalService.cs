using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

/// <summary>
///     This script sends a service request to 
///     cancel all goals to move base
/// </summary>
public class MoveBaseCancelGoalService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string moveBaseCancelGoalServiceName = 
        "move_base/cancel_goal";

    // Message
    private EmptyRequest emptyRequest;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<EmptyRequest, EmptyResponse>(
            moveBaseCancelGoalServiceName
        );

        // Initialize service request
        emptyRequest = new EmptyRequest();
    }

    void Update() {}

    // Request service to cancel all goal(s) to move base
    public void CancelGoalCommand()
    {
        // Request service
        ros.SendServiceMessage<EmptyResponse>(
            moveBaseCancelGoalServiceName, emptyRequest, CancelGoalCallback
        );
    }

    private void CancelGoalCallback(EmptyResponse response) {}
}
