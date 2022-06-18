using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Experiment : MonoBehaviour 
{
    // Store all the tasks
    protected Task[] tasks;
    private int[] randomizedIndices;

    // Get the total numbe of tasks in this experiment
    public int GetNumTasks()
    {
        return tasks.Length;
    }

    // Randomize the task orders
    public void RandomizeTasks()
    {
        // Generate a random indices array
        System.Random randomInt = new System.Random();
        randomizedIndices = Enumerable.Range(0, tasks.Length).ToArray();
        randomizedIndices = randomizedIndices.OrderBy(x => randomInt.Next()).ToArray();
    }

    // Get a specific task
    public Task GetTask(int taskIndex)
    {
        if (randomizedIndices == null)
            return tasks[taskIndex];
        else
            return tasks[randomizedIndices[taskIndex]];
    }

    // Utils
    protected Task.SpawnInfo ToSpawnInfo(GameObject gameObject, 
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
} 