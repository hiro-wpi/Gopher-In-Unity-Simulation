using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining navigation task.
/// </summary>
public class NavigationTask : Task 
{
    void Start()
    {
    }


    public override bool CheckTaskCompletion()
    {
        if (robot == null)
            return false;
        // Check if robot reaches the neighbor of the current goal
        if (goals[goalIndex].CheckIfObjectReachedGoal(robot))
        {
            // prev goal
            goals[goalIndex].DisableGoalVisualEffect();

            // next goal
            goalIndex++;
            if (goalIndex != goals.Length)
            {
                goals[goalIndex].EnableGoalVisualEffect(); 
            }

            // if all goal reached
            else
            {
                gUI.ShowPopUpMessage("Current Task Completed!");
                return true;
            }
        }
        return false;
    }


    public override string GetTaskStatus()
    {
        if (goalIndex == goals.Length)
            return "The task is completed.";

        float distance = goals[goalIndex].GetDistanceToGoal(robot);
        return "The robot is " + 
               string.Format("{0:0.000}", distance) + " m" + "\n" +
               "away from the goal.";
    }

    public override void ResetTaskStatus()
    {
        // Reset task flag
        taskStarted = false;
        // Reset goal
        goalIndex = 0;
        goals[0].EnableGoalVisualEffect();
        for (int i = 1; i < goalObjects.Length; ++i)
            goals[i].DisableGoalVisualEffect(); 
    }


    public override GameObject[] GenerateGoalObjects()
    {
        base.GenerateGoalObjects();

        // Keep only the first goal active
        goals[0].EnableGoalVisualEffect();
        for (int i = 1; i < goalObjects.Length; ++i)
            goals[i].DisableGoalVisualEffect();

        return goalObjects;
    }
}