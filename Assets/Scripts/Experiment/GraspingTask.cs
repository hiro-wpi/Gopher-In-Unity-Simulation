using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining grasping task.
/// </summary>
public class GraspingTask : Task 
{
    void Start()
    {}


    public override bool CheckTaskCompletion()
    {
        if (robot == null)
            return false;
        // Check if all task objects reach the neighbor of the current goal
        foreach (GameObject taskObject in taskObjects)
        {
            if (!goals[0].CheckIfObjectReachedGoal(taskObject))
                return false;
        }
        goals[0].DisableGoalVisualEffect();
        gUI.ShowPopUpMessage("Current Task Completed!");
        return true;
    }


    public override string GetTaskStatus()
    {
        if (goalIndex == goals.Length)
            return "The task is completed.";
        
        float distance = goals[goalIndex].GetDistanceToGoal(taskObjects[0]);
        return "The object is " + 
                string.Format("{0:0.000}", distance) + " m" + "\n" +
                "away from the goal.";
    }
    

    public override string[] GetTaskValueToRecordHeader()
    {
        // Header of task objects transform
        valueToRecordHeader = new string[6 * taskObjects.Length];
        for (int i = 0; i < taskObjects.Length; ++i)
        {
            string objectName = taskObjects[i].name;
            valueToRecordHeader[6*i+0] = objectName + "_x";
            valueToRecordHeader[6*i+1] = objectName + "_y";
            valueToRecordHeader[6*i+2] = objectName + "_z";
            valueToRecordHeader[6*i+3] = objectName + "_ax";
            valueToRecordHeader[6*i+4] = objectName + "_ay";
            valueToRecordHeader[6*i+5] = objectName + "_az";
        }

        return valueToRecordHeader;
    }

    public override float[] GetTaskValueToRecord()
    {
        // Record task objects transform
        valueToRecord = new float[6 * taskObjects.Length];
        for (int i = 0; i < taskObjects.Length; ++i)
        {
            Vector3 position = Utils.ToFLU(taskObjects[0].transform.position);
            Vector3 rotation = Mathf.Deg2Rad * 
                               Utils.ToFLU(taskObjects[0].transform.rotation).eulerAngles;
            valueToRecord[6*i+0] = position.x;
            valueToRecord[6*i+1] = position.y;
            valueToRecord[6*i+2] = position.z;
            valueToRecord[6*i+3] = rotation.x;
            valueToRecord[6*i+4] = rotation.y;
            valueToRecord[6*i+5] = rotation.z;
        }

        return valueToRecord;
    }
}