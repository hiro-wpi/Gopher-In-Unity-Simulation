using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     Abstract task class for defining task in different types
/// 
///     There are multiple functions that can be override for different tasks
///     Task status related:
///         CheckTaskStart(),
///         GetTaskDuration(),
///         CheckTaskCompletion(),
///         GetTaskStatus(),
///         SetResult(),
///         SetEndTime(),
///         CheckInput(),
///         ResetTask()
///     Object generation related:
///         GenerateStaticObjects(),
///         GenerateDynamicObjects(),
///         GenerateTaskObjects(),
///         GenerateGoalObjects(),
///         GenerateRobots()
///     Instead of generating it, it could also take the existing robots
///     or objects and set them to the task:
///         SetRobots()
///         SetStaticObjects()
///         SetDynamicObjects()
///         SetTaskObjects()
///         SetGoalObjects()
/// </summary>
public abstract class Task : MonoBehaviour 
{
    [Header("Task Properties")]
    // Task-related properties
    public string TaskName;
    public string TaskDescription;

    [Header("Scene Setup")]
    // Scene
    public string SceneName;
    // Objects to spawn
    public SpawnInfo[] StaticObjectSpawnArray;
    public SpawnInfo[] DynamicObjectSpawnArray;
    public SpawnInfo[] TaskObjectSpawnArray;
    public SpawnInfo[] GoalObjectSpawnArray;

    [Header("Robot")]
    // Robot
    public SpawnInfo[] RobotSpawnArray;
    public GraphicalInterface GUI;

    // Task status
    // start
    protected bool taskStarted;
    protected float startTime;
    // task could be end due to
    // 1, correct user input
    // 2, object reaches goal
    // 3, reach end time
    protected string result;
    protected Goal[] goals = new Goal[0];
    protected int goalIndex;
    protected float endTime = 100f;

    // Scene objects
    protected GameObject[] staticObjects;
    protected GameObject[] dynamicObjects;
    protected GameObject[] taskObjects;
    protected GameObject[] goalObjects;

    // Robot
    protected GameObject[] robots;
    protected GameObject robot;
    protected Vector3 robotStartPosition;
    protected Quaternion robotStartRotation;

    // User input
    protected string userInput;
    protected bool userInputReceived;

