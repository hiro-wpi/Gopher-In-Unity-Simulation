using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining navigation task.
/// The task assumes only one robot is controlled and used during the task
/// It supports waypoint navigation in order by setting goals
/// Humans are used as dynamic obstacles
/// </summary>
public class NavigationTask : Task 
{
    // Spawn
    private GameObject robot;
    private SpawnInfo[] humanSpawnArray;
    private GameObject[] humans;
    
    // End condition
    private Vector3[] goals;
    private int goalIndex;
    private float goalDectionRadius;

    // Record data from sensors
    private float updateRate;
    private StateReader stateReader;
    private LaserSocial laser;
    private CollisionReader collisionReader;
    private int collisionStorageIndex;

    void Start()
    {
        // end condition
        goalIndex = 0;
        goalDectionRadius = 1.0f;
    }

    // Set human spawn array
    public void SetHumanSpawnArray(SpawnInfo[] humanSpawnArray)
    {
        this.humanSpawnArray = humanSpawnArray;
    }

    // Set up the goals
    public void SetGoals(Vector3[] goals)
    {
        this.goals = goals;
    }
    
    // Check if the current task is done
    public override int CheckTaskCompletion()
    {
        // If robot is not spawned yet
        if (robots == null)
        {
            return 0;
        }
        
        // Check if robot reaches the neighbor of the current goal
        float distance = (goals[goalIndex] - robot.transform.position).magnitude;
        if (distance < goalDectionRadius)
        {
            goalIndex++;
            if (goalIndex == goals.Length)
                return 1;
        }
        return 0;
    }

    // Generate objects for this task
    public override void GenerateObjects()
    {
        // objects
        objects = SpawnGameObjectArray(objectSpawnArray);
        
        // humans
        humans = SpawnGameObjectArray(humanSpawnArray);
        // set trajectory for each human
        for (int i=0; i<humanSpawnArray.Length; i++)
        {
            CharacterWalk characterWalk = humans[i].GetComponent<CharacterWalk>();
            characterWalk.SetTrajectory(humanSpawnArray[i].trajectory);
            characterWalk.loop = true;
        }
    }

    // Generate robots for this task
    public override void GenerateRobots()
    {
        robots = SpawnGameObjectArray(robotSpawnArray);
        robot = robots[0];

        // Record data
        stateReader = robot.GetComponentInChildren<StateReader>();
        laser = robot.GetComponentInChildren<LaserSocial>();
        collisionReader = robot.GetComponentInChildren<CollisionReader>();
        collisionStorageIndex = 0;
        
        // Initialization
        InvokeRepeating("UpdateData", 1f, 1/updateRate);

        // GUI set output
        Camera[] cameras = robot.GetComponentsInChildren<Camera>();
        Debug.Log(robot.transform.position);
        Debug.Log(cameras[0].transform.position);
        uI.GUI.SetCamera(cameras[0]);
    }

    // Customized data to record
    void UpdateData()
    {
        if (robot == null)
            return;
    
        valueToRecord = new float[18];
        // Record state
        // t
        valueToRecord[0] = Time.time;
        // pose
        valueToRecord[1] = stateReader.position[2];
        valueToRecord[2] = -stateReader.position[0];
        valueToRecord[3] = ToFLUEuler(stateReader.eulerRotation[1] * Mathf.Deg2Rad);
        // vel
        valueToRecord[4] = stateReader.linearVelocity[2];
        valueToRecord[5] = -stateReader.angularVelocity[1];
        // obs dis
        int obsMinI = GetLaserMinIndex(laser.obstacleRanges);
        int humMinI = GetLaserMinIndex(laser.humanRanges);
        valueToRecord[6] = laser.obstacleRanges[obsMinI];
        valueToRecord[7] = laser.directions[obsMinI];
        valueToRecord[8] = laser.humanRanges[humMinI];
        valueToRecord[9] = laser.directions[humMinI];
        // main camera joint
        valueToRecord[10] = stateReader.positions[2];
        valueToRecord[11] = ToFLUEuler(stateReader.positions[3]);
        valueToRecord[12] = stateReader.velocities[2];
        valueToRecord[13] = -stateReader.velocities[3];
        // arm camera joint
        valueToRecord[14] = stateReader.positions[22];
        valueToRecord[15] = ToFLUEuler(stateReader.positions[21]);
        valueToRecord[16] = stateReader.velocities[22];
        valueToRecord[17] = -stateReader.velocities[21];

        // Record collision
        stringToRecord = null; // don't record when there is no new collision
        if (collisionStorageIndex != collisionReader.storageIndex)
        {
            stringToRecord = new string[5];
            // collision
            stringToRecord[0] = string.Format("{0:0.000}", valueToRecord[0]);
            stringToRecord[1] = string.Format("{0:0.000}", valueToRecord[1]);
            stringToRecord[2] = string.Format("{0:0.000}", valueToRecord[2]);
            stringToRecord[3] = collisionReader.collisionSelfNames[collisionStorageIndex];
            stringToRecord[4] = collisionReader.collisionOtherNames[collisionStorageIndex];

            collisionStorageIndex = (collisionStorageIndex+1) % collisionReader.storageLength;
        }
    }

    private float ToFLUEuler(float angle)
    {
        float twoPI = 2 * Mathf.PI;

        // Change direction
        angle = twoPI - angle;
        // Wrap to [-pi to pi]
        angle =  angle % twoPI; 
        // positive remainder, 0 <= angle < 2pi  
        angle = (angle + twoPI) % twoPI;
        
        return angle;
    }

    private int GetLaserMinIndex(float[] ranges)
    {
        if (ranges.Length == 0) 
            return 0;

        // Get smallest index of laser scan
        float minValue = ranges.Min();
        int minIndex = ranges.ToList().IndexOf(minValue);
        return minIndex;
    }
}