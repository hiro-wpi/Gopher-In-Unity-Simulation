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

    private bool taskEnd = false;
    
    private void Update()
    {
        if (!taskEnd)
        {
            if (graspingTask.CheckTaskCompletion())
            {
                taskEnd = true;
                RecordData();
            }
        }
    }

    void OnDestroy()
    {
        RecordData();
    }

    private void RecordData()
    {
        if (!isDataRecorded)
        {
            isDataRecorded = true;
            dataString = "Completion Time, Number of Collisions\n" + graspingTask.GetTaskDuration() + "," + robotCollisionWarning.collisionCounter;
            Debug.Log(dataString);


            textWriter = new StreamWriter(fileName + "_manipulation_task.csv", false);
            textWriter.WriteLine(dataString);
            textWriter.Close();

            textWriter = new StreamWriter(fileName + "_manipulation_task_collision.csv", false);
            textWriter.WriteLine(robotCollisionWarning.collisionReport);
            textWriter.Close();
        }
    }
}
