using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;


public class MoveBaseMakePlanService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string moveBaseMakePlanServiceName = "move_base/make_plan";
    
    private bool isGlobalWaypointsUpdated;
    private bool isPathUpdated;
    private Vector3[] globalWaypoints;
    private PathMsg planPath;
    // Message
    private GetPlanRequest getPlanCommand;

    void Start()
    {

        isGlobalWaypointsUpdated = false;
        isPathUpdated = false;
        globalWaypoints = new Vector3[0];

        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<GetPlanRequest, GetPlanResponse>(
            moveBaseMakePlanServiceName
        );
    }

    void Update()
    {
        if(isPathUpdated)
        {
            // convert the path to way points and store that
            globalWaypoints = ConvertPathToArray(planPath);

            // reset flag
            isPathUpdated = false;

            // indicate new waypoints ready
            isGlobalWaypointsUpdated = true;
        }

    }

    public Vector3[] ConvertPathToArray(PathMsg path)
    {
        // Create an empty list
        List<Vector3> vectorList = new List<Vector3>();

        // Iterates through the path and only adds the positions 
        foreach( var pose in path.poses)
        {
            vectorList.Add(pose.pose.position.From<FLU>());
        }
    
        // Converts the list to an array
        Vector3[] waypoints = vectorList.ToArray();
        
        return waypoints;
    }

    public void ResetWaypointFlagStatus()
    {
        isGlobalWaypointsUpdated = false;
    }

    public bool GetWaypointFlagStatus()
    {
        return isGlobalWaypointsUpdated;
    }

    public Vector3[] GetGlobalWaypoints()
    {
        isGlobalWaypointsUpdated = false;
        return globalWaypoints;
    } 

    // Request service to send the goal to move base
    public void MakePlanCommandService(Vector3 postion, Vector3 rotation)
    {
        // homeCommand.command = true;
        // HomingRequest homingRequest = new HomingRequest(true);
        GetPlanRequest planRequest = new GetPlanRequest();
        
        planRequest.goal.header.frame_id = "map";
        planRequest.goal.pose.position = postion.To<FLU>();

        Quaternion rot = Quaternion.LookRotation(rotation, Vector3.up);
        planRequest.goal.pose.orientation = rot.To<FLU>();

        
        // Request service
        ros.SendServiceMessage<GetPlanResponse>(
            moveBaseMakePlanServiceName, planRequest, PlanCallback
        );
    }

    // Callback function for service response
    private void PlanCallback(GetPlanResponse response)
    {
        planPath = response.plan;
        isPathUpdated = true;
        // Debug.Log("Home command response: " + response.state);
    }
}
