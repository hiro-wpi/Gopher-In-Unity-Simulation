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
///     Task 2.1 Vital Sign Reading
///     Task 2.2 Bar Code Scanning
///     Task 2.3 Picking And Placing 
///     Task 2.4 Furniture Disinfecting
/// 
/// Tasks are given as a task set, which is the combination of one Type 1
/// task, and one Type 2 task. The detailed content of each task
/// is randomized. 
/// A secondary task, checking trash and dirty laundry containers, 
/// is also assigned for each task set.
///
/// </summary>
public class ComprehensiveExperiment : Experiment 
{
    // Tasks of a specific task sets (right, up, left, down)
    // Defined as (type.task-level)

    // pilot study
    private int numType = 2;
    private string[] rightType1 = new string[] {"1.4-2", "1.2-3", "1.3-3"};
    private string[] rightType2 = new string[] {"2.1-1", "2.3-1", "2.2-3"};
    private string[] upType1 = new string[] {"1.2-1", "1.1-1", "1.3-2"};
    private string[] upType2 = new string[] {"2.4-3", "2.3-2", "2.4-1"};
    private string[] leftType1 = new string[] {"1.4-1", "1.3-1", "1.1-2"};
    private string[] leftType2 = new string[] {"2.2-2", "2.3-3", "2.1-3"};
    private string[] downType1 = new string[] {"1.2-2", "1.1-3", "1.4-3"};
    private string[] downType2 = new string[] {"2.2-1", "2.4-3", "2.1-2"};

    // official study
    // TODO

    // Task Objects


    void Start()
    {
        // Verify task parameter is completed
        VerifyCompleteness(1, 4, 3);
        VerifyCompleteness(2, 4, 3);

        // General
        useSameScene = true;  // use the same scene for all tasks
        sceneNames = new string[] {"Hospital"};
        levelNames = new string[] {"Level1", "Level2", "Level3"};
        taskNames = new string[] {
                                    "Navigation", "ObjectCarrying", 
                                    "CartPushing", "BlockedPathPassing",

                                    "VitalReading", "CodeScanning", 
                                    "PickingAndPlacing", "FurnitureDisinfection"
                                 };
        taskDescriptions = new string[] {
                                        "Please navigate to", 
                                        "Please carry the IV pole to",
                                        "Please push the medical cart to",
                                        "Please navigate to",

                                        "Please read the vital value of",
                                        "Please scan the bar code of the medicine box",
                                        "Please pick up the medicine box and put it on the tray", 
                                        "Please disinfect the furniture."
                                        };
        
        // Robot spawn poses (select 1 from 4)
        robotSpawnPositions = new Vector3[] {new Vector3( -6.7f, 0.0f, -10.0f),
                                             new Vector3(  7.8f, 0.0f, -10.0f),
                                             new Vector3(  7.0f, 0.0f,  10.0f),
                                             new Vector3(-10.0f, 0.0f,   6.0f)};
        robotSpawnRotations = new Vector3[] {new Vector3(0f,  0f, 0f),
                                             new Vector3(0f,  0f, 0f),
                                             new Vector3(0f, -90f, 0f),
                                             new Vector3(0f,  90f, 0f)};
        
        // Human spawn position and trajectories
        dynamicObjectSpawnPositions = new Vector3[]
                            {
                                new Vector3(-6.7f, 0f, -8.0f),
                                new Vector3( 8.0f, 0f,  5.0f),
                                new Vector3( 3.0f, 0f,  7.5f),
                                new Vector3(-2.5f, 0f, -1.5f)
                            };
        dynamicObjectTrajectories = new Vector3[,]
                            {
                                {new Vector3(-7.5f, 0f,  7.0f), new Vector3( 7.5f, 0f,  7.5f), 
                                 new Vector3( 7.5f, 0f, -2.0f), new Vector3(-6.7f, 0f, -8.0f)},
                                {new Vector3( 7.5f, 0f, -2.0f), new Vector3(-7.0f, 0f, -2.0f), 
                                 new Vector3(-7.0f, 0f,  7.5f), new Vector3( 8.0f, 0f,  5.0f)},
                                {new Vector3( 0.5f, 0f,  7.0f), new Vector3( 1.0f, 0f, -1.5f), 
                                 new Vector3( 5.0f, 0f, -1.5f), new Vector3( 3.0f, 0f,  7.5f)}, 
                                {new Vector3(-4.0f, 0f,  7.5f), new Vector3( 0.5f, 0f,  6.5f), 
                                 new Vector3( 0.0f, 0f, -1.5f), new Vector3(-2.5f, 0f, -1.5f)}
                            };

        // Create a gameobject container
        GameObject tasksObject = new GameObject("Comprehensive Tasks");
        tasksObject.transform.SetParent(this.transform);
        tasks = new Task[taskNames.Length * levelNames.Length];

        // Set up tasks
        int count = 0;
        for (int i = 0; i < levelNames.Length; ++i)
        {
            for (int j = 0; j < taskNames.Length; ++j)
            {
                // TODO
                tasks[count] = GenerateComprehensiveTask(i, j, tasksObject);
                count++;
            }
        }
    }

