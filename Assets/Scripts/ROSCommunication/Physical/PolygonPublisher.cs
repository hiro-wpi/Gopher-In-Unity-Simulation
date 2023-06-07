using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;

/// <summary>
///     This script publishes a poloygon msg to
///     a ROS topic to specify update footprint of the robot.
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
        // Convert vector3 array to point32Msg array
        Point32Msg[] points = new Point32Msg[poly.Length];
        for (int i = 0; i < poly.Length; i++)
        {
            points[i] = poly[i].To<FLU>();
        }
        
        polygon.points = points;

        ros.Publish(PolygonTopicName, polygon);
    }
}
