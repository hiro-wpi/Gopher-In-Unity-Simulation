using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for recording the experiment metrics.
/// Data of robots to record is defined in this script.
/// Extra data of task can also be defined and recorded by writing
/// task.robotValueToRecord and task.stringToRecord1.
/// </summary>
public class DataRecorder : MonoBehaviour
{
    // General
    public float updateRate = 10;
    public bool isRecording;

    // Robot
    private GameObject robot;
    private StateReader stateReader;
    private SurroundingDetection surroundingDetection;
    private CollisionReader collisionReader;
    private int collisionStorageIndex;
    // Task
    private Task task;

    // CSV writter
    private TextWriter robotValueTextWriter;
    private TextWriter robotStringTextWriter;
    private TextWriter taskValueTextWriter;
    private TextWriter taskStringTextWriter;
    // container - robot
    private string[] robotValueToRecordHeader;
    private string[] robotStringToRecordHeader;
    private float[] robotValueToRecord;
    private string[] robotStringToRecord;
    // container - task
    private string[] taskValueToRecordHeader;
    private string[] taskStringToRecordHeader;
    private float[] taskValueToRecord;
    private string[] taskStringToRecord;
    

    void Start()
    {
        // Initialization
        isRecording = false;
        updateRate = 10;

        // 6 + 12 * 2 + 2 + 14 * 2
        robotValueToRecordHeader = new string[] 
        {
            "time", 
            "x", "y", "az", "vx", "vaz", 

            "dis_to_obs_0", "dis_to_obs_36", "dis_to_obs_72", "dis_to_obs_108", 
            "dis_to_obs_144", "dis_to_obs_180", "dis_to_obs_-144", "dis_to_obs_-108", 
            "dis_to_obs_-72", "dis_to_obs_-36",
            "dis_to_min_obs", "direction_to_min_obs", 

            "dis_to_human_0", "dis_to_human_36", "dis_to_human_72", "dis_to_human_108", 
            "dis_to_human_144", "dis_to_human_180", "dis_to_human_-144", "dis_to_human_-108", 
            "dis_to_human_-72", "dis_to_human_-36",
            "dis_to_min_human", "direction_to_min_human", 
            
            "main_cam_yaw", "main_cam_pitch",

            "left_joint_1", "left_joint_2", "left_joint_3", "left_joint_4", 
            "left_joint_5", "left_joint_6", "left_joint_7", "left_joint_gripper", 
            "left_end_x", "left_end_y", "left_end_z", 
            "left_end_ax", "left_end_ay", "left_end_az", 

            "right_joint_1", "right_joint_2", "right_joint_3", "right_joint_4", 
            "right_joint_5", "right_joint_6", "right_joint_7", "right_joint_gripper", 
            "right_end_x", "right_end_y", "right_end_z", 
            "right_end_ax", "right_end_ay", "right_end_az"
        };
        robotStringToRecordHeader = new string[] {"time", "self_name", "other_name"};
    }
    
    // Start a new recording
    public void StartRecording(string fileName, GameObject[] robots, Task task)
    {
        // Stop previous recording if any
        if (isRecording)
            StopRecording();
    
        // Create new files
        robotValueTextWriter = new StreamWriter(fileName + "_robot_value.csv", false);
        robotStringTextWriter = new StreamWriter(fileName + "_robot_string.csv", false);
        taskValueTextWriter = new StreamWriter(fileName + "_task_value.csv", false);
        taskStringTextWriter = new StreamWriter(fileName + "_task_string.csv", false);

        // Start getting data
        // from robot
        this.robot = robots[0];
        stateReader = robot.GetComponentInChildren<StateReader>();
        surroundingDetection = robot.GetComponentInChildren<SurroundingDetection>();
        collisionReader = robot.GetComponentInChildren<CollisionReader>();
        collisionStorageIndex = 0;
        // from task
        this.task = task;
        taskValueToRecordHeader = task.GetTaskValueToRecordHeader();
        taskStringToRecordHeader = task.GetTaskStringToRecordHeader();

        // Headers
        robotValueTextWriter.WriteLine(ArrayToCSVLine<string>(robotValueToRecordHeader));
        robotStringTextWriter.WriteLine(ArrayToCSVLine<string>(robotStringToRecordHeader));
        taskValueTextWriter.WriteLine(ArrayToCSVLine<string>(taskValueToRecordHeader));  
        taskStringTextWriter.WriteLine(ArrayToCSVLine<string>(taskStringToRecordHeader));

        // Start
        isRecording = true;
        InvokeRepeating("WriteData", 1f, 1/updateRate);
    }
    // Get data from the task
    private void WriteData()
    {
        // General robot data
        UpdateRobotData();
        if (robotValueToRecord != null && robotValueToRecord.Length > 0)
            robotValueTextWriter.WriteLine(ArrayToCSVLine<float>(robotValueToRecord));
        if (robotStringToRecord != null && robotStringToRecord.Length > 0)
            robotStringTextWriter.WriteLine(ArrayToCSVLine<string>(robotStringToRecord));

        // Task specified data
        UpdateTaskData();
        if (taskValueToRecord != null && taskValueToRecord.Length > 0)
            taskValueTextWriter.WriteLine(ArrayToCSVLine<float>(taskValueToRecord));
        if (taskStringToRecord != null && taskStringToRecord.Length > 0)
            taskStringTextWriter.WriteLine(ArrayToCSVLine<string>(taskStringToRecord));
    }
    
