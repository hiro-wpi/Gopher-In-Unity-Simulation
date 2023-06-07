using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Nav;

/// <summary>
///     This script sends a service request
///     move base to plan a path to the goal
/// </summary>
public class MoveBaseSendGoalService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string moveBaseSendGoalServiceName = 
        "move_base/send_goal";
    [SerializeField] private string frameID = "map";

    // Message
    private GetPlanRequest goalRequest;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<GetPlanRequest, GetPlanResponse>(
            moveBaseSendGoalServiceName
        );

        // Initialize service request
        goalRequest = new GetPlanRequest();
    }

    void Update() {}

    // Request service to send the goal to move base
    public void SendGoalCommandService(Vector3 postion, Vector3 rotation)
    {
        // Only need to specify the goal
        goalRequest.goal.header = new HeaderMsg(
            Clock.GetCount(), new TimeStamp(Clock.time), frameID
        );
        goalRequest.goal.pose.position = postion.To<FLU>();
        goalRequest.goal.pose.orientation = Quaternion.Euler(rotation).To<FLU>();

        // Request service
        ros.SendServiceMessage<GetPlanResponse>(
            moveBaseSendGoalServiceName, goalRequest, GoalCallback
        );
    }

    private void GoalCallback(GetPlanResponse response) {}
}
