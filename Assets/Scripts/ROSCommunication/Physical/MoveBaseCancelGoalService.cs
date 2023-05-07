using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;


public class MoveBaseCancelGoalService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string moveBaseCancelGoalServiceName = "move_base/cancel_goal";

    // Message
    private EmptyRequest emptyCommand;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<EmptyRequest, EmptyResponse>(
            moveBaseCancelGoalServiceName
        );
    }

    void Update() {}

    // Request service to cancel all goal(s) to move base
    public void CancelGoalCommandService()
    {
        // Create Empty Request
        EmptyRequest emptyRequest = new EmptyRequest();
        
    
        // Request service
        ros.SendServiceMessage<EmptyResponse>(
            moveBaseCancelGoalServiceName, emptyRequest, CancelGoalCallback
        );
    }

    // Callback function for service response
    private void CancelGoalCallback(EmptyResponse response)
    {
        // Debug.Log("Home command response: " + response.state);
    }
}
