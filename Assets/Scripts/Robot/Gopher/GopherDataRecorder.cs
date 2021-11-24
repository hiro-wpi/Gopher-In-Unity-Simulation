using System.IO;
using System.Linq;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GopherDataRecorder : MonoBehaviour
{   
    // Robot
    public GameObject robot;
    // sensor
    private StateReader stateReader;
    private LaserSocial laser;
    private CollisionReader collisionReader;
    private int collisionStorageIndex;

    // Recorder
    public float updateRate = 10;
    public bool isRecording;
    public float[] states;
    public string[] collisions;

    // CSV writter
    private TextWriter stateTextWriter;
    private TextWriter collisionTextWriter;

    void Start()
    {
        // Initialization
        states = new float[18];
        collisions = new string[2];
        isRecording = false;
        
        setRobot(robot);

        InvokeRepeating("UpdateData", 1f, 1/updateRate);
    }

    void Update()
    {}

    public void setRobot(GameObject robot)
    {
        if (robot == null)
            return;

        // Get sensors
        this.robot = robot;
        stateReader = robot.GetComponentInChildren<StateReader>();
        laser = robot.GetComponentInChildren<LaserSocial>();
        collisionReader = robot.GetComponentInChildren<CollisionReader>();
        collisionStorageIndex = 0;
        
        // Initialization
        states = new float[18];
        collisions = new string[2];

        isRecording = false;
    }

    public void StartRecording(string fileName)
    {
        if (isRecording) return;
        isRecording = true;
        
        // Create writer to write csv files
        string parentFolder = Application.dataPath + "/Data";
        if (!Directory.Exists(parentFolder))
            Directory.CreateDirectory(parentFolder); 
        string name = parentFolder + "/" + fileName;

        stateTextWriter = new StreamWriter(name + " state.csv", false);
        collisionTextWriter = new StreamWriter(name + " collision.csv", false);
    }

    public void StopRecording()
    {
        if (!isRecording) return;
        isRecording = false;

        // stop writers
        stateTextWriter.Close();
        collisionTextWriter.Close();
    }

    private void UpdateData()
    {
        if (robot == null)
        {
            states = new float[18];
            collisions = new string[2];
            return;
        }
        
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
        {
            stateTextWriter.WriteLine(ArrayToCSVLine<float>(states));  
        }

        // Record collision
        if (collisionStorageIndex != collisionReader.storageIndex)
        {
            // collision
            collisions[0] = collisionReader.collisionSelfNames[collisionStorageIndex];
            collisions[1] = collisionReader.collisionOtherNames[collisionStorageIndex];

            collisionStorageIndex = (collisionStorageIndex+1) % collisionReader.storageLength;

            // write to csv
            if (isRecording)
            {
                collisionTextWriter.WriteLine(string.Format("{0:0.000}", states[0]) + "," + 
                                              string.Format("{0:0.000}", states[1]) + "," + 
                                              string.Format("{0:0.000}", states[2]) + "," + 
                                              ArrayToCSVLine<string>(collisions));
            }
        }
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
        line.Remove(line.Length - 1);
        return line;
    }

    private float ToFLUEuler(float angle)
    {
        float twoPI = 2 * Mathf.PI;

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
        if (ranges.Length == 0) 
            return 0;

        // Get smallest index of laser scan
        float minValue = ranges.Min();
        int minIndex = ranges.ToList().IndexOf(minValue);
        return minIndex;
    }
}
