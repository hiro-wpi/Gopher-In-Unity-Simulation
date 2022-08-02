using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining loco-motion and loco-manipulation experiment.
///
/// There are two types of tasks defined
/// Type 1 - Facilitate navigation tasks by moving manipulater
///     Task 1.1 Navigation
///     Task 1.2 Object Carrying
///     Task 1.3 Cart Pushing
///     Task 1.4 Blocked Path Passing
/// Type 2 - Facilitate end-effector control tasks by moving the base
///     Task 2.1 Read Vital Sign
///     Task 2.2 Bar Code Scanning
///     Task 2.3 Pick And Place 
///     Task 2.4 Furniture Disinfecting
/// 
/// Tasks are given as a task set, which is the combination of one Type 1
/// task, and one to two Type 2 tasks. The detailed content of each task
/// is randomized. A secondary task, checking trash and dirty laundry containers, 
/// is also assigned for each task set.
///
/// </summary>
public class LocomotionExperiment : Experiment 
{
    // General
    private string sceneName;
    private string[] levelNames;
    private string[] taskNames;
    private string[] taskDescriptions;
    private int numTasks;
    
    // Task specific
    private GameObject tasksObject;
    // robot
    public GameObject robot;
    private Vector3[] spawnPositions;
    private Vector3[] spawnRotations;
    // object
    public GameObject[] levelObjects;
    // temp
    public GameObject goalObjectNav;
    public GameObject goalObjectMan;
    // human
    public GameObject nurse;
    private Vector3[] humanSpawnPositions;
    private Vector3[] humanSpawnRotations;
    private Vector3[,] humanTrajectories;

    // UI
    public GraphicalInterface gUI;
    public ControlInterface cUI;

    void Start()
    {
        // TEMP //

        // General
        sceneName = "Hospital"; // use the same scene for all tasks
        // levelNames = new string[] {"Level1", "Level2"};

        // TEMP testing
        levelNames = new string[] {"Level2", "Level3"};
        taskNames = new string[] {"Navigation1", "ScanGrasp",
                                  "Navigation2", "LocateGrasp", "Carrying1",
                                  "Carrying2", "Reading", 
                                  "Navigation3"
                                 };
        // taskNames = new string[] {"GoHome", "Carrying", "Pushing", "LocalGrasping", "Navigation"};
        taskDescriptions = new string[] {"Please navigate to room S103.", 
                                         "Please scan the medicine in room S103 and find the medicine with code 0104530, " +
                                         "grasp and put it on the medical cart outside.",
                                         
                                         "Please navigate to the Pharmacy",
                                         "Please pick up one medicine with blue label in the medicine carbinet in the Pharmacy, " + 
                                         "and put it on the table.",
                                         "Please push the medical cart outside to treatment room 1.",
                                         
                                         "Please carry the IV pole outside to Room L101.",
                                         "Please read the vital value of Bed 3 in Room L101.", 
                                         "Please navigate back to the nurse station."
                                        };
        
        // Robot spawn pose and goal
        spawnPositions = new Vector3[] {new Vector3(11.0f, 0.0f, 2.0f)};
        spawnRotations = new Vector3[] {new Vector3(0f, -90f, 0f)};
                        
        // Human spawn position and trajectories
        humanSpawnPositions = new Vector3[]
                        {
                            new Vector3(-10.0f, 0f, -6.0f),
                            new Vector3( 11.0f, 0f,  6.0f),
                            new Vector3(  3.0f, 0f,  7.5f),
                            new Vector3( -2.5f, 0f, -1.5f)
                        };
        humanTrajectories = new Vector3[,]
                        {
                            {new Vector3(-7.5f, 0f,  7.0f), new Vector3(  7.5f, 0f,  7.5f), 
                             new Vector3( 7.5f, 0f, -2.0f), new Vector3(-10.0f, 0f, -6.0f)},
                            {new Vector3( 7.5f, 0f, -2.0f), new Vector3( -7.0f, 0f, -2.0f), 
                             new Vector3(-7.0f, 0f,  7.5f), new Vector3( 11.0f, 0f,  6.0f)},
                            {new Vector3( 0.5f, 0f,  7.0f), new Vector3(  1.0f, 0f, -1.5f), 
                             new Vector3( 5.0f, 0f, -1.5f), new Vector3(  3.0f, 0f,  7.5f)}, 
                            {new Vector3(-4.0f, 0f,  7.5f), new Vector3(  0.5f, 0f,  6.5f), 
                             new Vector3( 0.0f, 0f, -1.5f), new Vector3( -2.5f, 0f, -1.5f)}
                        };

        // Create a gameobject container
        tasksObject = new GameObject("Locomotion Tasks");
        tasksObject.transform.SetParent(this.transform);

        // TEMP Tasks
        numTasks = tasks.Length;
        //numTasks = taskNames.Length * levelNames.Length;
        //tasks = new NavigationTask[numTasks];

        // Set up tasks
        int count = 0;
        for (int i=0; i<levelNames.Length; ++i)
        {
            for (int j=0; j<taskNames.Length; ++j)
            {
                tasks[count] = GenerateLocomotionTask(i, j);
                count++;
            }
        }
    }

    // Generate specific navigation task given conditions
    private Task GenerateLocomotionTask(int levelIndex, int taskIndex)
    {
        // TEMP //
        GameObject taskObject = tasks[taskIndex].gameObject;
        taskObject.transform.parent = tasksObject.transform;
        Task task = tasks[taskIndex];
        
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
                                              spawnPositions[0], spawnRotations[0], 
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

        // robot, object, and human spawn array
        task.robotSpawnArray = robotSpawnArray;
        task.staticObjectSpawnArray = objectSpawnArray;
        task.dynamicObjectSpawnArray = humanSpawnArray;

        return task;
    }
}