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

    // path
    private bool isNewPathRecieved;
    private PathMsg path;
    private Vector3[] LocalWaypoints;
    // private Vector3[] waypoints;
    // public ROSAutoNaviagation AutoNav;

    void Start()
    {
        // Get the ros connection
        ros = ROSConnection.GetOrCreateInstance();

        // it flag
        isNewPathRecieved = false;

        // Subscribe
        ros.Subscribe<PathMsg>(localPlannerTopicName, Callback);

    }

    void Update()
    {
        if(isNewPathRecieved)
        {
            // Create an empty list
            LocalWaypoints = ConvertPathToArray(path);
            isNewPathRecieved = false;
        }
        
    }

    public Vector3[] ConvertPathToArray(PathMsg apath)
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

    public Vector3[] getLocalWaypoints()
    {
        return LocalWaypoints;
    }

    void Callback(PathMsg apath)
    {
        path = apath;
        isNewPathRecieved = true;
    }
    
    


}