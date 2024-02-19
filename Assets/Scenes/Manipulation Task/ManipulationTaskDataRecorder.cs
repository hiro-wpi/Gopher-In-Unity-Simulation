using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulationTaskDataRecorder : MonoBehaviour
{
    [SerializeField] private RobotCollisionWarning robotCollisionWarning;
    [SerializeField] private GraspingTask graspingTask;
    bool isDataRecorded = false;
    private string dataString = "";
    public string fileName;
    private TextWriter textWriter;
    
    private void Update()
    {
        // if the task is completed, print the task duration and the number of collisions
        if (!isDataRecorded && graspingTask.CheckTaskCompletion())
        {
            isDataRecorded = true;
            dataString = "Completion Time: " + graspingTask.taskDuration + " seconds, Number of Collisions: " + robotCollisionWarning.collisionCounter;
            Debug.Log(dataString);
            textWriter = new StreamWriter(fileName + "_manipulation_task.csv", false);
            textWriter.WriteLine(dataString);
            textWriter.Close();
        }
    }
}
