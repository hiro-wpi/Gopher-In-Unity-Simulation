using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using RosMessageTypes.Moveit;

public class TrajectorySubscriber : MonoBehaviour
{
    public ArticulationArmController armController;

    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    public string topicName = "move_group/display_planned_path";
    
    
    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<DisplayTrajectoryMsg>(topicName, TrajectoryReceived);
    }

    private void TrajectoryReceived(DisplayTrajectoryMsg displayTrajectory)
    {
        // Notify service user
        var (timeSteps, angles, velocities, accelerations) = ConvertTrajectory(displayTrajectory);
        armController.SetJointTrajectory(timeSteps, angles, velocities, accelerations);
    }

    // Converter utils function
    private (float[], float[][], float[][], float[][]) ConvertTrajectory(
        DisplayTrajectoryMsg displayTrajectory
    )
    {
        // Initialize
        var points = displayTrajectory.trajectory[0].joint_trajectory.points;
        float[] timeSteps = new float[points.Length];
        float[][] angles = new float[points.Length][];
        float[][] velocities = new float[points.Length][];
        float[][] accelerations = new float[points.Length][];

        // Get trajectory
        for (int i = 0; i < points.Length; ++i)
        {
            timeSteps[i] = points[i].time_from_start.sec
                         + points[i].time_from_start.nanosec / 1e9f;
            angles[i] = points[i].positions.Select(d => (float)d).ToArray();
            velocities[i] = points[i].velocities.Select(d => (float)d).ToArray();
            accelerations[i] = points[i].accelerations.Select(d => (float)d).ToArray();
        }

        return (timeSteps, angles, velocities, accelerations);
    }
}
