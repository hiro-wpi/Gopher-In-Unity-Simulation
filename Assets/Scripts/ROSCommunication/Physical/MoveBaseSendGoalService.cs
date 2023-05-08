using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;


public class MoveBaseSendGoalService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string moveBaseSendGoalServiceName = "move_base/send_goal";

    // Message
    private GetPlanRequest getPlanCommand;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<GetPlanRequest, GetPlanResponse>(
            moveBaseSendGoalServiceName
        );
    }

    void Update() {}

    // Request service to send the goal to move base
    public void SendGoalCommandService(Vector3 postion, Vector3 rotation)
    {
        // homeCommand.command = true;
        // HomingRequest homingRequest = new HomingRequest(true);
        GetPlanRequest goalRequest = new GetPlanRequest();
        
        goalRequest.goal.header.frame_id = "map";
        goalRequest.goal.pose.position = postion.To<FLU>();

        Quaternion rot = Quaternion.LookRotation(rotation, Vector3.up);
        goalRequest.goal.pose.orientation = rot.To<FLU>();

        Debug.Log("Sending Send Goal Ros Request");
        // Request service
        ros.SendServiceMessage<GetPlanResponse>(
            moveBaseSendGoalServiceName, goalRequest, GoalCallback
        );
    }

    // Callback function for service response
    private void GoalCallback(GetPlanResponse response)
    {
        // Debug.Log("Home command response: " + response.state);
    }
}
