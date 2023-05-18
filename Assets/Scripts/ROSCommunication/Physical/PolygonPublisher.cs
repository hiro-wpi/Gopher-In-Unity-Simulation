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
        polygon = new PolygonMsg();
    }

    public void PublishPolygon(Vector3[] poly)
    {
        // Convert all vectors to points in
        List<Point32Msg> pointList = new List<Point32Msg>();
        foreach (Vector3 p in poly)
        {
            Point32Msg point = new Point32Msg();
            point.x = p.To<FLU>().x;
            point.y = p.To<FLU>().y;
            pointList.Add(point);
        }

        // Convert list to arrray
        polygon.points = pointList.ToArray();

        ros.Publish(PolygonTopicName, polygon);
    }
}
