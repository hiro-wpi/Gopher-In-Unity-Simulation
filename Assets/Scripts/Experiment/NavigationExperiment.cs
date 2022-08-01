using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining navigation experiment.
/// It includes four different navigation tasks and two difficulty levels
/// It generates and stores a list of tasks given task index and level index
/// </summary>
public class NavigationExperiment : Experiment 
{
    // General
    private int numTasks;
    private string sceneName;
    private string[] levelNames;
    private string[] taskNames;
    private string[] taskDescriptions;
    
    // Robot
    public GameObject robot;
    private Vector3[] spawnPositions;
    private Vector3[] spawnRotations;
    // object
    public GameObject[] levelObjects;
    public GameObject goalObject;
    // human
    public GameObject nurse;
    private Vector3[] humanSpawnPositions;
    private Vector3[,] humanTrajectories;
    // Task
    private GameObject tasksParentObject;
    private Vector3[,] goalPositions;
    private Vector3[,] wayPointGoalPositions;

    // UI
    public GraphicalInterface gUI;
    public ControlInterface cUI;

    void Start()
    {
        // General
        sceneName = "Hospital"; // use the same scene for all tasks
        levelNames = new string[] {"Level1", "Level2", "Level3"};
        taskNames = new string[] {"Corridor", "Turning", "Door", "Waypoints"};
        taskDescriptions = new string[] {"Please go along the corridor and reach the goal.", 
                                         "Please go along the corridor and turn at the next intersection.", 
                                         "Please go through the right door and reach the goal.", 
                                         "Please follow the waypoints."};
        
        // Robot spawn pose and goal
        spawnPositions = new Vector3[]
                        {
                            new Vector3( 8.0f, 0.0f, -1.0f),
                            new Vector3(-7.0f, 0.0f, -1.0f),
                            new Vector3(-6.5f, 0.0f, -1.0f),
                            new Vector3( 4.4f, 0.0f, -5.5f)
                        };
        spawnRotations = new Vector3[]
                        {
                            new Vector3(0f, -90f, 0f), 
                            new Vector3(0f,  90f, 0f), 
                            new Vector3(0f, 180f, 0f), 
                            new Vector3(0f,  90f, 0f)
                        };

        // Goals
        goalPositions = new Vector3[,]
                        {
                            {new Vector3(-7.0f,  0.0f, -3.0f)}, 
                            {new Vector3( 0.5f,  0.0f,  6.5f)}, 
                            {new Vector3(-10.0f, 0.0f, -5.0f)}
                        };
        wayPointGoalPositions = new Vector3[,]
                        {
                            {new Vector3(7.7f, 0.0f, -4.5f), new Vector3( 7.7f, 0.0f, 7.5f), 
                             new Vector3(-7.0f, 0.0f, 8.5f), new Vector3(-7.0f, 0.0f, 13.0f)}
                        };

        // Human spawn position and trajectories
        humanSpawnPositions = new Vector3[]
                        {
                            new Vector3(-6.5f, 0f, -11.0f), 
                            new Vector3( 0.5f, 0f,  7.5f),
                            new Vector3( 7.5f, 0f,  7.5f)
                        };
        humanTrajectories = new Vector3[,]
                        {
                            {new Vector3(-7.0f, 0f, -1.5f), new Vector3(-7.0f, 0f,  7.5f), 
                             new Vector3( 0.5f, 0f,  7.5f), new Vector3( 0.5f, 0f, -2.0f)},
                            {new Vector3( 0.5f, 0f, -1.5f), new Vector3( 5.0f, 0f, -1.5f), 
                             new Vector3( 3.0f, 0f,  7.5f), new Vector3( 0.5f, 0f,  7.5f)}, 
                            {new Vector3( 7.5f, 0f, -1.5f), new Vector3(-6.5f, 0f, -1.5f), 
                             new Vector3(-6.5f, 0f,  7.5f), new Vector3( 7.5f, 0f,  7.5f)}
                        };

        // Create a gameobject container
        tasksParentObject = new GameObject("Navigation Tasks");
        tasksParentObject.transform.SetParent(this.transform);
        // Tasks
        numTasks = taskNames.Length * levelNames.Length;
        tasks = new NavigationTask[numTasks];

        // Set up tasks
        int count = 0;
        for (int i=0; i<levelNames.Length; ++i)
        {
            for (int j=0; j<taskNames.Length; ++j)
            {
                tasks[count] = GenerateNavigationTask(i, j);
                count++;
            }
        }
    }

    // Generate specific navigation task given conditions
    private NavigationTask GenerateNavigationTask(int levelIndex, int taskIndex)
    {
        // Instantiate task
        GameObject taskObject = new GameObject(levelNames[levelIndex] + "-" +
                                               taskNames[taskIndex]);
        taskObject.transform.parent = tasksParentObject.transform;
        NavigationTask task = taskObject.AddComponent<NavigationTask>();
        
        // General
        task.sceneName = sceneName;
        task.taskName = levelNames[levelIndex] + "-" + taskNames[taskIndex];
        task.taskDescription = taskDescriptions[taskIndex];

        // Interface -> all using the same
        task.gUI = gUI;
        // TODO task.CUI = cUI;

        // Convert array to spawn info
        // robot
        Task.SpawnInfo[] robotSpawnArray = new Task.SpawnInfo[1];
        robotSpawnArray[0] = Task.ToSpawnInfo(robot, 
                                              spawnPositions[taskIndex], spawnRotations[taskIndex], 
                                              null);
        
        // static object
        Task.SpawnInfo[] objectSpawnArray = new Task.SpawnInfo[1];
        objectSpawnArray[0] = Task.ToSpawnInfo(levelObjects[levelIndex], 
                                               new Vector3(), new Vector3(), 
                                               null);
        
        // dynamic object - human
        Task.SpawnInfo[] humanSpawnArray = new Task.SpawnInfo[0];
        if (levelNames[levelIndex] == "Level2")
        {
            int spawnLength = (int) humanSpawnPositions.Length / 2;
            humanSpawnArray = new Task.SpawnInfo[spawnLength];
            for (int i = 0; i < spawnLength; ++i)
            {
                humanSpawnArray[i] = Task.ToSpawnInfo(nurse, 
                                                      humanSpawnPositions[i], new Vector3(), 
                                                      Utils.GetRow(humanTrajectories, i));
            }
        }
        else if (levelNames[levelIndex] == "Level3")
        {
            humanSpawnArray = new Task.SpawnInfo[humanSpawnPositions.Length];
            for (int i = 0; i < humanSpawnPositions.Length; ++i)
            {
                humanSpawnArray[i] = Task.ToSpawnInfo(nurse, 
                                                      humanSpawnPositions[i], new Vector3(), 
                                                      Utils.GetRow(humanTrajectories, i));
            }
        }

        // goals
        Vector3[] goals;
        if (taskNames[taskIndex] != "Waypoints")
            goals = Utils.GetRow(goalPositions, taskIndex);
        else
            goals = Utils.GetRow(wayPointGoalPositions, 0);
        Task.SpawnInfo[] goalSpawnArray = new Task.SpawnInfo[goals.Length];
        for (int i = 0; i < goals.Length; ++i)
        {
            goalSpawnArray[i] = Task.ToSpawnInfo(goalObject, 
                                                 goals[i], new Vector3(), 
                                                 null);
        }

        // robot, object, and human spawn array
        task.robotSpawnArray = robotSpawnArray;
        task.staticObjectSpawnArray = objectSpawnArray;
        task.dynamicObjectSpawnArray = humanSpawnArray;
        // task goal
        task.goalObjectSpawnArray = goalSpawnArray;

        return task;
    }
}
