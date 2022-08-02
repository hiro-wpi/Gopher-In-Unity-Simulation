using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining free play. 
//  No actual task and goal are given so that the task would not end.
/// </summary>
public class FreePlayTask : Task 
{
    void Start()
    {}


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
        return "N/A";
    }
}