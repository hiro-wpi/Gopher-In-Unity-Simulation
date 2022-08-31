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
    // robot
    private string[] robotSpawnRooms;

    // more task objects
    // 1
    public GameObject[] movableObjects;
    // 2.1
    public GameObject[] monitorScreens;
    public string[] monitorValues;
    // 2.2
    public GameObject[] barcodeMedicines;
    public string[] barcodeResults;
    public GameObject[] barcodeMedicinesSpecial;
    public string[] barcodeResultsSpecial;
    // 2.3
    public GameObject[] medicines;
    public GameObject[] medicinesHorizontal;
    // 2.4
    public GameObject disinfectionPack;
    public GameObject[] disinfectionFurnitures;
    // secondary
    public GameObject[] trashCan;
    public GameObject[] laundryBasket;


    // Tasks of a specific task type, defined as (type.task-level)
    // (right, up, left, down)
    //  ←——
    // ↓   ↑
    //  ——→
    private int numType = 2;
    private int numTask = 4;
    private int numLevel = 3;
    
    // pilot study
    private int numStep = 4; // step per circle
    private int numCircle = 1;
    // TODO
    private string[,] tasksType1 = new string[,] {
                                                    {"1.1-2"},
                                                    {"1.4-2"},
                                                    {"1.2-2"},
                                                    {"1.3-2"}
                                                 };
    private string[,] tasksType2 = new string[,] {
                                                    {"2.4-2"},
                                                    {"2.2-2"},
                                                    {"2.1-2"},
                                                    {"2.3-2"}
                                                 };
    /*
    // official study
    private int numStep = 4; // step per circle
    private int numCircle = 3;
    private string[,] tasksType1 = new string[,] {
                                                    {"1.4-2", "1.2-3", "1.3-3"},
                                                    {"1.2-1", "1.1-1", "1.3-2"},
                                                    {"1.4-1", "1.3-1", "1.1-2"},
                                                    {"1.2-2", "1.1-3", "1.4-3"}
                                                 };
    private string[,] tasksType2 = new string[,] {
                                                    {"2.1-1", "2.3-1", "2.2-2"},
                                                    {"2.4-2", "2.3-2", "2.4-1"},
                                                    {"2.2-3", "2.3-3", "2.1-2"},
                                                    {"2.2-1", "2.4-3", "2.1-3"}
                                                 };
    */
    // randomization
    public int randomizationSeed = 0;
    private System.Random random;
    private int startIndex;
    private int[,] indexArrayType1;
    private int[,] indexArrayType2;


    void Start()
    {
        // Verify task parameters completeness
        VerifyCompleteness();
        // Init index array (task order)
        int[] temp = Enumerable.Range(0, numCircle).ToArray();
        indexArrayType1 = new int[numStep, numCircle];
        indexArrayType2 = new int[numStep, numCircle];
        for (int i = 0; i < indexArrayType1.GetLength(0); ++i)
            for (int j = 0; j < indexArrayType1.GetLength(1); ++j)
            {
                indexArrayType1[i, j] = temp[j];
                indexArrayType2[i, j] = temp[j];
            }
        // get randomized task order
        random = new System.Random(randomizationSeed);
        string[] randomizedTasks = GetRandomizedTaskOrder();

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
        string[] robotSpawnRooms = new string[] {"Room S101", "Room S102", "Room P105", "Room L101"};
        
        // Human spawn position and trajectories
        dynamicObjectSpawnPositions = new Vector3[]
                            {
                                new Vector3(-6.7f, 0f, -8.0f),
                                new Vector3( 8.0f, 0f,  5.0f),
                                new Vector3( 0.5f, 0f,  7.0f),
                                new Vector3(-2.5f, 0f, -1.5f)
                            };
        dynamicObjectTrajectories = new Vector3[,]
                            {
                                {new Vector3(-7.5f, 0f,  7.0f), new Vector3( 7.5f, 0f,  7.5f), 
                                 new Vector3( 7.5f, 0f, -2.0f), new Vector3(-6.7f, 0f, -8.0f)},
                                {new Vector3( 7.5f, 0f, -2.0f), new Vector3(-7.0f, 0f, -2.0f), 
                                 new Vector3(-7.0f, 0f,  7.5f), new Vector3( 8.0f, 0f,  5.0f)},
                                {new Vector3( 1.0f, 0f, -1.5f), new Vector3( 5.0f, 0f, -1.5f), 
                                 new Vector3( 3.0f, 0f,  7.5f), new Vector3( 0.5f, 0f,  7.0f)}, 
                                {new Vector3(-4.0f, 0f,  7.5f), new Vector3( 0.5f, 0f,  6.5f), 
                                 new Vector3( 0.0f, 0f, -1.5f), new Vector3(-2.5f, 0f, -1.5f)}
                            };

        // Create a gameobject container
        GameObject tasksObject = new GameObject("Comprehensive Tasks");
        tasksObject.transform.SetParent(this.transform);
        tasks = new Task[randomizedTasks.Length];

        // Set up tasks
        string currRoomName = robotSpawnRooms[startIndex];
        for (int i = 0; i < randomizedTasks.Length; i+=2)
        {
            int stepIndex = (i/2 + startIndex) % numStep;
            // Generate task set
            (tasks[i], tasks[i+1], currRoomName) = GenerateTaskSet(randomizedTasks[i], randomizedTasks[i+1],
                                                                   currRoomName, stepIndex, tasksObject);
        }
    }

    private (Task, Task, string) GenerateTaskSet(string task1Name, string task2Name, string currRoomName, 
                                                 int stepIndex, GameObject parent = null)
    {
        // Get task info
        var(taskType1, taskIndex1, levelIndex1) = GetIndicesFromName(task1Name);
        var(taskType2, taskIndex2, levelIndex2) = GetIndicesFromName(task2Name);    
        string nextRoomName = GetNextRoom(stepIndex, taskIndex2, levelIndex2);
        // Generate task set
        Task task1 = GenerateComprehensiveTask(taskType1, taskIndex1, levelIndex1, 
                                               currRoomName, nextRoomName, parent);
        Task task2 = GenerateComprehensiveTask(taskType2, taskIndex2, levelIndex2, 
                                               currRoomName, nextRoomName, parent);
        return (task1, task2, nextRoomName);
    }
    private Task GenerateComprehensiveTask(int taskType, int taskIndex, int levelIndex, 
                                           string currRoomName, string nextRoomName, 
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
            task.taskDescription = taskDescriptions[(taskType-1)*4 + taskIndex] 
                                   + " " + nextRoomName;
        else if (taskType == 2)
            task.taskDescription = taskDescriptions[(taskType-1)*4 + taskIndex];

        // Detailed spawning info
        // robot
        Task.SpawnInfo[] robotSpawnArray = new Task.SpawnInfo[1];
        var (entrancePos, entraceRot) = HospitalMapUtil.GetRoomEntrance(currRoomName);
        robotSpawnArray[0] = Task.ToSpawnInfo(robotPrefabs[0], 
                                              entrancePos + Quaternion.Euler(entraceRot) * Vector3.forward,
                                              entraceRot + new Vector3(0f, 180f, 0f), 
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

        // Task specific // task object and goal object
        Task.SpawnInfo[] taskObjectSpawnArray = new Task.SpawnInfo[0];
        Task.SpawnInfo[] goalObjectSpawnArray = new Task.SpawnInfo[0];
        if (taskType == 1)
        {
            // Get entrance and passage from room names for type 1 tasks
            var(currEntrace, currEntraceDir) = HospitalMapUtil.GetRoomEntrance(currRoomName);
            var(nextEntrace, nextEntraceDir) = HospitalMapUtil.GetRoomEntrance(nextRoomName);
            goalObjectSpawnArray = new Task.SpawnInfo[1];
            // 1.1 Navigation
            if (taskIndex == 0)
            {
                goalObjectSpawnArray[0] = Task.ToSpawnInfo(goalObjects[0], nextEntrace, 
                                                           new Vector3(), null);
            }
            // 1.2 Object Carrying
            else if (taskIndex == 1)
            {
                taskObjectSpawnArray = new Task.SpawnInfo[1];
                taskObjectSpawnArray[0] = Task.ToSpawnInfo(movableObjects[0], currEntrace,
                                                           currEntraceDir + new Vector3(0f, 180f, 0f), 
                                                           null);
                goalObjectSpawnArray[0] = Task.ToSpawnInfo(goalObjects[0], nextEntrace + 
                                                           Quaternion.Euler(nextEntraceDir) * Vector3.forward,
                                                           new Vector3(), null);
            }
            // 1.3 Cart Pushing
            else if (taskIndex == 2)
            {
                taskObjectSpawnArray = new Task.SpawnInfo[1];
                taskObjectSpawnArray[0] = Task.ToSpawnInfo(movableObjects[1], currEntrace,
                                                           currEntraceDir + new Vector3(0f, 180f, 0f), 
                                                           null);
                goalObjectSpawnArray[0] = Task.ToSpawnInfo(goalObjects[0], nextEntrace + 
                                                           Quaternion.Euler(nextEntraceDir) * Vector3.forward,
                                                           new Vector3(), null);
            }
            // 1.4 Blocked Path Passing
            else if (taskIndex == 3)
            {
                var(blockedPoss, blockedRots) = HospitalMapUtil.GetRoomPassage(nextRoomName);
                taskObjectSpawnArray = new Task.SpawnInfo[blockedPoss.Length];
                for (int i = 0; i < blockedPoss.Length; ++i)
                    taskObjectSpawnArray[i] = Task.ToSpawnInfo(movableObjects[1], blockedPoss[i],
                                                               blockedRots[i], null);
                goalObjectSpawnArray[0] = Task.ToSpawnInfo(goalObjects[0], nextEntrace,
                                                           new Vector3(), null);
            }
        }
        else if (taskType == 2)
        {
            // 2.1 Vital Reading
            if (taskIndex == 0)
            {
                // select a monitor value
                int valueInd = random.Next(monitorScreens.Length);
                task.result = monitorValues[valueInd];
                // get avaliable locations
                var (monitorPoss, monitorRots) = HospitalMapUtil.GetMonitorPose(nextRoomName);
                // select a monitor - manually set
                int monitorInd = Mathf.Clamp(monitorPoss.Length-2, 0, monitorPoss.Length-1);
                // spawn info
                taskObjectSpawnArray = new Task.SpawnInfo[1];
                taskObjectSpawnArray[0] = Task.ToSpawnInfo(monitorScreens[valueInd], 
                                                           monitorPoss[monitorInd], monitorRots[monitorInd], 
                                                           null);
                task.taskDescription += " (Bed " + (monitorInd+1) + ")";
            }
            // 2.2 Medicine Scanning
            else if (taskIndex == 1)
            {
                // selection pool based on level
                GameObject[] medicines = barcodeMedicines;
                string[] results = barcodeResults;
                if (levelIndex == 2)
                {
                    medicines = barcodeMedicinesSpecial;
                    results = barcodeResultsSpecial;
                }

                // select a monitor value   
                int valueInd = random.Next(results.Length);
                task.result = results[valueInd];
                // get avaliable locations
                Vector3[] tablePoss = HospitalMapUtil.GetFreeTableSpace(nextRoomName);
                // select a table - manually set
                int tableInd = tablePoss.Length - 1;
                // spawn info
                taskObjectSpawnArray = new Task.SpawnInfo[1];
                taskObjectSpawnArray[0] = Task.ToSpawnInfo(medicines[valueInd], 
                                                           tablePoss[tableInd], Vector3.zero, 
                                                           null);
            }
            // 2.3 Picking and Placing
            else if (taskIndex == 2)
            {
                // selection pool based on level
                GameObject[] meds = medicines;
                if (levelIndex == 2)
                    meds = medicinesHorizontal;

                // get avaliable locations
                Vector3[] tablePoss = HospitalMapUtil.GetFreeTableSpace(nextRoomName);
                // select a table - manually set
                int pickInd = tablePoss.Length - 2;
                int placeInd = tablePoss.Length - 1;
                // select a medicine value
                int ind = random.Next(meds.Length);
                // spawn info
                taskObjectSpawnArray = new Task.SpawnInfo[1];
                goalObjectSpawnArray = new Task.SpawnInfo[1];
                taskObjectSpawnArray[0] = Task.ToSpawnInfo(meds[ind], tablePoss[pickInd],
                                                           new Vector3(), null);
                goalObjectSpawnArray[0] = Task.ToSpawnInfo(goalObjects[1], tablePoss[placeInd],
                                                           new Vector3(), null);
            }
            // 2.4 Disinfection
            else if (taskIndex == 3)
            {
                // selection pool based on level
                GameObject furniture = disinfectionFurnitures[levelIndex];
                
                // get avaliable locations
                Vector3[] groudPoss = HospitalMapUtil.GetFreeGroundSpace(nextRoomName);
                // select a ground - manually set
                int ind = 0;
                // spawn info
                taskObjectSpawnArray = new Task.SpawnInfo[1];
                goalObjectSpawnArray = new Task.SpawnInfo[1];
                taskObjectSpawnArray[0] = Task.ToSpawnInfo(disinfectionPack, Vector3.zero,
                                                           Vector3.zero, null);
                goalObjectSpawnArray[0] = Task.ToSpawnInfo(furniture, groudPoss[ind],
                                                           new Vector3(), null);
            }

            // Secondary task
            // get avaliable locations
            Vector3[] freeGroudPoss = HospitalMapUtil.GetFreeGroundSpace(nextRoomName);
            // select a object
            int ind1 = random.Next(trashCan.Length);
            int ind2 = random.Next(laundryBasket.Length);
            // select a ground - manually set
            int ground1 = freeGroudPoss.Length-1;
            int ground2 = freeGroudPoss.Length-2;
            // spawn info
            Task.SpawnInfo[] secondaryObjectSpawnArray = new Task.SpawnInfo[2];
            secondaryObjectSpawnArray[0] = Task.ToSpawnInfo(trashCan[ind1], freeGroudPoss[ground1],
                                                            Vector3.zero, null);
            secondaryObjectSpawnArray[1] = Task.ToSpawnInfo(laundryBasket[ind2], freeGroudPoss[ground2],
                                                            new Vector3(), null);
            staticObjectSpawnArray = Utils.ConcatenateArray(staticObjectSpawnArray, 
                                                            secondaryObjectSpawnArray);
            task.CheckInput("TrashCan " + ind1 + "," + "Laundry " + ind2);
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

    
    private string[] GetRandomizedTaskOrder()
    {
        // Randomize order
        startIndex = Utils.Shuffle<int>(random, Enumerable.Range(0, numStep).ToArray())[0];
        indexArrayType1 = Utils.RandomizeRow<int>(random, indexArrayType1);
        indexArrayType2 = Utils.RandomizeRow<int>(random, indexArrayType2);
        string[] randomizedTasks = new string[numCircle * numStep * numType];

        // Based on the startStep, numStep of a circle and numCircle,
        // return the final randomized taks list
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
                    else if (k == 1)
                        randomizedTasks[count] = 
                            tasksType2[newJ, indexArrayType2[newJ, i]];
                    count ++;
                }
            }
        }
        return randomizedTasks;
    }

    private string GetNextRoom(int stepIndex, int type2TaskIndex, int type2LevelIndex)
    {
        string[] stepRoomNames = new string[0];
        string[] taskRoomNames = new string[0];

        // Avaliable rooms based on current step
        if (stepIndex == 0) 
            stepRoomNames = new string[] {"Room P103", "Room P104", "Room S102"};
        else if (stepIndex == 1)
            stepRoomNames = new string[] {"Room P105", "Room S103"};
        else if (stepIndex == 2)
            stepRoomNames = new string[] {"Pharmacy", "Room L101"};
        else if (stepIndex == 3)
            stepRoomNames = new string[] {"Room P101", "Room P102", "Room S101"};
        
        // Avaliable rooms based on current task and difficulty
        // Room L101 only for 2.1-2 and 2.4-3
        // Pharmacy only for 2.2-3 and 2.3-3
        if (type2TaskIndex == 0) 
        {
            if (type2LevelIndex == 0)
                taskRoomNames = new string[] {"Room P101", "Room P103", "Room P105"};
            else if (type2LevelIndex == 1)
                taskRoomNames = new string[] {"Room L101"};
            else if (type2LevelIndex == 2)
                taskRoomNames = new string[] {"Room S101", "Room S102", "Room S103"};
        }
        else if (type2TaskIndex == 1)
        {   
            if (type2LevelIndex == 0)
                taskRoomNames = new string[] {"Room P102", "Room P104", "Room P105"};
            else if (type2LevelIndex == 1)
                taskRoomNames = new string[] {"Room S101", "Room S102", "Room S103"};
            else if (type2LevelIndex == 2)
                taskRoomNames = new string[] {"Pharmacy"};
        }
        else if (type2TaskIndex == 2)
        {
            if (type2LevelIndex == 0)
                taskRoomNames = new string[] {"Room P101", "Room P103", "Room P105"};
            else if (type2LevelIndex == 1)
                taskRoomNames = new string[] {"Room S101", "Room S102", "Room S103"};
            else if (type2LevelIndex == 2)
                taskRoomNames = new string[] {"Pharmacy"};
        }
        else if (type2TaskIndex == 3)
        {
            if (type2LevelIndex == 0)
                taskRoomNames = new string[] {"Room P102", "Room P104", "Room P105", "Room L101"};
            else if (type2LevelIndex == 1)
                taskRoomNames = new string[] {"Room P101", "Room P103", "Room P105", "Room L101"};
            else if (type2LevelIndex == 2)
                taskRoomNames = new string[] {"Room S101", "Room S102", "Room S103", "Room L101"};
        }

        // Find room that satisfies both conditions
        string[] roomNames = stepRoomNames.Intersect(taskRoomNames).ToArray();
        if (roomNames.Length == 0)
            Debug.LogWarning("Task definition error, no proper room found for " + 
                             "2." + (type2TaskIndex+1) + "-"  + (type2LevelIndex+1) + 
                             "at step " + stepIndex);
        // randomly select one
        return Utils.Shuffle<string>(random, roomNames)[0];
    }


    // Utils
    private (int, int, int) GetIndicesFromName(string task)
    {
        string[] temp1 = task.Split(char.Parse("."));
        string[] temp2 = temp1[1].Split(char.Parse("-"));
        int taskType = int.Parse(temp1[0]);
        int taskIndex = int.Parse(temp2[0]) - 1;
        int levelIndex = int.Parse(temp2[1]) - 1;

        return (taskType, taskIndex, levelIndex);
    }

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
            var (taskType, taskIndex, levelIndex) = GetIndicesFromName(task);
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
}