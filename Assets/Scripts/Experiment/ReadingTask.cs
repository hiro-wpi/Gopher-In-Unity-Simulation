using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining reading task.
/// </summary>
public class ReadingTask : Task 
{
    public string result;
    private bool isCorrect = false;

    void Start()
    {
    }

    public override void CheckInput(string input)
    {
        // Record input
        base.CheckInput(input);
        
        // Check result
        if (input == result)
            isCorrect = true;
    }
    
    public override bool CheckTaskCompletion()
    {
        return isCorrect;
    }


    // Get current task status
    public override string GetTaskStatus()
    { 
        string status = "Your input was: ";
        if (userInputs != null)
            status += userInputs[userInputIndex];
        return status;
    }

    public override void ResetTaskStatus()
    {
        taskStarted = false;
        isCorrect = false;
    }
}