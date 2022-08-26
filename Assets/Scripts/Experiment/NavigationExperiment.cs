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
    // for waypoint navigation task
    private Vector3[] wayPointGoalPositions;

    void Start()
    {
        // General
        useSameScene = true;  // use the same scene for all tasks
        sceneNames = new string[] {"Hospital"};
        levelNames = new string[] {"Level1", "Level2", "Level3"};
        taskNames = new string[] {"Corridor", "Turning", "Door", "Waypoints"};
        taskDescriptions = new string[] {"Please go along the corridor and reach the goal.", 
                                         "Please go along the corridor and turn at the next intersection.", 
                                         "Please go through the right door and reach the goal.", 
                                         "Please follow the waypoints."};
        
        // Robot spawn pose and goal
        robotSpawnPositions = new Vector3[]
                            {
                                new Vector3( 8.0f, 0.0f, -1.0f),
                                new Vector3(-7.0f, 0.0f, -1.0f),
                                new Vector3(-6.5f, 0.0f, -1.0f),
                                new Vector3( 4.4f, 0.0f, -5.5f)
                            };
        robotSpawnRotations = new Vector3[]
                            {
                                new Vector3(0f, -90f, 0f), 
                                new Vector3(0f,  90f, 0f), 
                                new Vector3(0f, 180f, 0f), 
                                new Vector3(0f,  90f, 0f)
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
                                new Vector3(-7.0f,  0.0f, -3.0f), 
                                new Vector3( 0.5f,  0.0f,  6.5f), 
                                new Vector3(-10.0f, 0.0f, -5.0f)
                            };
        wayPointGoalPositions = new Vector3[]
                            {
                                new Vector3(7.7f, 0.0f, -4.5f), new Vector3( 7.7f, 0.0f, 7.5f), 
                                new Vector3(-7.0f, 0.0f, 8.5f), new Vector3(-7.0f, 0.0f, 13.0f)
                            };

        // Create a gameobject container
        GameObject tasksParentObject = new GameObject("Navigation Tasks");
        tasksParentObject.transform.SetParent(this.transform);
        // Tasks
        tasks = new NavigationTask[taskNames.Length * levelNames.Length];

        // Set up tasks
        int count = 0;
        for (int i=0; i<levelNames.Length; ++i)
        {
            for (int j=0; j<taskNames.Length; ++j)
            {
                tasks[count] = GenerateTask<NavigationTask>(i, j, tasksParentObject);
                count++;
            }
        }
    }


    protected override NavigationTask GenerateTask<NavigationTask>(
           int levelIndex, int taskIndex, GameObject parent = null)
    {
        // General info
        NavigationTask task = base.GenerateTask<NavigationTask>(levelIndex, taskIndex, parent);

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
        // goals
        Vector3[] goals;
        if (taskNames[taskIndex] == "Waypoints")
            goals = wayPointGoalPositions;
        else
            goals = new Vector3[1] {goalSpawnPositions[taskIndex]};
        Task.SpawnInfo[] goalSpawnArray = new Task.SpawnInfo[goals.Length];
        for (int i = 0; i < goals.Length; ++i)
        {
            goalSpawnArray[i] = Task.ToSpawnInfo(goalObjects[0], 
                                                 goals[i], 
                                                 new Vector3(), 
                                                 null);
        }

        // robot, object, and human spawn array
        task.robotSpawnArray = robotSpawnArray;
        task.staticObjectSpawnArray = staticObjectSpawnArray;
        task.dynamicObjectSpawnArray = humanSpawnArray;
        // task goal
        task.goalObjectSpawnArray = goalSpawnArray;

        // Interface -> all using the same
        task.gUI = graphicalInterfaces[0];
        // TODO task.CUI = controlInterfaces[0];

        return task;
    }
}
