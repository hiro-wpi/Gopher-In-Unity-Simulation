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
    private string[] taskNames;
    private string[] levelNames;

    // Task specific
    private GameObject tasksObject;
    // robot
    public GameObject robot;
    private Vector3[] spawnPositions;
    private Vector3[] spawnRotations;
    private Vector3[,] goalPositions;
    private Vector3[,] wayPointGoalPositions;
    // object
    public GameObject[] levelObjects;
    public GameObject goalObject;
    // human
    public GameObject nurse;
    private Vector3[] humanSpawnPositions;
    private Vector3[] humanSpawnRotations;
    private Vector3[,] humanTrajectories;

    void Start()
    {
        // General
        sceneName = "Hospital"; // use the same scene for all tasks
        taskNames = new string[] {"Corridor", "Turning", "Door", "Waypoints"};
        levelNames = new string[] {"Level1", "Level2"};

        // Robot spawn pose and goal
        spawnPositions = new Vector3[]
                        {
                            new Vector3(6.7f, 0.0f, -11.0f), 
                            new Vector3(-16.6f, 0.0f, -12.5f), 
                            new Vector3(-6.7f, 0.0f, -11.5f), 
                            new Vector3(-3.0f, 0.0f, -16.5f)
                        };
        spawnRotations = new Vector3[]
                        {
                            new Vector3(0f, -90f, 0f), 
                            new Vector3(0f, 90f, 0f), 
                            new Vector3(0f, 180f, 0f), 
                            new Vector3(0f, -90f, 0f)
                        };
        goalPositions = new Vector3[,]
                        {
                            {new Vector3(-16.0f, 0.0f, -12.0f)}, 
                            {new Vector3(-7.0f, 0.0f, -6.0f)}, 
                            {new Vector3(-8.0f, 0.0f, -20.0f)}
                        };
        wayPointGoalPositions = new Vector3[,]
                        {
                            {new Vector3(-7.5f, 0.0f, -16.5f), new Vector3(-7.5f, 0.0f, -11.5f), 
                             new Vector3(7.5f, 0.0f, -11.5f), new Vector3(7.5f, 0.0f, -20.0f)}
                        };

        // Human spawn position and trajectories
        humanSpawnPositions = new Vector3[]
                        {
                            new Vector3(-7f, 0f, -11.5f), 
                            new Vector3(-7.4f, 0f, -7f),
                            new Vector3(-10f, 0f, -16.4f)
                        };
        humanSpawnRotations = new Vector3[]
                        {
                            new Vector3(0f, 90f, 0f), 
                            new Vector3(0f, 180f, 0f), 
                            new Vector3(0f, 90f, 0f)
                        };
        humanTrajectories = new Vector3[,]
                        {
                            {new Vector3(-7.0f, 0f, -11.5f), new Vector3(-1.0f, 0f, -11.5f), 
                             new Vector3(-1.3f, 0f, -7.8f), new Vector3(-1.0f, 0f, -11.5f)},
                            {new Vector3(-7.4f, 0f, -7f), new Vector3(-7.4f, 0f, -10f), 
                             new Vector3(-7.4f, 0f, -15f), new Vector3(-7.4f, 0f, -10f)}, 
                            {new Vector3(-10f, 0f, -16.4f), new Vector3(-7.4f, 0f, -16.4f), 
                             new Vector3(-7.4f, 0f, -20.0f), new Vector3(-7.4f, 0f, -16.4f)}
                        };

        // Create a game object container
        tasksObject = new GameObject("Tasks");
        tasksObject.transform.SetParent(this.transform);

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
        GameObject taskObject = new GameObject("Level"+levelIndex.ToString()+" Task"+taskIndex.ToString());
        taskObject.transform.parent = tasksObject.transform;
        NavigationTask task = taskObject.AddComponent<NavigationTask>();
        
        // General
        task.TaskName = levelNames[levelIndex] + "-" + taskNames[taskIndex];
        task.SceneName = sceneName;

        // Interface -> all using the same
        // TODO give different interfaces to different tasks 
        UserInterface uI = taskObject.AddComponent<UserInterface>();
        GraphicalInterface gUI = taskObject.AddComponent<GraphicalInterface>();
        ControlInterface cUI = taskObject.AddComponent<ControlInterface>();
        uI.GUI = gUI;
        uI.CUI = cUI;
        task.UI = uI;

        // Convert array to spawn info
        // robot
        Task.SpawnInfo[] robotSpawnArray = new Task.SpawnInfo[1];
        robotSpawnArray[0] = ToSpawnInfo(robot, 
                                         spawnPositions[taskIndex], spawnRotations[taskIndex], 
                                         null);
        // human
        Task.SpawnInfo[] humanSpawnArray = new Task.SpawnInfo[0];
        if (levelIndex == 1)
        {
            humanSpawnArray = new Task.SpawnInfo[humanSpawnPositions.Length];
            for (int i=0; i<humanSpawnPositions.Length; ++i)
            {
                humanSpawnArray[i] = ToSpawnInfo(nurse, 
                                                 humanSpawnPositions[i], humanSpawnRotations[i], 
                                                 GetRow(humanTrajectories, i));
            }
        }
        // goals
        Vector3[] goals;
        if (taskIndex != 3)
            goals = GetRow(goalPositions, taskIndex);
        else
            goals = GetRow(wayPointGoalPositions, 0);
        // object
        Task.SpawnInfo[] objectSpawnArray = new Task.SpawnInfo[1 + goals.Length];
        objectSpawnArray[0] = ToSpawnInfo(levelObjects[levelIndex], 
                                          new Vector3(), new Vector3(), 
                                          null);
        if (taskIndex != 3)
            objectSpawnArray[1] = ToSpawnInfo(goalObject, 
                                              goals[0], new Vector3(), 
                                              null);
        else
            for (int g=0; g<goals.Length; ++g)
                objectSpawnArray[1+g] = ToSpawnInfo(goalObject, 
                                                    goals[g], new Vector3(), 
                                                    null);
        
        // Robot, object, and human spawn array
        task.SetRobotSpawnArray(robotSpawnArray);
        task.SetGoals(goals);
        task.SetObjectSpawnArray(objectSpawnArray);
        task.SetHumanSpawnArray(humanSpawnArray);
        
        return task;
    }

    // Utils
    private T[] GetRow<T>(T[,] Array2D, int index)
    {
        T[] row = new T[Array2D.GetLength(1)];
        for (int i = 0; i < row.Length; ++i)
            row[i] = Array2D[index, i];
        return row;
    }
}
