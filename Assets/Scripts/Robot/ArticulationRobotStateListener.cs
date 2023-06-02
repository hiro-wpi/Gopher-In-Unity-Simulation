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

    void Start() {
        StartCoroutine(ReaderInitcialization());
    }

    //void Update() { }

    IEnumerator ReaderInitcialization()
    {
        // Initciallize the StateReader
        while (reader.jointPositions.Length == 0) 
        {
            yield return null;
        }
        isReaderInitcialized = true;

        // Checking to See if the StateListener is connected to the Simulated Robot, not Physical
        Debug.Assert(reader.jointPositions.Length == 23, "Issue with Robot Used, Different then what was exspected! " + reader.jointPositions.Length.ToString() + " joints found");
    }

    // Reads and Stores the Values from the StateReader
    public override void ReadState() 
    {

        // Extract the base position from the StateReader
        basePosition = reader.position;
        baseOrientationEuler = reader.rotationEuler;

        // Extract the target joint positions from the StateReader
        // Reference the StateReader jointnames for what index cooresponds to what joint
        chestPosition = reader.jointPositions[0];
        cameraYawJoint = reader.jointPositions[1]; 
        cameraPitchJoint = reader.jointPositions[2];
        leftArmJointsPosition = SliceArray(3, 7); 
        leftArmGripperPosition = SliceArray(10, 2);
        rightArmJointsPosition = SliceArray(12, 7);
        rightArmGripperPosition = SliceArray(19, 2);

    }

    // Extracts a set of elements from an array
    private float[] SliceArray(int startIndex, int numOfElements)
    {
        float[] sliceJoints = new float[numOfElements];  
        Array.Copy(reader.jointPositions, startIndex, sliceJoints, 0, numOfElements);
        return sliceJoints;
    }

}
