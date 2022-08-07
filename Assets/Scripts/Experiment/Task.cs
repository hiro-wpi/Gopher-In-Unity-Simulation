using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Abstract task class for defining task in different types
/// </summary>
public abstract class Task : MonoBehaviour 
{
    // Task-related properties
    public string sceneName;
    public string taskName;
    public string taskDescription;
    public GraphicalInterface gUI;
    public ControlInterface cUI;

    // Task status
    // start
    protected bool taskStarted;
    protected float startTime;
    // end
    protected Goal[] goals = new Goal[0];
    protected int goalIndex;
    
    // Spawn objects
    public SpawnInfo[] robotSpawnArray;
    public SpawnInfo[] staticObjectSpawnArray;
    public SpawnInfo[] dynamicObjectSpawnArray;
    public SpawnInfo[] taskObjectSpawnArray;
    public SpawnInfo[] goalObjectSpawnArray;
    protected GameObject[] robots;
    protected GameObject[] staticObjects;
    protected GameObject[] dynamicObjects;
    protected GameObject[] taskObjects;
    protected GameObject[] goalObjects;
    // current controlled robot
    protected GameObject robot; 
    protected Vector3 robotStartPosition;

    // Data to record
    protected string[] valueToRecordHeader;
    protected string[] stringToRecordHeader;
    protected float[] valueToRecord;
    protected string[] stringToRecord;
    protected int userInputRecordIndex = -1;
    // user input
    protected string[] userInputs;
    protected int userInputIndex = -1;
    protected int userInputLength = 3;


    // Check if the current task is started
    public virtual bool CheckTaskStart()
    {
        if (robot == null) 
            return false;
        // If robot moves more than 0.1m
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
            return 0;
        else
            return Time.time - startTime;
    }

    // Check if the current task is done
    // Differ from tasks
    public abstract bool CheckTaskCompletion();


    // Task status
    // Will be displayed in the GUI
    public virtual string GetTaskStatus()
    {
        return "";
    }

    // Reset task status - for reloading the task
    public virtual void ResetTaskStatus()
    {
        taskStarted = false;
    }


    // Check user input (needed for some of the tasks)
    public virtual void CheckInput(string input)
    {
        if (userInputs == null)
            userInputs = new string[userInputLength];
        
        // store user input
        userInputIndex = (userInputIndex + 1) % userInputLength;
        userInputs[userInputIndex] = input;
    }

    // The header of extra task data to record besides robot's
    public virtual string[] GetTaskValueToRecordHeader()
    {
        valueToRecordHeader = new string[0];
        return valueToRecordHeader;
    }
    public virtual string[] GetTaskStringToRecordHeader()
    {
        stringToRecordHeader = new string[2];
        stringToRecordHeader[0] = "time";
        stringToRecordHeader[1] = "user_inputs";
        return stringToRecordHeader;
    }

    // Extra task data to record besides robot's
    public virtual float[] GetTaskValueToRecord()
    {
        valueToRecord = new float[0];
        return valueToRecord;
    }
    public virtual string[] GetTaskStringToRecord()
    {
        stringToRecord = new string[0];
        if (userInputRecordIndex != userInputIndex)
        {
            stringToRecord = new string[2];
            stringToRecord[0] = string.Format("{0:0.000}", Time.time);
            stringToRecord[1] = userInputs[userInputRecordIndex];
            userInputRecordIndex = (userInputRecordIndex + 1) % userInputLength;
        }
        return stringToRecord;
    }

    
    // Generate static objects for this task
    public virtual (GameObject[], GameObject[], GameObject[]) GenerateStaticObjects()
    {
        // Spawn static objects, task objects and goal objects
        staticObjects = SpawnGameObjectArray(staticObjectSpawnArray);
        taskObjects = SpawnGameObjectArray(taskObjectSpawnArray);
        goalObjects = SpawnGameObjectArray(goalObjectSpawnArray);

        // goals
        goals = new Goal[goalObjects.Length];
        for (int i = 0; i < goalObjects.Length; ++i)
            goals[i] = goalObjects[i].GetComponent<Goal>();

        return (staticObjects, taskObjects, goalObjects);
    }

    // Generate dynamic objects for this task
    public virtual GameObject[] GenerateDynamicObjects()
    {
        // Spawn dynamic objects - Humans
        dynamicObjects = SpawnGameObjectArray(dynamicObjectSpawnArray);
        
        // set trajectory for each human
        for (int i=0; i<dynamicObjects.Length; i++)
        {
            CharacterNavigation charNav = dynamicObjects[i].GetComponent<CharacterNavigation>();
            charNav.SetTrajectory(dynamicObjectSpawnArray[i].trajectory);
            charNav.loop = true;
        }

        return dynamicObjects;
    }

    // Destroy all spawned objects
    public virtual void DestroyObjects(bool deStatic = true, bool deTask = true,
                                       bool deGoal = true, bool deDynamic = true)
    {
        if (deStatic)
            DestoryGameObjects(staticObjects);
        if (deTask)
            DestoryGameObjects(taskObjects);
        if (deGoal)
            DestoryGameObjects(goalObjects);
        if (deDynamic)
            DestoryGameObjects(dynamicObjects);
    }
    private void DestoryGameObjects(GameObject[] gameObjects)
    {
        if (gameObjects == null)
            return;
        foreach (GameObject obj in gameObjects)
            Destroy(obj);
    }

    // Generate robots for this task
    public virtual GameObject[] GenerateRobots()
    {
        // Spawn robot
        robots = SpawnGameObjectArray(robotSpawnArray);
        robot = robots[0];
        robotStartPosition = robot.transform.position;

        // GUI set output
        gUI.SetRobot(robot);
        gUI.SetTask(this);

        return robots;
    }

    // In the case that same existed robots are used
    public virtual void SetRobots(GameObject[] existingRobots)
    {
        // Set existing robots
        robots = existingRobots;
        robot = robots[0];
        robotStartPosition = robot.transform.position;

        // GUI set output
        gUI.SetRobot(robot);
        gUI.SetTask(this);
    }


    // Define SpawnInfo for spawning objects
    [System.Serializable]
    public struct SpawnInfo
    {
        public GameObject gameObject;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3[] trajectory; // for dynamic objects
    };
    // Create a spawn info from given gameObject
    public static Task.SpawnInfo ToSpawnInfo(GameObject gameObject, 
                                             Vector3 position, Vector3 rotation, 
                                             Vector3[] trajectory = null)
    {
        Task.SpawnInfo spawnInfo;
        spawnInfo.gameObject = gameObject;
        spawnInfo.position = position;
        spawnInfo.rotation = rotation;
        spawnInfo.trajectory = trajectory;
        return spawnInfo;
    }
    // Generate an array of game objects from an array of spawnInfo
    public static GameObject[] SpawnGameObjectArray(SpawnInfo[] spawnInfos)
    {
        if (spawnInfos == null)
            spawnInfos = new SpawnInfo[0];
        
        GameObject[] gameObjects = new GameObject[spawnInfos.Length];
        for (int i=0; i < spawnInfos.Length; ++i)
        {
            SpawnInfo spawnInfo = spawnInfos[i];
            GameObject gameObject = Instantiate(spawnInfo.gameObject,
                                                spawnInfo.position, 
                                                Quaternion.Euler(spawnInfo.rotation));
            gameObjects[i] = gameObject;
        }
        return gameObjects;
    }
}
