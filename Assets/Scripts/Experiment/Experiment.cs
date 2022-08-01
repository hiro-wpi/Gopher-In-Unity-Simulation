using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Experiment : MonoBehaviour 
{
    // Store all the tasks
    // temp protected Task[] tasks;
    public Task[] tasks;
    private System.Random randomInt = new System.Random();
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
        randomizedIndices = Enumerable.Range(0, tasks.Length).ToArray();

        // TODO
        Array.Reverse(randomizedIndices);
        //randomizedIndices = randomizedIndices.OrderBy(x => randomInt.Next()).ToArray();
    }

    // Get a specific task
    public Task GetTask(int taskIndex)
    {
        if (taskIndex > tasks.Length)
            return null;

        if (randomizedIndices == null)
            return tasks[taskIndex];
        else
            return tasks[randomizedIndices[taskIndex]];
    }
} 