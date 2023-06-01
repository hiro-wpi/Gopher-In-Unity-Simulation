using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>

/// An abstract class that focuses on Listening to the state of the robot,
/// and interacting with the robot visualization. 

/// </summary>
public class ArticulationRobotStateListener : RobotStateListener
{
    [SerializeField] private StateReader reader;
    [SerializeField] private float[] joints;

    // Flags
    // private bool isReaderInitcialized = false;



    void Start() {
        StartCoroutine(ReaderInitcialization());
    }

    IEnumerator ReaderInitcialization()
    {
        Debug.Log("Init Reader");
        while (reader.jointPositions.Length == 0) 
        {
            yield return null;
        }
        isReaderInitcialized = true;
        Debug.Assert(reader.jointPositions.Length == 23, "Issue with Robot Used, Different then what was exspected! " + reader.jointPositions.Length.ToString() + " joints found");
        joints = reader.jointPositions;
        UpdateVisualization();
    }

    void Update() 
    {
        if (!isReaderInitcialized)
        {
            return;
        }

        ReadState();
        UpdateVisualization();

    }

    public override void ReadState() 
    {

        //Debug.Log("ReadState");
        basePosition = reader.position;
        baseOrientationEuler = reader.rotationEuler;

        linearVelocity = reader.linearVelocity;
        angularVelocity = reader.angularVelocity;

        // 0  -> chest
        chestPosition = reader.jointPositions[0];

        // 1  -> camera yaw
        cameraYawJoint = reader.jointPositions[1];
        // 2  -> camera pitch
        cameraPitchJoint = reader.jointPositions[2];
        // 3  -> left arm
        leftArmJointsPosition = SliceArray(3, 7);
        // 10 -> left gripper
        // 12 -> right arm
        rightArmJointsPosition = SliceArray(12, 7);
        // 19 -> right gripper
    
    }

    private float[] SliceArray(int startElement, int numOfElements)
    {
        float[] sliceJoints = new float[numOfElements];  
        Array.Copy(reader.jointPositions, startElement, sliceJoints, 0, numOfElements);
        return sliceJoints;
    }

}
