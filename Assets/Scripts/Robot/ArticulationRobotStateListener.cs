using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A class that listens to the state of the robot,
///     in the simulation and visulize the it.
/// </summary>
public class ArticulationRobotStateListener : RobotStateListener
{
    [SerializeField] private StateReader reader;
    [SerializeField] private Localization localization;

    void Start() {}

    // void Update() {}

    // Reads and Stores the Values from the StateReader
    protected override void ReadState() 
    {
        // StateReader not ready
        if (reader.JointNames == null || reader.JointNames.Length == 0)
        {
            return;
        }

        // Extract the base position
        BasePosition = localization.Position;
        BaseOrientationEuler = localization.RotationEuler;

        // Extract the joint names and positions
        JointNames = reader.JointNames;
        JointPositions = reader.JointPositions;
    }

    // Extracts a set of elements from an array
    private float[] SliceArray(int startIndex, int numOfElements)
    {
        float[] sliceJoints = new float[numOfElements];  
        Array.Copy(reader.JointPositions, startIndex, sliceJoints, 0, numOfElements);
        return sliceJoints;
    }
}
