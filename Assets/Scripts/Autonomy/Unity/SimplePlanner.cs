using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlanner : MonoBehaviour
{
    [SerializeField] private Transform startTF;
    [SerializeField] private Transform goalTF;
    [SerializeField] private JacobianIK iK;
    public ArticulationArmController armController;
    [SerializeField] private GameObject startGameObject;
    [SerializeField] private GameObject goalGameObject;
    [SerializeField] private GameObject waypointGameObject;

    public GameObject armEE;

    private int numWaypoints = 10;
    private float timeStep; // Time step in seconds
    private float speed = 0.05f; // Speed of the arm in m/s
    private int waypointDensityPerMeter = 33; // Number of waypoints per meter

    public bool goalReached = false;
    public bool motionInProgress = false;

    private bool debugVisual = false;

    void Start()
    {
        // armController.SetJointTargets(new float[] { 0, 0, 0, 0, 0, 0, 0 });
    }    
    void Update()
    {

    }

    public void PlanTrajectory(Vector3 startPosition, Quaternion startRotation, Vector3 goalPosition, Quaternion goalRotation)
    {
        if(motionInProgress)
        {
            Debug.LogWarning("Motion in progress, cannot plan trajectory");
            return;
        }

        if(debugVisual)
        {
            // Visualize Start and Goal
            VisualizeStartAndGoal(startPosition, startRotation, goalPosition, goalRotation);
        }
        
        // Collision Check
        if (!CheckForCollisionFreePath(startPosition, goalPosition))
        {
            Debug.LogWarning("Imminent Collision Detected, No Direct Path Found");
            return;
        }

        // Calculate time step
        float distance = Vector3.Distance(startPosition, goalPosition);
        numWaypoints = Mathf.RoundToInt(distance*waypointDensityPerMeter);
        timeStep = GetTimeStep(distance, speed, numWaypoints);
       
        // Interpolate between Start and Goal (positions and rotations)
        List<Vector3> positions = InterpolatePositions(startPosition, goalPosition);
        List<Quaternion> rotations = InterpolateRotations(startRotation, goalRotation);

        
        // Generate an array of waypoints between Start and Goal (positions and rotations)
        List<Vector3> waypointsPositions = GenerateWaypoints(positions);
        List<Quaternion> waypointsRotations = GenerateWaypoints(rotations);

        if(debugVisual)
        {
            for (int i = 0; i < waypointsPositions.Count; i++)
            {
                VisualizeWaypoint(waypointsPositions[i], waypointsRotations[i]);
            }
        }
        
        // Initcialize lists to store joint angles and time steps
        List<float[]> jointAngles = new List<float[]>();
        List<float> timeSteps = new List<float>();
        float[] jointAngle = armController.GetCurrentJointAngles();

        // Initial joint angles
        // jointAngles.Add(jointAngle);

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

        // Convert jointAngles list and timeSteps list to arrays
        float[][] jointAnglesArray = jointAngles.ToArray();
        float[] timeStepsArray = timeSteps.ToArray();

        // Check if the goal configuration is possible
        if(!CheckGoalConfiguration(jointAngle, goalPosition, goalRotation))
        {
            Debug.Log("Goal Arm Configuration Not Achievable, No Trajectory Sent");
            return;
        }

        // Call SetJointTrajectory method
        // Goal Arm Configuration Achievable, sending trajectory to arm controller;
        armController.SetJointTrajectory(timeStepsArray, jointAnglesArray, new float[][] { }, new float[][] { });
        motionInProgress = true;

        // Check if the goal is reached at the end of the time
        StartCoroutine(CheckGoalReached(timeStepsArray[timeStepsArray.Length - 1], goalPosition));

    }

    private bool CheckForCollisionFreePath(Vector3 startPosition, Vector3 goalPosition)
    {
        // Check if there is a collision free path between start and goal
        // If there is a collision free path, return true
        // If there is no collision free path, return false

        // ignore collisions with graspable objects

        Vector3 direction = goalPosition - startPosition;
        // Ray ray = new Ray(n.previousNode.position, direction);
        float maxDistance = Vector3.Distance(goalPosition, startPosition);

        if (Physics.Raycast(startPosition, direction, out RaycastHit hit, maxDistance))
        {
            // collision detected

            // Filtering Collision With Graspable Objects
            if (hit.collider.gameObject.CompareTag("GraspableObject"))
            {
                return true;
            }

            // Filtering Collision With the Robot
            int robotLayerNum = 15;
            if (hit.collider.gameObject.layer == robotLayerNum)
            {
                return true;
            }

            // Debug.LogWarning("Collision detected from " + hit.collider.name + " with layer number " + hit.collider.gameObject.layer);

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


    private void VisualizeStartAndGoal(Vector3 startPosition, Quaternion startRotation, Vector3 goalPosition, Quaternion goalRotation)
    {
        Instantiate(startGameObject, startPosition, startRotation, startGameObject.transform.parent);
        Instantiate(goalGameObject, goalPosition, goalRotation, goalGameObject.transform.parent);
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

    IEnumerator CheckGoalReached(float timer, Vector3 goal)
    {
        motionInProgress = true;
        goalReached = false;
        float timebuffer = 1f;
        float startTime = Time.time;
        float timeElapsed = 0f;
        
        Vector3 prevArmPos = armEE.transform.position;
        
        while(timeElapsed < timer + 5f)
        {
            Vector3 armPosition = armEE.transform.position;

            // Distance from goal
            float distance = Vector3.Distance(armPosition, goal);

            if (distance < 0.01f)
            {
                // Goal Reached

                // Give it some time to properly stop
                yield return new WaitForSeconds(timebuffer);

                // Signal that we are done, ready to move on to the next motion
                goalReached = true;
                motionInProgress = false;

                yield break;
            }
            
            // update timeElapsed
            timeElapsed = Time.time - startTime;

            yield return null;
        }

        // failure. We didn't get to the goal in the expected time, or something else occured
        goalReached = false;
        motionInProgress = false;

    }

    public bool CheckGoalConfiguration(float[] jointAngles, Vector3 goalPosition, Quaternion goalRotation)
    {
        return iK.CheckGoalReached(jointAngles, goalPosition, goalRotation);
    }

   

}