    // Check if the current task is started
    public virtual bool CheckTaskStart()
    {
        if (robot == null)
        {
            return false;
        }

        // Default - if robot moves more than 0.1m
        if ((robot.transform.position - robotStartPosition).magnitude > 0.1)
        {
            startTime = Time.time;
            taskStarted = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    // Get task running time
    public virtual float GetTaskDuration()
    {
        if (!taskStarted)
        {
            return 0;
        }
        else
        {
            return Time.time - startTime;
        }
    }

    // Check if the current task is done
    public virtual bool CheckTaskCompletion()
    {
        if (robot == null)
        {
            return true;
        }

        // Default - time out
        if (GetTaskDuration() > endTime)
        {
            return true;
        }
        return false;
    }

    // Task status - can be displayed in the GUI
    public virtual string GetTaskStatus()
    {
        return "";
    }

    // Set task ending condition
    public virtual void SetResult(string value)
    {
        result = value;
    }

    // Set ending time
    public virtual void SetEndTime(float value)
    {
        endTime = value;
    }

    // Check and store user input
    public virtual void CheckInput(string input)
    {
        userInput = input;
        userInputReceived = true;

        // Could check the result here
        // if (input == result)
    }

    // Reset task - for reloading the task
    public virtual void ResetTask()
    {
        taskStarted = false;
    }

    // Load the scene
    // Be careful: this would destroy all the existing objects
    // that is not a child of this game object
    public virtual void LoadScene()
    {
        // Load the scene
        SceneManager.LoadScene(SceneName);
        DontDestroyOnLoad(gameObject);
    }

    // Generate static objects for this task
    public virtual GameObject[] GenerateStaticObjects()
    {
        // Spawn static objects
        staticObjects = SpawnGameObjectArray(StaticObjectSpawnArray);
        return staticObjects;
    }

    // Generate dynamic objects for this task
    public virtual GameObject[] GenerateDynamicObjects()
    {
        // Spawn dynamic objects - Humans
        dynamicObjects = SpawnGameObjectArray(DynamicObjectSpawnArray);
        
        // set trajectory for each human
        for (int i = 0; i < dynamicObjects.Length; i++)
        {
            CharacterNavigation charNav = 
                dynamicObjects[i].GetComponent<CharacterNavigation>();
            charNav.SetTrajectory(DynamicObjectSpawnArray[i].trajectory, true);
        }

        return dynamicObjects;
    }

    // Generate task objects for this task
    public virtual GameObject[] GenerateTaskObjects()
    {
        // Spawn task objects
        taskObjects = SpawnGameObjectArray(TaskObjectSpawnArray);
        return taskObjects;
    }

    // Generate goal objects for this task
    public virtual GameObject[] GenerateGoalObjects()
    {
        // Spawn goal objects
        goalObjects = SpawnGameObjectArray(GoalObjectSpawnArray);

        // Goals
        goals = new Goal[goalObjects.Length];
        for (int i = 0; i < goalObjects.Length; ++i)
        {
            goals[i] = goalObjects[i].GetComponent<Goal>();
        }

        return goalObjects;
    }

    // Generate robots for this task
    public virtual GameObject[] GenerateRobots()
    {
        // Spawn robot
        robots = SpawnGameObjectArray(RobotSpawnArray);
        robot = robots[0];
        robotStartPosition = robot.transform.position;

        // GUI set output
        GUI.SetRobot(robot, true);
        GUI.SetTask(this);
        return robots;
    }

    // Existed robots are used
    public virtual void SetRobots(GameObject[] existingRobots)
    {
        // Set existing robots
        robots = existingRobots;
        robot = robots[0];
        robotStartPosition = robot.transform.position;
        robotStartRotation = robot.transform.rotation;

        // GUI set output
        GUI.SetRobot(robot, false);
        GUI.SetTask(this);
    }

    // Existed static objects are used
    public virtual void SetStaticObjects(GameObject[] existingStaticObjects)
    {
        staticObjects = existingStaticObjects;
    }

    // Existed dynamic objects are used
    public virtual void SetDynamicObjects(GameObject[] existingDynamicObjects)
    {
        dynamicObjects = existingDynamicObjects;
    }

    // Existed task objects are used
    public virtual void SetTaskObjects(GameObject[] existingTaskObjects)
    {
        taskObjects = existingTaskObjects;
    }

    // Existed goal objects are used
    public virtual void SetGoalObjects(GameObject[] existingGoalObjects)
    {
        goalObjects = existingGoalObjects;
    }

    // Destroy all spawned objects
    public virtual void DestroyObjects(
        bool destroyStatic = true, 
        bool destroyTask = true,
        bool destroyGoal = true, 
        bool destroyDynamic = true, 
        float delayTime = 0f
    ) {
        if (destroyStatic)
        {
            DestoryGameObjects(staticObjects, delayTime);
        }
        if (destroyTask)
        {
            DestoryGameObjects(taskObjects, delayTime);
        }
        if (destroyGoal)
        {
            DestoryGameObjects(goalObjects, delayTime);
        }
        if (destroyDynamic)
        {
            DestoryGameObjects(dynamicObjects, delayTime);
        }
    }

    protected void DestoryGameObjects(
        GameObject[] gameObjects, float delayTime
    ) {
        foreach (GameObject obj in gameObjects)
        {
            Destroy(obj, delayTime);
        }     
    }

    // Define SpawnInfo for spawning objects in the scene
    [System.Serializable]
    public struct SpawnInfo
    {
        public GameObject gameObject;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3[] trajectory;  // for dynamic objects
    };

    // Create a spawn info from given gameObject
    public static SpawnInfo ToSpawnInfo(
        GameObject gameObject,
        Vector3 position,
        Vector3 rotation,
        Vector3[] trajectory = null
    ) {
        SpawnInfo spawnInfo;
        spawnInfo.gameObject = gameObject;
        spawnInfo.position = position;
        spawnInfo.rotation = rotation;
        spawnInfo.trajectory = trajectory;
        return spawnInfo;
    }

    // Generate an array of game objects from an array of spawnInfo
    public static GameObject[] SpawnGameObjectArray(SpawnInfo[] spawnInfos)
    {
        if (spawnInfos == null || spawnInfos.Length == 0)
        {
            return new GameObject[0];
        }

        // Spawn a list of objects
        GameObject[] gameObjects = new GameObject[spawnInfos.Length];
        for (int i=0; i < spawnInfos.Length; ++i)
        {
            SpawnInfo spawnInfo = spawnInfos[i];
            GameObject gameObject = Instantiate(
                spawnInfo.gameObject,
                spawnInfo.position,
                Quaternion.Euler(spawnInfo.rotation)
            );
            gameObjects[i] = gameObject;
        }
        return gameObjects;
    }
}
