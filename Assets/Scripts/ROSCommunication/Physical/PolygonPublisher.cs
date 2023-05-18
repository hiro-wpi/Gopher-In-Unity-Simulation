using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;

/// <summary>
///     This script publishes twist msg to a ROS topic
/// </summary>
public class PolygonPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string PolygonTopicName = "move_base/footprint";

    // Message
    private PolygonMsg polygon;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PolygonMsg>(PolygonTopicName);

        // Initialize message
        twist = new TwistMsg();
    }

    public void PublishPolygon(Vector3[] poly)
    {
        // Convert all vectors to points in
        List<Point32msg> pointList = new List<Point32msg>();
        foreach (Vector3 p in poly)
        {
            Point32msg point = new Point32msg();
            point.x = p.To<FLU>().x;
            point.y = p.To<FLU>().y;
            pointList.append(point);
        }

        // Convert list to arrray
        pointArray = array(pointList);

        ros.Publish(twistTopicName, pointArray);
    }
}