    // Stop current recording
    public void StopRecording()
    {
        // If not recording
        if (!isRecording) 
            return;
    
        // Cancel getting data
        CancelInvoke("UpdateRobotData");
        CancelInvoke("WriteData");
        // Stop writers and save files
        robotValueTextWriter.Close();
        robotStringTextWriter.Close();
        taskValueTextWriter.Close();
        taskStringTextWriter.Close();

        isRecording = false;
    }
    

    // Data from task
    void UpdateTaskData()
    {
        taskValueToRecord = task.GetTaskValueToRecord();
        taskStringToRecord = task.GetTaskStringToRecord();
    }

    // Data from robot
    void UpdateRobotData()
    {
        if (robot == null)
            return;
    
        robotValueToRecord = new float[robotValueToRecordHeader.Length];
        // Record state
        // t
        robotValueToRecord[0] = Time.time;
        // pose
        robotValueToRecord[1] = stateReader.position[2];
        robotValueToRecord[2] = -stateReader.position[0];
        robotValueToRecord[3] = ToFLUEuler(stateReader.rotationEuler[1] * Mathf.Deg2Rad);
        // vel
        robotValueToRecord[4] = stateReader.linearVelocity[2];
        robotValueToRecord[5] = ToFLUEuler(stateReader.angularVelocity[1]);
        
        // obs dis
        robotValueToRecord[6] = surroundingDetection.obstacleRanges[89];
        robotValueToRecord[7] = surroundingDetection.obstacleRanges[71];
        robotValueToRecord[8] = surroundingDetection.obstacleRanges[53];
        robotValueToRecord[9] = surroundingDetection.obstacleRanges[35];
        robotValueToRecord[10] = surroundingDetection.obstacleRanges[17];
        robotValueToRecord[11] = surroundingDetection.obstacleRanges[179];
        robotValueToRecord[12] = surroundingDetection.obstacleRanges[161];
        robotValueToRecord[13] = surroundingDetection.obstacleRanges[143];
        robotValueToRecord[14] = surroundingDetection.obstacleRanges[125];
        robotValueToRecord[15] = surroundingDetection.obstacleRanges[107];
        int obsMinI = Utils.ArgMinArray(surroundingDetection.obstacleRanges);
        robotValueToRecord[16] = surroundingDetection.obstacleRanges[obsMinI];
        robotValueToRecord[17] = ToFLUEuler(surroundingDetection.directions[obsMinI]);

        // human dis
        robotValueToRecord[18] = surroundingDetection.humanRanges[89];
        robotValueToRecord[19] = surroundingDetection.humanRanges[71];
        robotValueToRecord[20] = surroundingDetection.humanRanges[53];
        robotValueToRecord[21] = surroundingDetection.humanRanges[35];
        robotValueToRecord[22] = surroundingDetection.humanRanges[17];
        robotValueToRecord[23] = surroundingDetection.humanRanges[179];
        robotValueToRecord[24] = surroundingDetection.humanRanges[161];
        robotValueToRecord[25] = surroundingDetection.humanRanges[143];
        robotValueToRecord[26] = surroundingDetection.humanRanges[125];
        robotValueToRecord[27] = surroundingDetection.humanRanges[107];
        int humMinI = Utils.ArgMinArray(surroundingDetection.humanRanges);
        robotValueToRecord[28] = surroundingDetection.humanRanges[humMinI];
        robotValueToRecord[29] = ToFLUEuler(surroundingDetection.directions[humMinI]);

        // main camera joint
        robotValueToRecord[30] = ToFLUEuler(stateReader.jointPositions[2]);
        robotValueToRecord[31] = ToFLUEuler(stateReader.jointPositions[3]);

        // left joints
        robotValueToRecord[32] = ToFLUEuler(stateReader.jointPositions[4]);
        robotValueToRecord[33] = ToFLUEuler(stateReader.jointPositions[5]);
        robotValueToRecord[34] = ToFLUEuler(stateReader.jointPositions[6]);
        robotValueToRecord[35] = ToFLUEuler(stateReader.jointPositions[7]);
        robotValueToRecord[36] = ToFLUEuler(stateReader.jointPositions[8]);
        robotValueToRecord[37] = ToFLUEuler(stateReader.jointPositions[9]);
        robotValueToRecord[38] = ToFLUEuler(stateReader.jointPositions[10]);
        robotValueToRecord[39] = stateReader.jointPositions[11]; // gripper
        // left end effector
        Vector3 leftPosition = Utils.ToFlu(stateReader.objectPositions[0]);
        Vector3 leftRotation = Mathf.Deg2Rad * 
                               Utils.ToFlu(Quaternion.Euler(
                                           stateReader.objectRotations[0])).eulerAngles;
        robotValueToRecord[40] = leftPosition.x;
        robotValueToRecord[41] = leftPosition.y;
        robotValueToRecord[42] = leftPosition.z;
        robotValueToRecord[43] = leftRotation.x;
        robotValueToRecord[44] = leftRotation.y;
        robotValueToRecord[45] = leftRotation.z;

        // right joints
        robotValueToRecord[46] = ToFLUEuler(stateReader.jointPositions[13]);
        robotValueToRecord[47] = ToFLUEuler(stateReader.jointPositions[14]);
        robotValueToRecord[48] = ToFLUEuler(stateReader.jointPositions[15]);
        robotValueToRecord[49] = ToFLUEuler(stateReader.jointPositions[16]);
        robotValueToRecord[50] = ToFLUEuler(stateReader.jointPositions[17]);
        robotValueToRecord[51] = ToFLUEuler(stateReader.jointPositions[18]);
        robotValueToRecord[52] = ToFLUEuler(stateReader.jointPositions[19]);
        robotValueToRecord[53] = stateReader.jointPositions[20]; // gripper
        // right end effector
        Vector3 rightPosition = Utils.ToFlu(stateReader.objectPositions[1]);
        Vector3 rightRotation = Mathf.Deg2Rad * 
                                Utils.ToFlu(Quaternion.Euler(
                                            stateReader.objectRotations[1])).eulerAngles;
        robotValueToRecord[54] = rightPosition.x;
        robotValueToRecord[55] = rightPosition.y;
        robotValueToRecord[56] = rightPosition.z;
        robotValueToRecord[57] = rightRotation.x;
        robotValueToRecord[58] = rightRotation.y;
        robotValueToRecord[59] = rightRotation.z;

        // Record collision
        robotStringToRecord = null; // don't record when there is no new collision
        if (collisionStorageIndex != collisionReader.storageIndex)
        {
            robotStringToRecord = new string[3];
            // collision
            robotStringToRecord[0] = string.Format("{0:0.000}", robotValueToRecord[0]);
            robotStringToRecord[1] = collisionReader.collisionSelfNames[collisionStorageIndex];
            robotStringToRecord[2] = collisionReader.collisionOtherNames[collisionStorageIndex];

            collisionStorageIndex = (collisionStorageIndex+1) % collisionReader.storageLength;
        }
    }


    // Utils
    private float ToFLUEuler(float angle)
    {
        float twoPI = 2 * Mathf.PI;
        // Change direction
        angle = twoPI - angle;
        // Wrap to [0, 2pi)
        angle = angle % twoPI;
        return angle;
    }

    private string ArrayToCSVLine<T>(T[] array)
    {
        string line = "";
        // Add value to line
        foreach (T value in array)
        {
            if (value is float || value is int)
                line += string.Format("{0:0.000}", value) + ",";
            else if (value is string)
                line += value + ",";
        }
        // Remove "," in the end
        if (line.Length > 0)
            line.Remove(line.Length - 1);
        return line;
    }
}
