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
    public GameObject[] pickAndPlaceTaskObjects;
    private Vector3[] pickAndPlaceSpawnPositions;

    // Define task details
    void Start()
    {
        // General
        useSameScene = true;  // use the same scene for all tasks
        sceneNames = new string[] {"Hospital"};
        levelNames = new string[] {"Level1", "Level2", "Level3"};
        taskNames = new string[] {"FindAndGrasp", "Carrying", "Pushing", "PickAndPlace"};
        taskDescriptions = new string[] {"Please find the object with blue label in the Pharmacy, " +
                                            "and put it on the medical tray in the Pharmacy.", 
                                         "Please carry the IV pole to the Treatment Room 1.", 
                                         "Please push the medical cart to the Office.",
                                         "Please pick up all medicines on the cart in Room S103, " + 
                                            "and put them on the medical tray."};
        
        // Robot spawn pose and goal
        robotSpawnPositions = new Vector3[]
                            {
                                new Vector3(-7.0f, 0.0f, 10.5f),
                                new Vector3( 7.5f, 0.0f, -1.0f),
                                new Vector3(-7.0f, 0.0f, -1.0f),
                                new Vector3( 7.0f, 0.0f, 10.5f)
                            };
        robotSpawnRotations = new Vector3[]
                            {
                                new Vector3(0f,   0f, 0f), 
                                new Vector3(0f, -90f, 0f), 
                                new Vector3(0f,  90f, 0f),
                                new Vector3(0f,   0f, 0f)
                            };
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

        // Goals
        goalSpawnPositions = new Vector3[]
                            {
                                new Vector3(-5.8f,  0.9f, 11.2f), 
                                new Vector3(-3.0f,  0.0f,  1.0f),
                                new Vector3( 0.0f,  0.0f, 11.0f),
                                new Vector3( 6.8f,  0.8f, 13.9f)
                            };
        
        // Task
        taskObjectSpawnPositions = new Vector3[]
                            {
                                new Vector3(-8.9f, 1.2f, 13.5f),
                                new Vector3( 6.5f, 0.0f, -1.6f),
                                new Vector3(-6.5f, 0.0f, -1.0f)
                            };
        taskObjectSpawnRotations = new Vector3[]
                            {
                                new Vector3(0f,  0f, 0f), 
                                new Vector3(0f,  0f, 0f), 
                                new Vector3(0f, 90f, 0f)
                            };
        pickAndPlaceSpawnPositions = new Vector3[]
                            {
                                new Vector3(6.6f, 0.8f, 13.7f),
                                new Vector3(6.8f, 0.8f, 13.7f),
                                new Vector3(6.8f, 0.8f, 13.6f),
                            };

        // Create a gameobject container
        GameObject tasksParentObject = new GameObject("Grasping Tasks");
        tasksParentObject.transform.SetParent(this.transform);
        // Tasks
        tasks = new GraspingTask[taskNames.Length * levelNames.Length];

        // Set up tasks
        int count = 0;
        for (int i=0; i<levelNames.Length; ++i)
        {
            for (int j=0; j<taskNames.Length; ++j)
            {
                tasks[count] = GenerateTask<GraspingTask>(i, j, tasksParentObject);
                count++;
            }
        }
    }


    protected override GraspingTask GenerateTask<GraspingTask>(
       int levelIndex, int taskIndex, GameObject parent = null)
    {
        // General info
        GraspingTask task = base.GenerateTask<GraspingTask>(levelIndex, taskIndex, parent);

        // Detailed spawning info
        // robot
        Task.SpawnInfo[] robotSpawnArray = new Task.SpawnInfo[1];
        robotSpawnArray[0] = Task.ToSpawnInfo(robotPrefabs[0], 
                                              robotSpawnPositions[taskIndex], 
                                              robotSpawnRotations[taskIndex], 
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

        // task object
        Task.SpawnInfo[] taskObjectSpawnArray;
        if (taskNames[taskIndex] != "PickAndPlace")
        {
            taskObjectSpawnArray = new Task.SpawnInfo[1];
            taskObjectSpawnArray[0] = Task.ToSpawnInfo(taskObjects[taskIndex], 
                                                       taskObjectSpawnPositions[taskIndex], 
                                                       taskObjectSpawnRotations[taskIndex], 
                                                       null);
        }
        else
        {
            taskObjectSpawnArray = new Task.SpawnInfo[pickAndPlaceTaskObjects.Length];
            for (int i = 0; i < taskObjectSpawnArray.Length; ++i)
            {
                taskObjectSpawnArray[i] = Task.ToSpawnInfo(pickAndPlaceTaskObjects[i], 
                                                           pickAndPlaceSpawnPositions[i], 
                                                           new Vector3(), 
                                                           null);
            }
        }
        
        // goals
        Vector3 goal = goalSpawnPositions[taskIndex];
        Task.SpawnInfo[] goalSpawnArray = new Task.SpawnInfo[1];
        goalSpawnArray[0] = Task.ToSpawnInfo(goalObjects[taskIndex], 
                                             goal, 
                                             new Vector3(), 
                                             null);
        
        // robot, object, and human spawn array
        task.robotSpawnArray = robotSpawnArray;
        task.staticObjectSpawnArray = staticObjectSpawnArray;
        task.dynamicObjectSpawnArray = humanSpawnArray;
        // task goal
        task.goalObjectSpawnArray = goalSpawnArray;
        task.taskObjectSpawnArray = taskObjectSpawnArray;

        // Interface -> all using the same
        task.gUI = graphicalInterfaces[0];
        // TODO task.CUI = controlInterfaces[0];

        return task;
    }
}
