using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlanner : MonoBehaviour
{
    [SerializeField] private Transform start;
    [SerializeField] private Transform goal;
    [SerializeField] private JacobianIK iK;
    [SerializeField] private ArticulationArmController armController;

    [SerializeField] private GameObject startGameObject;
    [SerializeField] private GameObject goalGameObject;

    [SerializeField] private GameObject waypointGameObject;

    private int numWaypoints = 10;
    [SerializeField] private float timeStep; // Time step in seconds
    
    private float speed = 0.05f; // Speed of the arm in m/s
    private int waypointDensityPerMeter = 33; // Number of waypoints per meter

    void Start()
    {
        // armController.SetJointTargets(new float[] { 0, 0, 0, 0, 0, 0, 0 });
    }    
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
        // Visualize Start and Goal
        VisualizeStartAndGoal(start, goal);

        // Collision Check
        if (!CheckForCollisionFreePath(start, goal))
        {
            Debug.Log("Collision Detected, No Path Found");
            return;
        }

        // Calculate time step
        float distance = Vector3.Distance(start.position, goal.position);
        timeStep = GetTimeStep(distance, speed, Mathf.RoundToInt(distance*waypointDensityPerMeter));

        VisualizeWaypoint(start.position, start.rotation);

        // Interpolate between Start and Goal (positions and rotations)
        List<Vector3> positions = InterpolatePositions(start.position, goal.position);
        List<Quaternion> rotations = InterpolateRotations(start.rotation, goal.rotation);

        
        // Generate an array of waypoints between Start and Goal (positions and rotations)
        List<Vector3> waypointsPositions = GenerateWaypoints(positions);
        List<Quaternion> waypointsRotations = GenerateWaypoints(rotations);

        for (int i = 0; i < waypointsPositions.Count; i++)
        {
            VisualizeWaypoint(waypointsPositions[i], waypointsRotations[i]);
        }


        // Initcialize lists to store joint angles and time steps
        List<float[]> jointAngles = new List<float[]>();
        List<float> timeSteps = new List<float>();
        float[] jointAngle = armController.GetCurrentJointAngles();

        // Initial joint angles
        jointAngles.Add(jointAngle);

        // Solve IK for each waypoint
        for (int i = 0; i < waypointsPositions.Count; i++)
        {
            // Solve to get new Joint Angles
            jointAngle = iK.SolveIK(jointAngle, waypointsPositions[i], waypointsRotations[i]);
            jointAngles.Add(jointAngle);

            // Print out waypoints and joint angles for debugging
            // Debug.Log($"Waypoint {i}: Position = {waypointsPositions[i]}, Rotation = {waypointsRotations[i]}, Joint Angles = {string.Join(", ", jointAngle)}");
            
            float currentTimeStep = GenerateTimeStep(i);
            timeSteps.Add(currentTimeStep);

            // Debug.Log(currentTimeStep);
        }

        //Account for the last waypoint not being reached
        jointAngles.Add(jointAngle);
        timeSteps.Add(GenerateTimeStep(waypointsPositions.Count));

        // Convert jointAngles list and timeSteps list to arrays
        float[][] jointAnglesArray = jointAngles.ToArray();
        float[] timeStepsArray = timeSteps.ToArray();

        // Call SetJointTrajectory method
        armController.SetJointTrajectory(timeStepsArray, jointAnglesArray, new float[][] { }, new float[][] { });
    }

    private bool CheckForCollisionFreePath(Transform start, Transform goal)
    {
        // Check if there is a collision free path between start and goal
        // If there is a collision free path, return true
        // If there is no collision free path, return false

        Vector3 direction = goal.position - start.position;
        // Ray ray = new Ray(n.previousNode.position, direction);
        float maxDistance = Vector3.Distance(goal.position, start.position);

        if (Physics.Raycast(start.position, direction, out RaycastHit hit, maxDistance))
        {
            // collision detected
            Debug.Log("Max Distance is " + maxDistance);
            Debug.Log("Collision detected with " + hit.collider.name);
            Debug.DrawRay(start.position, direction, Color.red, 100f);
            Debug.Log(start.position);
            return false;
        }

        // No collision

        return true;
    }


    private List<Vector3> InterpolatePositions(Vector3 start, Vector3 goal)
    {
        List<Vector3> positions = new List<Vector3>();
        
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

    private void VisualizeStartAndGoal(Transform start, Transform goal)
    {
        Instantiate(startGameObject, start.position, start.rotation, startGameObject.transform.parent);
        Instantiate(goalGameObject, goal.position, goal.rotation, goalGameObject.transform.parent);
    }

    private void VisualizeWaypoint(Vector3 position, Quaternion rotation)
    {
        Instantiate(waypointGameObject, position, rotation, waypointGameObject.transform.parent);
    }

    private float GetTimeStep(float distance, float speed, int waypoints)
    {
        float time = distance / speed;
        float timeStep = time / waypoints;

        return timeStep;
    }
}

