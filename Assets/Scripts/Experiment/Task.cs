using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task : MonoBehaviour 
{
    public struct SpawnInfo
    {
        public GameObject gameObject;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3[] trajectory; // for dynamic objects
    };
    
    // Task-related properties
    protected UserInterface uI;
    protected string taskName;
    protected string sceneName;

    // Get or set the GUI
    public UserInterface UI{ get; set; }
    // Get or set the name of this task
    public string TaskName{ get; set; }
    // Get or set the name of the scene
    public string SceneName{ get; set; }

    // Spawn objects
    protected SpawnInfo[] objectSpawnArray;
    public SpawnInfo[] robotSpawnArray;
    protected GameObject[] objects;
    protected GameObject[] robots;

    // Set object spawn array
    public virtual void SetObjectSpawnArray(SpawnInfo[] objectSpawnArray)
    {
        this.objectSpawnArray = objectSpawnArray;
    }
    // Set robot spawn array
    public virtual void SetRobotSpawnArray(SpawnInfo[] robotSpawnArray)
    {
        this.robotSpawnArray = robotSpawnArray;
    }

    // Data to record
    protected float[] valueToRecord;
    protected string[] stringToRecord;

    // Get value array for recording
    public float[] ValueToRecord { get; }
    // Get string array for recording
    public string[] StringToRecord { get; }

    // Check if the current task is done
    // Use int but not bool in order to identify different ending situations
    public abstract int CheckTaskCompletion();

    // Generate objects for this task
    public virtual void GenerateObjects()
    {
        objects = SpawnGameObjectArray(objectSpawnArray);
    }

    // Generate robots for this task
    public virtual void GenerateRobots()
    {
        robots = SpawnGameObjectArray(robotSpawnArray);
    }

    // Utils, generate an array of game objects
    protected GameObject[] SpawnGameObjectArray(SpawnInfo[] spawnInfos)
    {
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
