using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataRecorder : MonoBehaviour
{
    public GameObject robot;
    private StateReader stateReader;
    private LaserSocial laser;
    private CollisionReader collisionReader;
    private int collisionStorageIndex;

    public float recordRate = 10;
    public bool updateData;
    private bool isRecording;
    public float[] states;
    public string[] collisions;
    public float[] task;

    private TextWriter stateTextWriter;
    private TextWriter collisionTextWriter;

    private float twoPI;

    void Start()
    {
        // Initialization
        states = new float[18];
        collisions = new string[2];
        task = new float[12]; 
        isRecording = false;
        updateData = false;
        if (robot != null)
            setRobot(robot);

        // Constant
        twoPI = 2 * Mathf.PI;

        InvokeRepeating("RecordData", 1f, 1/recordRate);
    }

    void Update()
    {
    }

    public void setRobot(GameObject robot)
    {
        this.robot = robot;
        stateReader = robot.GetComponentInChildren<StateReader>();
        laser = robot.GetComponentInChildren<LaserSocial>();
        collisionReader = robot.GetComponentInChildren<CollisionReader>();
        collisionStorageIndex = 0;
        
        states = new float[18];
        collisions = new string[2];
        task = new float[12]; 

        isRecording = false;
        updateData = false;
    }

    public void StartRecording(string indexNumber)
    {
        isRecording = true;

        string parentFolder = Application.dataPath + "/Data";
        if (!Directory.Exists(parentFolder))
            Directory.CreateDirectory(parentFolder); 
        string name = parentFolder + "/" + indexNumber + 
                      " " + System.DateTime.Now.ToString("MM-dd HH-mm-ss");

        stateTextWriter = new StreamWriter(name + " state.csv", false);
        collisionTextWriter = new StreamWriter(name + " collision.csv", false);
    }

    public void StopRecording()
    {
        stateTextWriter.Close();
        collisionTextWriter.Close();
        isRecording = false;
    }

    private void RecordData()
    {
        if (robot == null)
        {
            states = new float[18];
            collisions = new string[2];
        }
        if(!isRecording && !updateData)
            return;
        
        // Record state
        // t
        states[0] = Time.time;
        // pose
        states[1] = stateReader.position[2];
        states[2] = -stateReader.position[0];
        states[3] = ToFLUEuler(stateReader.eulerRotation[1] * Mathf.Deg2Rad);
        // vel
        states[4] = stateReader.linearVelocity[2];
        states[5] = -stateReader.angularVelocity[1];
        // obs dis
        int obsMinI = GetLaserMinIndex(laser.obstacleRanges);
        int humMinI = GetLaserMinIndex(laser.humanRanges);
        states[6] = laser.obstacleRanges[obsMinI];
        states[7] = laser.directions[obsMinI];
        states[8] = laser.humanRanges[humMinI];
        states[9] = laser.directions[humMinI];
        // main camera joint
        states[10] = stateReader.positions[2];
        states[11] = ToFLUEuler(stateReader.positions[3]);
        states[12] = stateReader.velocities[2];
        states[13] = -stateReader.velocities[3];
        // arm camera joint
        states[14] = stateReader.positions[22];
        states[15] = ToFLUEuler(stateReader.positions[21]);
        states[16] = stateReader.velocities[22];
        states[17] = -stateReader.velocities[21];

        // write to csv
        if (isRecording)
            stateTextWriter.WriteLine(ArrayToCSVLine(states));

        // Record collision
        if (collisionStorageIndex != collisionReader.storageIndex)
        {
            // collision
            collisions[0] = collisionReader.collisionSelfNames[collisionStorageIndex];
            collisions[1] = collisionReader.collisionOtherNames[collisionStorageIndex];

            collisionStorageIndex = (collisionStorageIndex+1) % collisionReader.storageLength;

            // write to csv
            if (isRecording)
                collisionTextWriter.WriteLine(string.Format("{0:0.000}", states[0]) + "," + 
                                              string.Format("{0:0.000}", states[1]) + "," + 
                                              string.Format("{0:0.000}", states[2]) + "," + 
                                              ArrayToCSVLine(collisions));
        }
    }

    private string ArrayToCSVLine(float[] array)
    {
        string line = "";
        foreach (float value in array)
        {
            line += string.Format("{0:0.000}", value) + ",";
        }
        line.Remove(line.Length - 1);
        return line;
    }
    private string ArrayToCSVLine(string[] array)
    {
        string line = "";
        foreach (string value in array)
        {
            line += value + ",";
        }
        line.Remove(line.Length - 1);
        return line;
    }

    private float ToFLUEuler(float angle)
    {
        // Change direction
        angle = twoPI - angle;

        // Wrap to [-pi to pi]
        angle =  angle % twoPI; 
        // positive remainder, 0 <= angle < 2pi  
        angle = (angle + twoPI) % twoPI;
        
        return angle;
    }

    private int GetLaserMinIndex(float[] ranges)
    {
        int index = 0;
        float min = 100f;
        for(int i = 0; i < ranges.Length; ++i)
            if (ranges[i] != 0f && ranges[i] < min)
            {
                min = ranges[i];
                index = i;
            }
        return index;
    }
}
