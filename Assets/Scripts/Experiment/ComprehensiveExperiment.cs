using System.Linq;
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
    // Tasks of a specific task type, defined as (type.task-level)
    // (right, up, left, down)
    //  ←——
    // ↓   ↑
    //  ——→
    private int numType = 2;
    private int numTask = 4;
    
    // pilot study
    /*
    private int numLevel = 3;
    private int numStep = 4; // step per circle
    private int numCircle = 3;
    // TODO
    private string[,] tasksType1 = new string[,] {
                                                    {"1.4-2", "1.2-3", "1.3-3"},
                                                    {"1.2-1", "1.1-1", "1.3-2"},
                                                    {"1.4-1", "1.3-1", "1.1-2"},
                                                    {"1.2-2", "1.1-3", "1.4-3"}
                                                 };
    private string[,] tasksType2 = new string[,] {
                                                    {"2.1-1", "2.3-1", "2.2-3"},
                                                    {"2.4-3", "2.3-2", "2.4-1"},
                                                    {"2.2-2", "2.3-3", "2.1-3"},
                                                    {"2.2-1", "2.4-3", "2.1-2"}
                                                 };
    */

    // official study
    private int numLevel = 3;
    private int numStep = 4; // step per circle
    private int numCircle = 3;
    private string[,] tasksType1 = new string[,] {
                                                    {"1.4-2", "1.2-3", "1.3-3"},
                                                    {"1.2-1", "1.1-1", "1.3-2"},
                                                    {"1.4-1", "1.3-1", "1.1-2"},
                                                    {"1.2-2", "1.1-3", "1.4-3"}
                                                 };
    private string[,] tasksType2 = new string[,] {
                                                    {"2.1-1", "2.3-1", "2.2-3"},
                                                    {"2.4-2", "2.3-2", "2.4-1"},
                                                    {"2.2-2", "2.3-3", "2.1-3"},
                                                    {"2.2-1", "2.4-3", "2.1-2"}
                                                 };
    // randomization
    private System.Random random = new System.Random(0);
    private int[,] indexArrayType1 = new int[4, 3];
    private int[,] indexArrayType2 = new int[4, 3];

    // Task Objects


    void Start()
    {
        // Verify task parameters completeness
        VerifyCompleteness();
        // Randomize index array
        int[] temp = new int[] {0, 1, 2};
        for (int i = 0; i < indexArrayType1.GetLength(0); ++i)
            for (int j = 0; j < indexArrayType1.GetLength(1); ++j)
            {
                indexArrayType1[i, j] = temp[j];
                indexArrayType2[i, j] = temp[j];
            }
        indexArrayType1 = Utils.RandomizeRow<int>(random, indexArrayType1);
        indexArrayType2 = Utils.RandomizeRow<int>(random, indexArrayType2);
        // get final task order
        int startIndex = Utils.Shuffle<int>(random, new int[] {0, 1, 2, 3})[0];
        string[] randomizedTasks = GetTaskOrder(startIndex);

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

                                        "Please read the vital value of the highlighted monitor.",
                                        "Please scan the bar code of the highlighted medicine box.",
                                        "Please pick up the highlighted medicine box and put it on the tray.", 
                                        "Please disinfect the highlighted furniture."
                                        };
        
        // Robot spawn poses (select 1 from 4)
        robotSpawnPositions = new Vector3[] {new Vector3( -6.7f, 0.0f, -11.0f),
                                             new Vector3(  7.8f, 0.0f, -11.0f),
                                             new Vector3(  7.0f, 0.0f,  11.0f),
                                             new Vector3(-11.0f, 0.0f,   6.0f)};
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
        tasks = new Task[randomizedTasks.Length];

        // Set up tasks
        string currentRoomName = HospitalMapUtil.GetLocationName(robotSpawnPositions[startIndex]);
        string nextRoonName = "";
        for (int i = 0; i < randomizedTasks.Length; ++i)
        {
            // Get task info
            string[] temp1 = randomizedTasks[i].Split(char.Parse("."));
            string[] temp2 = temp1[1].Split(char.Parse("-"));
            int taskType = int.Parse(temp1[0]);
            int taskIndex = int.Parse(temp2[0]) - 1;
            int levelIndex = int.Parse(temp2[1]) - 1;
            
            // Get next room name
            int stepIndex = (i + startIndex) % numStep;
            if (taskType == 1) // going to antoher room
            {
                int nextTaskIndex = int.Parse( randomizedTasks[i+1].Split(
                                               char.Parse("."))[1].Split(
                                               char.Parse("-"))[0] ) - 1;
                nextRoonName = GetNextRoom(stepIndex, nextTaskIndex);
            }
            else if (taskType == 2) // stay in the same room
                nextRoonName = currentRoomName;
            // Generate task
            tasks[i] = GenerateComprehensiveTask(taskType, taskIndex, levelIndex, stepIndex,
                                                 currentRoomName, nextRoonName, 
                                                 tasksObject);
            currentRoomName = nextRoonName;
        }
    }
    private string GetNextRoom(int stepIndex, int type2TaskIndex)
    {
        string[] roomNames = new string[0];
        if (stepIndex == 0) 
        {
            roomNames = new string[] {"Room P103", "Room P104", "Room S102"};
            if (type2TaskIndex == 0) 
            {
                
            }
            else if (type2TaskIndex == 1)
            {
                
            }
            else if (type2TaskIndex == 2)
            {
                
            }
            else if (type2TaskIndex == 3)
            {
                
            }
        }
        else if (stepIndex == 1)
        {
            roomNames = new string[] {"Pharmacy", "Room L101"};
        }
        else if (stepIndex == 2)
        {
            roomNames = new string[] {"Room P105", "Room S103"};
        }
        else if (stepIndex == 3)
        {
            roomNames = new string[] {"Room P101", "Room P102", "Room S101"};
        }
        // randomly select one
        string roomName = Utils.Shuffle<string>(random, roomNames)[0];
        return roomName;
    }

    private Task GenerateComprehensiveTask(int taskType, int taskIndex, int levelIndex, 
                                           int stepIndex,
                                           string currentRoomName, string nextRoonName, 
                                           GameObject parent = null)
    {
        // Instantiate task object
        GameObject taskObject = new GameObject(taskType+"."+(taskIndex+1)+"-"+(levelIndex+1));
        taskObject.transform.parent = parent.transform;
        // Instantiate task
        Task task = null;
        if (taskType == 1)
        {
            if (taskIndex == 0)
                task = taskObject.AddComponent<NavigationTask>();
            else if (taskIndex == 1)
                task = taskObject.AddComponent<GraspingTask>();
            else if (taskIndex == 2)
                task = taskObject.AddComponent<GraspingTask>();
            else if (taskIndex == 3)
                task = taskObject.AddComponent<NavigationTask>();
        }
        else if (taskType == 2)
        {
            if (taskIndex == 0)
                task = taskObject.AddComponent<ReadingTask>();
            else if (taskIndex == 1)
                task = taskObject.AddComponent<ReadingTask>();
            else if (taskIndex == 2)
                task = taskObject.AddComponent<GraspingTask>();
            else if (taskIndex == 3)
                task = taskObject.AddComponent<PaintingTask>();
        }

        // General
        task.sceneName = sceneNames[0];
        task.taskName = taskType+"."+(taskIndex+1)+"-"+(levelIndex+1);
        if (taskType == 1)
            task.taskDescription = taskDescriptions[(taskType-1)*4 + taskIndex] + nextRoonName;
        else if (taskType == 2)
            task.taskDescription = taskDescriptions[(taskType-1)*4 + taskIndex] + currentRoomName;

        // Detailed spawning info
        // robot
        Task.SpawnInfo[] robotSpawnArray = new Task.SpawnInfo[1];
        robotSpawnArray[0] = Task.ToSpawnInfo(robotPrefabs[0], 
                                              robotSpawnPositions[stepIndex],
                                              robotSpawnRotations[stepIndex], 
                                              null);

        // static object
        Task.SpawnInfo[] staticObjectSpawnArray = new Task.SpawnInfo[1];
        staticObjectSpawnArray[0] = Task.ToSpawnInfo(staticObjects[levelIndex], 
                                                     new Vector3(), new Vector3(), 
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

        // Task specific
        // task object 
        // goal object
        Task.SpawnInfo[] taskObjectSpawnArray = new Task.SpawnInfo[0];
        Task.SpawnInfo[] goalObjectSpawnArray = new Task.SpawnInfo[0];
        if (taskType == 1)
        {
            // Get entrance and passage from room names for type 1 tasks
            var(currEntrace, currEntraceDir) = HospitalMapUtil.GetRoomEntrance(currentRoomName);
            var(nextEntrace, nextEntraceDir) = HospitalMapUtil.GetRoomEntrance(nextRoonName);
            goalObjectSpawnArray = new Task.SpawnInfo[1];

            if (taskIndex == 0)
            {
                goalObjectSpawnArray[0] = Task.ToSpawnInfo(goalObjects[0], nextEntrace, 
                                                           new Vector3(), null);
            }
            else if (taskIndex == 1)
            {
                taskObjectSpawnArray = new Task.SpawnInfo[1];
                taskObjectSpawnArray[0] = Task.ToSpawnInfo(taskObjects[0], currEntrace,
                                                           currEntraceDir + new Vector3(0f, 180f, 0f), 
                                                           null);
                goalObjectSpawnArray[0] = Task.ToSpawnInfo(goalObjects[0], nextEntrace,
                                                           new Vector3(), null);
            }
            else if (taskIndex == 2)
            {
                taskObjectSpawnArray = new Task.SpawnInfo[1];
                taskObjectSpawnArray[0] = Task.ToSpawnInfo(taskObjects[1], currEntrace,
                                                           currEntraceDir + new Vector3(0f, 180f, 0f), 
                                                           null);
                goalObjectSpawnArray[0] = Task.ToSpawnInfo(goalObjects[0], nextEntrace,
                                                           new Vector3(), null);
            }
            else //else if (taskIndex == 3)
            {
                var(blockedPoss, blockedRots) = HospitalMapUtil.GetRoomPassage(nextRoonName);
                taskObjectSpawnArray = new Task.SpawnInfo[blockedPoss.Length];
                for (int i = 0; i < blockedPoss.Length; ++i)
                    taskObjectSpawnArray[i] = Task.ToSpawnInfo(taskObjects[1], blockedPoss[i],
                                                               blockedRots[i], null);
                goalObjectSpawnArray[0] = Task.ToSpawnInfo(goalObjects[0], nextEntrace,
                                                           new Vector3(), null);
            }
        }
        else //else if (taskType == 2)
        {
            if (taskIndex == 0)
            {
                
                task = taskObject.AddComponent<ReadingTask>();
            }
            else if (taskIndex == 1)
            {

                task = taskObject.AddComponent<ReadingTask>();
            }
            else if (taskIndex == 2)
            {

                task = taskObject.AddComponent<GraspingTask>();
            }
            else //else if (taskIndex == 3)
            {
                
                task = taskObject.AddComponent<PaintingTask>();
            }
        }

        // robot, object, and human spawn array
        task.robotSpawnArray = robotSpawnArray;
        task.staticObjectSpawnArray = staticObjectSpawnArray;
        task.dynamicObjectSpawnArray = humanSpawnArray;
        task.taskObjectSpawnArray = taskObjectSpawnArray;
        task.goalObjectSpawnArray = goalObjectSpawnArray;

        // Interface -> all using the same
        task.gUI = graphicalInterfaces[0];
        // TODO task.CUI = controlInterfaces[0];

        return task;
    }


    // Utils
    private void VerifyCompleteness()
    {
        // Combine tasks from each part
        string [] combination = new string[0];
        combination = Utils.ConcatenateArray<string>(Utils.GetRow<string>(tasksType1, 0),
                      Utils.ConcatenateArray<string>(Utils.GetRow<string>(tasksType1, 1),
                      Utils.ConcatenateArray<string>(Utils.GetRow<string>(tasksType1, 2), 
                                                     Utils.GetRow<string>(tasksType1, 3))));
        VerifyTypeCompleteness(combination, 1);
        combination = Utils.ConcatenateArray<string>(Utils.GetRow<string>(tasksType2, 0),
                      Utils.ConcatenateArray<string>(Utils.GetRow<string>(tasksType2, 1),
                      Utils.ConcatenateArray<string>(Utils.GetRow<string>(tasksType2, 2), 
                                                     Utils.GetRow<string>(tasksType2, 3))));
        VerifyTypeCompleteness(combination, 2);
    }
    private void VerifyTypeCompleteness(string[] tasks, int type)
    {
        // Verify task setting
        if (numCircle * numStep != numLevel * numTask)
            Debug.LogWarning("Task orders did not cover all the tasks");

        // Verify completeness
        int [,] counter = new int[numTask, numLevel];
        foreach (string task in tasks)
        {
            string[] temp1 = task.Split(char.Parse("."));
            string[] temp2 = temp1[1].Split(char.Parse("-"));
            int taskType = int.Parse(temp1[0]);
            int taskIndex = int.Parse(temp2[0]) - 1;
            int levelIndex = int.Parse(temp2[1]) - 1;

            counter[taskIndex, levelIndex]++;
            if (taskType != type)
                Debug.LogWarning("Wrong type of task defined: " + 
                                 taskType+"."+(taskIndex+1)+"-"+(levelIndex+1));
        }
        
        for (int i = 0; i < numTask; ++i)
            for (int j = 0; j < numLevel; ++j)
                if (counter[i, j] == 0)
                    Debug.LogWarning("Task is missing: " + 
                                     type+"."+(i+1)+"-"+(j+1));
    }

    private string[] GetTaskOrder(int startIndex)
    {
        string[] randomizedTasks = new string[numCircle * numStep * numType];

        int count = 0;
        for (int i = 0; i < numCircle; ++i)
        {
            for (int j = 0; j < numStep; ++j)
            {
                // change start step
                int newJ = (j + startIndex) % numStep; 
                // get the type 1 and type 2 tasks
                for (int k = 0; k < numType; ++k)
                {
                    if (k == 0)
                        randomizedTasks[count] = 
                            tasksType1[newJ, indexArrayType1[newJ, i]];
                    else if(k == 1)
                        randomizedTasks[count] = 
                            tasksType2[newJ, indexArrayType2[newJ, i]];
                    count ++;
                }
            }
        }
        return randomizedTasks;
    }
}