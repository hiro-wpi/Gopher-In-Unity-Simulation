using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RosMessageTypes.GopherMoveItConfig;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

/// <summary>
///     Use Moveit planner to move the joint to 
///     a given target
/// </summary>
public class TrajectoryPlanner : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    public string plannerServiceName = "kinova/moveit";

    // Joint controllers
    private int numJoint;
    public GameObject robotRoot;
    public ArticulationJointController jointController;
    public ArticulationGripperController gripperController;

    // Traget
    public GameObject target;
    private Transform targetTransform;
    // assume the target is on a flat floor -> gripper pointing downwards
    private Quaternion pickOrientation = Quaternion.Euler(180, 90, 0);
    readonly Vector3 pickPoseOffset = Vector3.up * 0.1f;

    // Service
    private JointsMsg joints;
    private PlanTrajectoryRequest request;
    private PlanTrajectoryResponse response;
    
    // TODO this should be removed in the future as the 
    // motion time should be determined by the trajectory
    private const float waitTimeAfterWaypoint = 0.03f;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<PlanTrajectoryRequest, 
                               PlanTrajectoryResponse>(plannerServiceName);
    
        // Robots
        numJoint = jointController.numJoint;
    }
    
    public void PlanTrajectory()
    {
        // Initialize request;
        request = new PlanTrajectoryRequest();
        joints = new JointsMsg();

        // Get current joint angles
        float[] currJointTargets = jointController.GetCurrentJointTargets();
        joints.joints = currJointTargets.Select(r => (double)r).ToArray();
        request.joints = joints;

        // Target position and rotation
        // w.r.t. robot base
        Vector3 position = robotRoot.transform.InverseTransformPoint(
                                               target.transform.position) +
                           pickPoseOffset;
        Quaternion rotation = Quaternion.Inverse(robotRoot.transform.rotation) *
                                                 pickOrientation;
        // Unity coordinate -> ROS coordinate
        request.target.position = position.To<FLU>();
        request.target.orientation = pickOrientation.To<FLU>();

        ros.SendServiceMessage<PlanTrajectoryResponse>(plannerServiceName, request, 
                                                       TrajectoryResponse);
    }
    
    private void TrajectoryResponse(PlanTrajectoryResponse response)
    {
        // Use coroutine to prevent blocking the program
        var points = response.trajectory.joint_trajectory.points;
        if (points.Length > 0)
        {
            Debug.Log("Trajectory returned.");
            StartCoroutine(ExecuteTrajectory(response));
        }
        else
        {
            Debug.LogWarning("No trajectory returned from MoveIt service.");
        }
    }

    private IEnumerator ExecuteTrajectory(PlanTrajectoryResponse response)
    {
        var points = response.trajectory.joint_trajectory.points;

        foreach (var point in points)
        {
            float[] positions = point.positions.Select(r => (float)r).ToArray();
            float[] velocities = point.velocities.Select(r => (float)r).ToArray();

            for (int joint = 0; joint < numJoint; joint++)
            {
                jointController.SetJointTarget(joint, positions[joint]);
            }

            yield return new WaitForSeconds(waitTimeAfterWaypoint);
        }

        yield return new WaitForSeconds(0.5f);
        gripperController.CloseGrippers();
    }
}
