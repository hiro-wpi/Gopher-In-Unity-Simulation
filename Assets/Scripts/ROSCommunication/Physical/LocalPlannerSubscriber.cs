using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Nav;

/// <summary>
///      This script subscribes to a local path topic
/// </summary>
public class LocalPlannerSubscriber : MonoBehaviour
{
    //ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string localPlannerTopicName = "local_plan";

    // Message
    private bool isNewPathReceived;
    private PathMsg path;
    private Vector3[] LocalWaypoints = new Vector3[0];

    void Start()
    {
        // Get the ros connection
        ros = ROSConnection.GetOrCreateInstance();

        // Init flag
        isNewPathReceived = false;

        // Subscribe
        ros.Subscribe<PathMsg>(localPlannerTopicName, ReceiveNewPath);
    }

    void Update()
    {
        if(isNewPathReceived)
        {
            // Create an empty list
            LocalWaypoints = ConvertPathToArray(path);
            isNewPathReceived = false;
        }
    }

    private void ReceiveNewPath(PathMsg newPath)
    {
        path = newPath;
        isNewPathReceived = true;
    }

    public Vector3[] getLocalWaypoints()
    {
        return LocalWaypoints;
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