using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining navigation task.
/// </summary>
public class NavigationTask : Task 
{
    private bool isPaused = false;
    private float previousPauseDuration = 0;
    private float previousTimePaused;


    private int buttonState = 0;
    protected float timePausedDuration = 0; // The duration that we paused the task
    protected float pausedTime = 0;


    void Start() {}

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
                GUI.ShowPopUpMessage("Current Task Completed!");
                return true;
            }
        }
        return false;
    }

    public override string GetTaskStatus()
    {
        if (goalIndex == goals.Length)
        {
            return "The task is completed.";
        }

        float distance = goals[goalIndex].GetDistanceToGoal(robot);
        return (
            "The robot is "
            + string.Format("{0:0.000}", distance)
            + " m"
            + "\n"
            + "away from the goal."
        );
    }

    public override void ResetTask()
    {
        // Reset task flag
        taskStarted = false;

        // Reset goal
        goalIndex = 0;
        goals[0].EnableGoalVisualEffect();
        for (int i = 1; i < goalObjects.Length; ++i)
        {
            goals[i].DisableGoalVisualEffect();
        }
    }

    public override GameObject[] GenerateGoalObjects()
    {
        base.GenerateGoalObjects();

        // Keep only the first goal active
        goals[0].EnableGoalVisualEffect();
        for (int i = 1; i < goalObjects.Length; ++i)
        {
            goals[i].DisableGoalVisualEffect();
        }

        return goalObjects;
    }


    // TODO
    // Remove this from the navigation task script
    public override float GetTaskDuration()
    {
        if(!taskStarted)
        {
            return 0;
        }
        else
        {

            //Handle the changes in the state via button press
            switch(buttonState)
            {
                case 0:
                    
                    if(Input.GetKeyDown("space"))
                    {
                        buttonState = 1;
                        isPaused = true;
                        // pausedTime = Time.time;
                        previousTimePaused = Time.time;
                        Debug.Log("Pause");
                    }

                    // Normal Timer
                    //return Time.time - timePausedDuration - startTime;
                    break;
                    
                case 1:
                    

                    if(Input.GetKeyUp("space"))
                    {
                        buttonState = 2;
                    }
                    break;
                case 2:
                    if(Input.GetKeyDown("space"))
                    {
                        buttonState = 3;
                        isPaused = false;
                        previousPauseDuration += Time.time - previousTimePaused;
                        Debug.Log("Resume");
                    }
                    break;
                case 3:
                    if(Input.GetKeyUp("space"))
                    {
                        buttonState = 0;
                    }
                    break;

            }

            float pauseDur = GetPauseDuration();
            // Debug.Log(pauseDur);
            return Time.time - pauseDur - startTime ;
            
        }
    }


    public override bool CheckTaskStart()
    {
        if (robot == null)
        {
            return false;
        }

        // Default - if robot moves more than 0.1m
        if ((robot.transform.position - robotStartPosition).magnitude > 0.1)
        {
            startTime = Time.time;
            pausedTime = startTime;
            taskStarted = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    private float GetPauseDuration()
    {
        if(isPaused)
        {
            return Time.time - previousTimePaused + previousPauseDuration;
        }
        else
        {
            return previousPauseDuration; // should be done when switching from pause to resume;
        }
    }
}