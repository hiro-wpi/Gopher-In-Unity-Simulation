using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for quick prototyping some tasks and experiment.
/// </summary>
public class TestExperiment : Experiment 
{
    void Start()
    {
        // General
        useSameScene = true;  // use the same scene for all tasks
        sceneNames = new string[] {"Hospital"};
        levelNames = new string[] {"Level1"};
        taskNames = new string[] {
                                  "Navigation", "Reading",
                                  "Carrying", "Scanning", 
                                  "Pushing", "Grasping", 
                                  "NavObs", "Disinfection"
                                 };
        // taskNames = new string[] {"GoHome", "Carrying", "Pushing", "LocalGrasping", "Navigation"};
        taskDescriptions = new string[] {"Please navigate to room S103.", 
                                         "Please read the vital value of Bed 2 in Room S103.",

                                         "Please carry the IV pole to Room P104.",
                                         "Please scan the medicine on the table and enter the bar code number.",
                                         
                                         "Please push the medical cart to the Pharmacy.",
                                         "Please pick up the medicine with blue label in the medicine cabinet, " + 
                                         "and put it on the tray on the table.",
                                         
                                         "Please navigate to Room P101.",
                                         "Please disinfect the table.",
                                        };
        
        // Robot spawn pose and goal
        robotSpawnPositions = new Vector3[] {new Vector3(11.0f, 0.0f, 2.0f)};
        robotSpawnRotations = new Vector3[] {new Vector3(0f, 180f, 0f)};
                        
        // Human spawn position and trajectories
        dynamicObjectSpawnPositions = new Vector3[]
                            {
                                new Vector3(-10.0f, 0f, -6.0f),
                                new Vector3( 11.0f, 0f,  6.0f),
                                new Vector3(  3.0f, 0f,  7.5f),
                                new Vector3( -2.5f, 0f, -1.5f)
                            };
        dynamicObjectTrajectories = new Vector3[,]
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
        GameObject tasksObject = new GameObject("Comprehensive Tasks");
        tasksObject.transform.SetParent(this.transform);

        // Set up tasks
        int count = 0;
        for (int j=0; j<tasks.Length; ++j)
        {
            tasks[count] = GenerateTask<Task>(0, j, tasksObject);
            count++;
        }
    }

    protected override T GenerateTask<T>(int levelIndex, int taskIndex, 
                                         GameObject parent = null)
    {
        // TEMP // General Info
        GameObject taskObject = tasks[taskIndex].gameObject;
        taskObject.transform.parent = parent.transform;
        Task task = tasks[taskIndex];
        
        // General
        task.sceneName = sceneNames[0];
        // task.taskName = levelNames[levelIndex] + "-" + taskNames[taskIndex];
        // task.taskDescription = taskDescriptions[taskIndex];

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

        return (T)task;
    }
}