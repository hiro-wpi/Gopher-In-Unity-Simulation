using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlanner : MonoBehaviour
{
    [SerializeField] private Transform start;
    [SerializeField] private Transform goal;
    [SerializeField] private JacobianIK iK;
    [SerializeField] private ArticulationArmController armController;

    [SerializeField] private float timeStep = 0.5f; // Time step in seconds

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Planning Trajectory");
            PlanTrajectory();
        }
    }

    private void PlanTrajectory()
    {
        // Interpolate between Start and Goal (positions and rotations)
        List<Vector3> positions = InterpolatePositions(start.position, goal.position);
        List<Quaternion> rotations = InterpolateRotations(start.rotation, goal.rotation);

        // Generate an array of waypoints between Start and Goal (positions and rotations)
        List<Vector3> waypointsPositions = GenerateWaypoints(positions);
        List<Quaternion> waypointsRotations = GenerateWaypoints(rotations);

        // for each point in waypoints
        List<float[]> jointAngles = new List<float[]>();
        List<float> timeSteps = new List<float>();
        float[] jointAngle = armController.GetCurrentJointAngles();
        jointAngles.Add(jointAngle);
        for (int i = 0; i < waypointsPositions.Count; i++)
        {
            jointAngle = iK.SolveIK(jointAngle, waypointsPositions[i], waypointsRotations[i]);
            jointAngles.Add(jointAngle);

            // Print out waypoints and joint angles for debugging
            Debug.Log($"Waypoint {i}: Position = {waypointsPositions[i]}, Rotation = {waypointsRotations[i]}, Joint Angles = {string.Join(", ", jointAngle)}");
            
            float currentTimeStep = GenerateTimeStep(i);
            timeSteps.Add(currentTimeStep);
        }

        // Convert jointAngles list and timeSteps list to arrays
        float[][] jointAnglesArray = jointAngles.ToArray();
        float[][] timeStepsArray = new float[][] { timeSteps.ToArray() };

        // Call SetJointTrajectory method
        armController.SetJointTrajectory(timeStepsArray[0], jointAnglesArray, new float[][] { }, new float[][] { });
    }

    private List<Vector3> InterpolatePositions(Vector3 start, Vector3 goal)
    {
        List<Vector3> positions = new List<Vector3>();
        int numWaypoints = 10;

        for (int i = 0; i <= numWaypoints; i++)
        {
            float t = i / (float)numWaypoints;
            positions.Add(Vector3.Lerp(start, goal, t));
        }

        return positions;
    }

    private List<Quaternion> InterpolateRotations(Quaternion start, Quaternion goal)
    {
        List<Quaternion> rotations = new List<Quaternion>();
        int numWaypoints = 10;

        for (int i = 0; i <= numWaypoints; i++)
        {
            float t = i / (float)numWaypoints;
            rotations.Add(Quaternion.Slerp(start, goal, t));
        }

        return rotations;
    }

    private List<Vector3> GenerateWaypoints(List<Vector3> keyframes)
    {
        return keyframes;
    }

    private List<Quaternion> GenerateWaypoints(List<Quaternion> keyframes)
    {
        return keyframes;
    }

    private float GenerateTimeStep(int index)
    {
        return index * timeStep;
    }
}

