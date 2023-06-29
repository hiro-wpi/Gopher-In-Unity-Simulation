using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.UrdfImporter;

/// <summary>
//      Visuazlize the state of the robot in the scene.
/// </summary>
public class RobotStateVisualizer : MonoBehaviour
{
    // Robot
    [SerializeField] private RobotStateListener robotStateListener;
    [SerializeField] private GameObject robotModel;

    private ArticulationBody robotBase;
    private Dictionary<string, ArticulationBody> jointDict = new();

    // Visulaization vertical offset in the World
    // This could be used to avoid overlapping with the simulated robot
    [SerializeField] private float verticalOffset;

    void Start() 
    {
        // Get the base (assumed to be the first joint)
        robotBase = robotModel.GetComponentInChildren<ArticulationBody>();

        // Get the articulation body chain
        UrdfJoint[] jointChain = robotModel.GetComponentsInChildren<UrdfJoint>();
        jointChain = jointChain.Where( 
           joint => joint.JointType != UrdfJoint.JointTypes.Fixed
        ).ToArray();
        int jointStateLength = jointChain.Length;

        // Build a dict to store the joint names and the corresponding articulation body
        jointDict = new Dictionary<string, ArticulationBody>();
        for (int i = 0; i < jointStateLength; i++)
        {
            jointDict.Add(
                jointChain[i].jointName, 
                jointChain[i].GetComponent<ArticulationBody>()
            );
        }
    }

    void Update() 
    {
        robotStateListener.ReadState();

        SetPose(
            robotStateListener.BasePosition, 
            robotStateListener.BaseOrientationEuler
        );
        SetJoints(
            robotStateListener.JointNames, 
            robotStateListener.JointPositions
        );
    }

    // Set Functions for Controlling the Robot Visulaization
    public void SetPose(Vector3 targetPosition, Vector3 targetRotation)
    {
        // Move the base directly to the given pose
        robotBase.TeleportRoot(
            targetPosition + new Vector3(0, verticalOffset, 0),
            Quaternion.Euler(targetRotation)
        );
    }

    public void SetJoints(string[] names, float[] targets)
    {
        // check to make sure the length of the list for each is the same
        Debug.Assert(names.Length == targets.Length,
            "The length of the joints' names and targets array are not the same"
        );

        // For each joint, set the target
        float target;
        for (int i = 0; i < targets.Length; i++)
        {
            // if the given joint name exists
            if (jointDict.TryGetValue(names[i], out ArticulationBody joint))
            {
                target = targets[i];
                // set joint target
                if (joint.jointType == ArticulationJointType.RevoluteJoint)
                {
                    target *= Mathf.Rad2Deg;
                }
                ArticulationBodyUtils.SetJointTarget(joint, target);
            }
        }
    }
}
