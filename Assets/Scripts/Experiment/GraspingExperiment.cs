using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining grasping experiment.
/// It includes four different grasping tasks and different difficulty levels
/// It generates and stores a list of tasks given task index and level index
/// </summary>
public class GraspingExperiment : Experiment 
{
    // General
    private int numTasks;
    private string sceneName;
    private string[] levelNames;
    private string[] taskNames;
    private string[] taskDescriptions;
    
    // Object
    // static object
    public GameObject[] levelObjects;
    // human
    public GameObject nurse;
    private Vector3[] humanSpawnPositions;
    private Vector3[] humanSpawnRotations;
    private Vector3[,] humanTrajectories;
    // robot
    public GameObject robot;
    private Vector3[] spawnPositions;
    private Vector3[] spawnRotations;

    // Task
    private GameObject tasksParentObject;
    public GameObject[] taskObjects;
    public GameObject[] goalObjects;
    private Vector3[,] goalPositions;
    private Vector3[] taskSpawnPositions;
    private Vector3[] taskSpawnRotations;

    // UI
    public GraphicalInterface gUI;
    public ControlInterface cUI;

    void Start()
    {
        // General
        sceneName = "Hospital"; // use the same scene for all tasks
        levelNames = new string[] {"Level1", "Level2", "Level3"};
        taskNames = new string[] {"ScanAndGrasp", "FindAndGrasp", "Carry", "Push"};
        taskDescriptions = new string[] {"Please scan and find the object with bar code 0104530 in Room S103, " + 
                                            "and put it on the medical tray in Room S103.", 
                                         "Please find the object with blue label in the Pharmacy, " +
                                            "and put it on the medical tray in the Pharmacy.", 
                                         "Please carry the IV pole to the Treatment Room 1.", 
                                         "Please push the medical cart to the Office."};
        
        // Robot spawn pose and goal
        spawnPositions = new Vector3[]
                        {
                            new Vector3( 7.0f, 0.0f, 10.5f),
                            new Vector3(-7.0f, 0.0f, 10.5f),
                            new Vector3( 7.5f, 0.0f, -1.0f),
                            new Vector3(-7.0f, 0.0f, -1.0f)
                        };
        spawnRotations = new Vector3[]
                        {
                            new Vector3(0f,   0f, 0f), 
                            new Vector3(0f,   0f, 0f), 
                            new Vector3(0f, -90f, 0f), 
                            new Vector3(0f,  90f, 0f)
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

        // Goals
        goalPositions = new Vector3[,]
                        {
                            {new Vector3(10.3f,  0.8f, 10.5f)}, 
                            {new Vector3(-5.8f,  0.9f, 11.2f)}, 
                            {new Vector3(-3.0f,  0.0f,  1.0f)},
                            {new Vector3( 0.0f,  0.0f, 11.0f)}
                        };
        
        // Task
        taskSpawnPositions = new Vector3[]
                        {
                            new Vector3(6.85f, 0.8f, 13.7f),
                            new Vector3(-8.9f, 1.2f, 13.5f),
                            new Vector3( 6.5f, 0.0f, -1.6f),
                            new Vector3(-6.5f, 0.0f, -1.0f)
                        };
        taskSpawnRotations = new Vector3[]
                        {
                            new Vector3(0f, -90f, 0f), 
                            new Vector3(0f,   0f, 0f), 
                            new Vector3(0f,   0f, 0f), 
                            new Vector3(0f,  90f, 0f)
                        };

        // Create a gameobject container
        tasksParentObject = new GameObject("Grasping Tasks");
        tasksParentObject.transform.SetParent(this.transform);
        // Tasks
        numTasks = taskNames.Length * levelNames.Length;
        tasks = new GraspingTask[numTasks];

        // Set up tasks
        int count = 0;
        for (int i=0; i<levelNames.Length; ++i)
        {
            for (int j=0; j<taskNames.Length; ++j)
            {
                tasks[count] = GenerateGraspingTask(i, j);
                count++;
            }
        }
    }

    // Generate specific navigation task given conditions
    private GraspingTask GenerateGraspingTask(int levelIndex, int taskIndex)
    {
        // Instantiate task
        GameObject taskObject = new GameObject(levelNames[levelIndex] + "-" +
                                               taskNames[taskIndex]);
        taskObject.transform.parent = tasksParentObject.transform;
        GraspingTask task = taskObject.AddComponent<GraspingTask>();
        
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
        Vector3[] goals = Utils.GetRow(goalPositions, taskIndex);
        Task.SpawnInfo[] goalSpawnArray = new Task.SpawnInfo[goals.Length];
        for (int i = 0; i < goals.Length; ++i)
        {
            goalSpawnArray[i] = Task.ToSpawnInfo(goalObjects[taskIndex], 
                                                 goals[i], new Vector3(), 
                                                 null);
        }

        // task object
        Task.SpawnInfo[] taskObjectSpawnArray = new Task.SpawnInfo[1];
        taskObjectSpawnArray[0] = Task.ToSpawnInfo(taskObjects[taskIndex], 
                                                   taskSpawnPositions[taskIndex], taskSpawnRotations[taskIndex], 
                                                   null);

        // robot, object, and human spawn array
        task.robotSpawnArray = robotSpawnArray;
        task.staticObjectSpawnArray = objectSpawnArray;
        task.dynamicObjectSpawnArray = humanSpawnArray;
        // task goal
        task.goalObjectSpawnArray = goalSpawnArray;
        task.taskObjectSpawnArray = taskObjectSpawnArray;

        return task;
    }
}
