using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Experiment : MonoBehaviour 
{
    // General
    public Task[] tasks;
    protected bool useSameScene;
    protected string[] sceneNames;
    protected string[] levelNames;
    protected string[] taskNames;
    protected string[] taskDescriptions;

    // Object
    // robot
    public GameObject[] robotPrefabs;
    protected Vector3[] robotSpawnPositions;
    protected Vector3[] robotSpawnRotations;
    // static object
    public GameObject[] staticObjects;
    protected Vector3[] staticObjectSpawnPositions;
    protected Vector3[] staticObjectSpawnRotations;
    // human
    public GameObject[] dynamicObjects;
    protected Vector3[] dynamicObjectSpawnPositions;
    protected Vector3[] dynamicObjectSpawnRotations;
    protected Vector3[,] dynamicObjectTrajectories;
    
    // task object
    public GameObject[] taskObjects;
    protected Vector3[] taskObjectSpawnPositions;
    protected Vector3[] taskObjectSpawnRotations;
    // goal object
    public GameObject[] goalObjects;
    protected Vector3[] goalSpawnPositions;
    protected Vector3[] goalSpawnRotations;

    // Interface
    public GraphicalInterface[] graphicalInterfaces;
    public ControlInterface[] controlInterfaces;

    
    // Generate task
    protected virtual T GenerateTask<T>(int levelIndex, int taskIndex, GameObject parent) 
                where T : Task 
    {
        // Instantiate task
        GameObject taskObject = new GameObject(levelNames[levelIndex] + "-" + taskNames[taskIndex]);
        taskObject.transform.parent = parent.transform;
        T task = taskObject.AddComponent<T>();

        // General
        task.sceneName = sceneNames[0];
        if (!useSameScene)
            task.sceneName = sceneNames[taskIndex];
        task.taskName = levelNames[levelIndex] + "-" + taskNames[taskIndex];
        task.taskDescription = taskDescriptions[taskIndex];
        
        // Detailed task information can be specified in derived method
        return task;
    }


    // Get the total numbe of tasks in this experiment
    public int GetNumTasks()
    {
        return tasks.Length;
    }
    // Get a specific task
    public Task GetTask(int taskIndex)
    {
        if (taskIndex > tasks.Length)
            return null;
        
        return tasks[taskIndex];
    }
} 