using System.Collections;
using System.Collections.Generics;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessagesTypes.Nav;

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
    // private Vector3[] waypoints;
    public ROSAutoNaviagation AutoNav;

    void Start()
    {
        // Get the ros connection
        ros = ROSConnection.GetorCreateInstance();

        // it flag
        isNewPathRecieved = false;

        // Subsciber
        ros.Subsciber<PathMsg>(localPlannerTopicName, Callback)

    }

    void Update()
    {
        if(isNewPathRecieved)
        {
            // Create an empty list
            List<Vector3> vectorList = new List<Vector3>();

            // Iterates through the path and only adds the positions 
            foreach( var pose in path.poses)
            {
                vectorList.add(pose.position.From<FLU>());
            }
        
            // Converts the list to an array
            Vector3[] waypoints = vectorList.ToArray();

            // Updates the waypoints in the AutoNavigation
            AutoNav.setLocalWaypoints(waypoints);

            isNewPathRecieved = false;
        }
        

    }

    void Callback(PathMsg apath)
    {
        path = apath;
        isNewPathRecieved = true;
    }
    
    


}