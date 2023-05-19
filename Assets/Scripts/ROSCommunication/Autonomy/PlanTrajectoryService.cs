using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using RosMessageTypes.GopherMoveit;

/// <summary>
///     This script request a service to plan a trajectory
/// </summary>
public class PlanTrajectoryService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string planTrajectoryServiceName = "arm/plan_trajectory";

    // Service request and response
    private PlanTrajectoryRequest planTrajectoryCommand;
    private PlanTrajectoryResponse planTrajectoryResponse;
    // Callback function to inform the caller
    Action<float[], float[][], float[][], float[][]> responseCallback;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<PlanTrajectoryRequest, PlanTrajectoryResponse>(
            planTrajectoryServiceName
        );
    }

    void Update() {}

    public void SendPlanTrajectoryRequest(
        float[] currJointAngles, 
        Vector3 targetPosition, 
        Quaternion targetRotation, 
        Action<float[], float[][], float[][], float[][]> callback
    )
    {
        PlanTrajectoryRequest request = new();

        // Current Joint Angles
        request.joints_input = currJointAngles;

        // Target Pose
        request.target_pose = new PoseMsg
        {
            position = (targetPosition - Vector3.zero).To<FLU>(),
            orientation = targetRotation.To<FLU>()
        };

        // Send request
        responseCallback = callback;
        ros.SendServiceMessage<PlanTrajectoryResponse>(
            planTrajectoryServiceName, request, TrajectoryResponse
        );
    }

    private void TrajectoryResponse(PlanTrajectoryResponse response)
    {
        // Notify service user
        var (timeSteps, angles, velocities, accelerations) = ConvertTrajectory(response);
        responseCallback(timeSteps, angles, velocities, accelerations);
    }

    // Converter utils function
    private (float[], float[][], float[][], float[][]) ConvertTrajectory(
        PlanTrajectoryResponse response
    )
    {
        // Initialize
        var points = response.trajectory.joint_trajectory.points;
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
