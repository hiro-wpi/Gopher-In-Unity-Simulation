using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining free play. 
//  No actual task and goal are given so that the task would not end.
/// </summary>
public class FreePlayTask : Task 
{
    private string scene;
    private string taskDescription;
    
    // object
    public GameObject levelObject;
    // human
    public GameObject nurse;
    private Vector3[] humanSpawnPositions;
    private Vector3[,] humanTrajectories;
    // robot
    public GameObject robot;
    private Vector3 spawnPosition;
    private Vector3 spawnRotation;


    void Start()
    {
        spawnPosition = new Vector3( 8.0f, 0.0f, -1.0f);
        spawnRotation = new Vector3(0f, -90f, 0f);
        humanSpawnPositions = new Vector3[]
                        {
                            new Vector3(-6.5f, 0f, -11.0f), 
                            new Vector3( 0.5f, 0f,  7.5f),
                            new Vector3( 7.5f, 0f,  7.5f)
                        };
        humanTrajectories = new Vector3[,]
                        {
                            {new Vector3(-7.0f, 0f, -1.5f), new Vector3(-7.0f, 0f,  7.5f), 
                             new Vector3( 0.5f, 0f,  7.5f), new Vector3( 0.5f, 0f, -2.0f)},
                            {new Vector3( 0.5f, 0f, -1.5f), new Vector3( 5.0f, 0f, -1.5f), 
                             new Vector3( 3.0f, 0f,  7.5f), new Vector3( 0.5f, 0f,  7.5f)}, 
                            {new Vector3( 7.5f, 0f, -1.5f), new Vector3(-6.5f, 0f, -1.5f), 
                             new Vector3(-6.5f, 0f,  7.5f), new Vector3( 7.5f, 0f,  7.5f)}
                        };

        robotSpawnArray = new Task.SpawnInfo[1];
        robotSpawnArray[0] = Task.ToSpawnInfo(robot, 
                                              spawnPosition, spawnRotation, 
                                              null);
        staticObjectSpawnArray = new Task.SpawnInfo[1];
        staticObjectSpawnArray[0] = Task.ToSpawnInfo(levelObject, 
                                               new Vector3(), new Vector3(), 
                                               null);
        dynamicObjectSpawnArray = new Task.SpawnInfo[3];
        for (int i = 0; i < dynamicObjectSpawnArray.Length; ++i)
            dynamicObjectSpawnArray[i] = Task.ToSpawnInfo(nurse, 
                                                         humanSpawnPositions[i], new Vector3(), 
                                                         Utils.GetRow(humanTrajectories, i));
    }

    public override bool CheckTaskStart()
    {
        return false;
    }

    public override bool CheckTaskCompletion()
    {
        return false;
    }


    public override string GetTaskStatus()
    {
        return "Free play mode." + "\n" + 
               "No task is assigned";
    }

    public override void ResetTaskStatus()
    {
        taskStarted = false;
    }
}