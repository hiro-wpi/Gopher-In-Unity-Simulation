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

    
    void Start() {
        while(reader.jointNames.Length == 0) {} // wait until the StateReader has joints
        Debug.Assert(reader.jointPositions.Length == 23, "Issue with Robot Used, Different then what was exspected!");
        joints = reader.jointPositions;

    }

    void Update() {}

    public override void ReadState() 
    {
        basePosition = reader.position;
        baseOrientationEuler = reader.rotationEuler;

        // 0  -> chest
        chestPosition = joints[0];

        // 1  -> camera yaw
        cameraYawJoint = joints[1];
        // 2  -> camera pitch
        cameraPitchJoint = joints[2];
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
        Array.Copy(joints, startElement, sliceJoints, 0, numOfElements);
        return sliceJoints;
    }

}
