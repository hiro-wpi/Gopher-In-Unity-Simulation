using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Nav;

/// <summary>
///      This script subscribes to a local path topic
/// </summary>
public class PathSubscriber : MonoBehaviour
{
    //ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string localPlanTopicName = "local_plan";
    [SerializeField] private string globalPlanTopicName = "global_plan";

    // Message
    private bool isLocalPathReceived;
    private bool isGlobalPathReceived;
    private PathMsg localPath;
    private PathMsg globalPath;
    private Vector3[] LocalWaypoints = new Vector3[0];
    private Vector3[] GlobalWaypoints = new Vector3[0];

    void Start()
    {
        // Get the ros connection
        ros = ROSConnection.GetOrCreateInstance();

        // Init flag
        isLocalPathReceived = false;
        isGlobalPathReceived = false;

        // Subscribe
        ros.Subscribe<PathMsg>(localPlanTopicName, ReceiveLocalPath);
        ros.Subscribe<PathMsg>(globalPlanTopicName, ReceiveGlobalPath);
    }

    void Update()
    {
        if(isLocalPathReceived)
        {
            // Create an empty list
            LocalWaypoints = ConvertPathToArray(localPath);
            isLocalPathReceived = false;
        }

        if(isGlobalPathReceived)
        {
            // Create an empty list
            GlobalWaypoints = ConvertPathToArray(globalPath);
            isGlobalPathReceived = false;
        }
    }

    private void ReceiveLocalPath(PathMsg path)
    {
        localPath = path;
        isLocalPathReceived = true;
    }

    private void ReceiveGlobalPath(PathMsg path)
    {
        globalPath = path;
        isGlobalPathReceived = true;
    }

    public Vector3[] getLocalWaypoints()
    {
        return LocalWaypoints;
    }

    public Vector3[] getGlobalWaypoints()
    {
        return GlobalWaypoints;
    }

    // Utils
    private Vector3[] ConvertPathToArray(PathMsg path)
    {
        // Create an empty array
        var poses = path.poses;
        Vector3[] waypoints = new Vector3[poses.Length];

        // Iterates through the path and only adds the positions 
        for (int i = 0; i < poses.Length; i++)
        {
            waypoints[i] = poses[i].pose.position.From<FLU>();
        }
        return waypoints;
    }
}