    private Task GenerateComprehensiveTask(int levelIndex, int taskIndex, 
                                           GameObject parent = null)
    {
        // TEMP // General Info
        GameObject taskObject = tasks[taskIndex].gameObject;
        taskObject.transform.parent = parent.transform;
        Task task = tasks[taskIndex];
        
        // General
        task.sceneName = sceneNames[0];
        task.taskName = levelNames[levelIndex] + "-" + taskNames[taskIndex];
        task.taskDescription = taskDescriptions[taskIndex];

        // Detailed spawning info
        // robot
        Task.SpawnInfo[] robotSpawnArray = new Task.SpawnInfo[1];
        robotSpawnArray[0] = Task.ToSpawnInfo(robotPrefabs[0], 
                                              robotSpawnPositions[0], // temp
                                              robotSpawnRotations[0], 
                                              null);

        // static object
        Task.SpawnInfo[] staticObjectSpawnArray = new Task.SpawnInfo[1];
        staticObjectSpawnArray[0] = Task.ToSpawnInfo(staticObjects[levelIndex], 
                                                     new Vector3(), 
                                                     new Vector3(), 
                                                     null);

        // dynamic object - human
        Task.SpawnInfo[] humanSpawnArray = new Task.SpawnInfo[0];
        if (levelNames[levelIndex] == "Level2")
        {
            int spawnLength = (int) dynamicObjectSpawnPositions.Length / 2;
            humanSpawnArray = new Task.SpawnInfo[spawnLength];
            for (int i = 0; i < spawnLength; ++i)
            {
                humanSpawnArray[i] = Task.ToSpawnInfo(dynamicObjects[0], 
                                                      dynamicObjectSpawnPositions[i], 
                                                      new Vector3(), 
                                                      Utils.GetRow(dynamicObjectTrajectories, i));
            }
        }
        else if (levelNames[levelIndex] == "Level3")
        {
            humanSpawnArray = new Task.SpawnInfo[dynamicObjectSpawnPositions.Length];
            for (int i = 0; i < dynamicObjectSpawnPositions.Length; ++i)
            {
                humanSpawnArray[i] = Task.ToSpawnInfo(dynamicObjects[0], 
                                                      dynamicObjectSpawnPositions[i],
                                                      new Vector3(), 
                                                      Utils.GetRow(dynamicObjectTrajectories, i));
            }
        }

        // robot, object, and human spawn array
        task.robotSpawnArray = robotSpawnArray;
        task.staticObjectSpawnArray = staticObjectSpawnArray;
        task.dynamicObjectSpawnArray = humanSpawnArray;

        // Interface -> all using the same
        task.gUI = graphicalInterfaces[0];
        // TODO task.CUI = controlInterfaces[0];

        return task;
    }


    // Utils
    private void VerifyCompleteness(int type, int numTask, int numLevel)
    {
        // Combine tasks from each part
        string [] combination = new string[0];
        int [,] counter = new int[numTask, numLevel];
        if (type == 1)
        {
            combination = Utils.ConcatenateArray<string>(rightType1,
                          Utils.ConcatenateArray<string>(upType1,
                          Utils.ConcatenateArray<string>(leftType1, downType1)));
        }
        else if (type == 2)
        {
            combination = Utils.ConcatenateArray<string>(rightType2,
                          Utils.ConcatenateArray<string>(upType2,
                          Utils.ConcatenateArray<string>(leftType2, downType2)));
        }   

        // Verify completeness
        foreach (string task in combination)
        {
            string[] temp1 = task.Split(char.Parse(","));
            string[] temp2 = temp1[1].Split(char.Parse("-"));
            int taskType = int.Parse(temp1[0]);
            int taskIndex = int.Parse(temp2[0]);
            int level = int.Parse(temp2[1]);

            counter[taskIndex-1, level-1]++;
            if (taskType != type)
                Debug.LogWarning("Wrong type of task defined: " + 
                                 taskType+"."+taskIndex+"-"+level);
        }
        
        for (int i = 0; i < numTask; ++i)
            for (int j = 0; j < numLevel; ++j)
                if (counter[i, j] == 0)
                    Debug.LogWarning("Task is missing: " + 
                                     type+"."+(i+1)+"-"+(j+1));
    }
}