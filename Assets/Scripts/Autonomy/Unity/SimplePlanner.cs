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
    [SerializeField] private float timeStep; // Time step in seconds
    
    private float speed = 0.02f; // Speed of the arm in m/s
    private int waypointDensityPerMeter = 33; // Number of waypoints per meter

    public bool goalReached = false;
    public bool motionInProgress = false;

    private bool debug = false;

    [SerializeField] private 

    void Start()
    {
        // armController.SetJointTargets(new float[] { 0, 0, 0, 0, 0, 0, 0 });
    }    
    void Update()
    {
    }

    public void PlanTrajectory(Transform start, Transform goal)
    {
        if(motionInProgress)
        {
            Debug.Log("Motion in progress, cannot plan trajectory");
            return;
        }

        if(debug)
        {
            // Visualize Start and Goal
            VisualizeStartAndGoal(start, goal);
        }
        
        // Collision Check
        if (!CheckForCollisionFreePath(start, goal))
        {
            Debug.Log("Collision Detected, No Path Found");
            return;
        }

        // Saving a instance of the goal position
        Vector3 goalPosition = goal.position;

        // Calculate time step
        float distance = Vector3.Distance(start.position, goal.position);
        timeStep = GetTimeStep(distance, speed, Mathf.RoundToInt(distance*waypointDensityPerMeter));
       
        // Interpolate between Start and Goal (positions and rotations)
        List<Vector3> positions = InterpolatePositions(start.position, goal.position);
        List<Quaternion> rotations = InterpolateRotations(start.rotation, goal.rotation);

        
        // Generate an array of waypoints between Start and Goal (positions and rotations)
        List<Vector3> waypointsPositions = GenerateWaypoints(positions);
        List<Quaternion> waypointsRotations = GenerateWaypoints(rotations);

        if(debug)
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
        jointAngles.Add(jointAngle); // Last joint angles
        timeSteps.Add(GenerateTimeStep(waypointsPositions.Count));

        // Convert jointAngles list and timeSteps list to arrays
        float[][] jointAnglesArray = jointAngles.ToArray();
        float[] timeStepsArray = timeSteps.ToArray();

        // Check if the goal configuration is possible
        if(!CheckGoalConfiguration(jointAngle, goal))
        {
            Debug.Log("Goal Arm Configuration Not Achievable, No Trajectory Sent");
            return;
        }

        // Call SetJointTrajectory method
        Debug.Log("Goal Arm Configuration Achievable, sending trajectory to arm controller");
        armController.SetJointTrajectory(timeStepsArray, jointAnglesArray, new float[][] { }, new float[][] { });
        motionInProgress = true;

        // Check if the goal is reached at the end of the time
        // Debug.Log(timeStepsArray[timeStepsArray.Length - 1]);
        // Debug.Log(timeStepsArray.Length - 1);
        // Debug.Log(timeStepsArray[timeStepsArray.Length - 1]);
        StartCoroutine(CheckGoalReached(timeStepsArray[timeStepsArray.Length - 1], goalPosition));

        // // Handle the post action
        // HandleInstruction(postAction);
    }


    public void PlanTrajectory(Vector3 startPosition, Quaternion startRotation, Vector3 goalPosition, Quaternion goalRotation)
    {
        if(motionInProgress)
        {
            Debug.Log("Motion in progress, cannot plan trajectory");
            return;
        }

        // if(debug)
        // {
        //     // Visualize Start and Goal
        //     VisualizeStartAndGoal(start, goal);
        // }
        
        // Collision Check
        if (!CheckForCollisionFreePath(startPosition, goalPosition))
        {
            Debug.Log("Collision Detected, No Path Found");
            return;
        }

        // // Saving a instance of the goal position
        // Vector3 goalPosition = goal.position;

        // Calculate time step
        float distance = Vector3.Distance(startPosition, goalPosition);
        timeStep = GetTimeStep(distance, speed, Mathf.RoundToInt(distance*waypointDensityPerMeter));
       
        // Interpolate between Start and Goal (positions and rotations)
        List<Vector3> positions = InterpolatePositions(startPosition, goalPosition);
        List<Quaternion> rotations = InterpolateRotations(startRotation, goalRotation);

        
        // Generate an array of waypoints between Start and Goal (positions and rotations)
        List<Vector3> waypointsPositions = GenerateWaypoints(positions);
        List<Quaternion> waypointsRotations = GenerateWaypoints(rotations);

        if(debug)
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
        jointAngles.Add(jointAngle); // Last joint angles
        timeSteps.Add(GenerateTimeStep(waypointsPositions.Count));

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
        Debug.Log("Goal Arm Configuration Achievable, sending trajectory to arm controller");
        armController.SetJointTrajectory(timeStepsArray, jointAnglesArray, new float[][] { }, new float[][] { });
        motionInProgress = true;

        // Check if the goal is reached at the end of the time
        // Debug.Log(timeStepsArray[timeStepsArray.Length - 1]);
        // Debug.Log(timeStepsArray.Length - 1);
        // Debug.Log(timeStepsArray[timeStepsArray.Length - 1]);
        StartCoroutine(CheckGoalReached(timeStepsArray[timeStepsArray.Length - 1], goalPosition));

        // // Handle the post action
        // HandleInstruction(postAction);
    }

    private bool CheckForCollisionFreePath(Transform start, Transform goal)
    {
        // Check if there is a collision free path between start and goal
        // If there is a collision free path, return true
        // If there is no collision free path, return false

        // ignore collisions with graspable objects

        Vector3 direction = goal.position - start.position;
        // Ray ray = new Ray(n.previousNode.position, direction);
        float maxDistance = Vector3.Distance(goal.position, start.position);

        if (Physics.Raycast(start.position, direction, out RaycastHit hit, maxDistance))
        {
            // collision detected
            if (hit.collider.gameObject.CompareTag("GraspableObject"))
            {
                Debug.Log("Collision detected with graspable object, ignoring collision");
                return true;
            }

            // Robot layer Number
            int robotLayerNum = 15;
            
            // ignoring collision with robot layer
            if (hit.collider.gameObject.layer == robotLayerNum)
            {
                Debug.Log("Collision detected with robot, ignoring collision");
                return true;
            }

            Debug.Log("Max Distance is " + maxDistance);
            Debug.Log("Collision detected with " + hit.collider.name);
            Debug.Log("Collision detected with " + hit.collider.gameObject.layer + " type of object");
            Debug.DrawRay(start.position, direction, Color.red, 100f);
            Debug.Log(start.position);
            return false;
        }

        // No collision

        return true;
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
            if (hit.collider.gameObject.CompareTag("GraspableObject"))
            {
                Debug.Log("Collision detected with graspable object, ignoring collision");
                return true;
            }

            // Robot layer Number
            int robotLayerNum = 15;
            
            // ignoring collision with robot layer
            if (hit.collider.gameObject.layer == robotLayerNum)
            {
                Debug.Log("Collision detected with robot, ignoring collision");
                return true;
            }

            Debug.Log("Max Distance is " + maxDistance);
            Debug.Log("Collision detected with " + hit.collider.name);
            Debug.Log("Collision detected with " + hit.collider.gameObject.layer + " type of object");
            Debug.DrawRay(startPosition, direction, Color.red, 100f);
            Debug.Log(startPosition);
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

    IEnumerator CheckGoalReached(float time, Vector3 goal)
    {
        motionInProgress = true;
        goalReached = false;

        float timebuffer = 0.05f;
        yield return new WaitForSeconds(time + timebuffer);
        // Proper Way for checking if the goal is reached
            // Use forward kinematics of the robot arm to see if the final pose is reached
        //Impropper way for checking if the goal is reached
            // We could also just as easily check the distance between the end effector gameobject tf and the goal
        
        motionInProgress = false;
        float distance = Vector3.Distance(armEE.transform.position, goal);

        if (distance < 0.05f)
        {
            goalReached = true;
            Debug.Log("Goal Reached");
        }
        else
        {
            goalReached = false;
        }
        
    }

    public bool CheckGoalConfiguration(float[] jointAngles, Transform goal)
    {
        return iK.CheckGoalReached(jointAngles, goal.position, goal.rotation);
    }

    public bool CheckGoalConfiguration(float[] jointAngles, Vector3 goalPosition, Quaternion goalRotation)
    {
        return iK.CheckGoalReached(jointAngles, goalPosition, goalRotation);
    }

   

}

