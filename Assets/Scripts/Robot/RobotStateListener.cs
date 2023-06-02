using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>

/// An abstract class that focuses on Listening to the state of the robot,
/// and interacting with the robot visualization. 

/// </summary>
public abstract class RobotStateListener : MonoBehaviour
{

    [SerializeField] private RobotStateVisualizer visualizer;

    // Target Positions
    public Vector3 basePosition;
    public Vector3 baseOrientationEuler;
    public float chestPosition;
    public float[] leftArmJointsPosition;
    public float[] rightArmJointsPosition;
    public float[] leftArmGripperPosition;
    public float[] rightArmGripperPosition;
    public float cameraPitchJoint;
    public float cameraYawJoint;

    // Flags
    public bool isReaderInitcialized = false;

    void Start() {}

    void Update() 
    {
        if (!isReaderInitcialized)
        {
            return;
        }

        ReadState();
        UpdateVisualization();
    }

    public void UpdateVisualization()
    {
        visualizer.SetBase(basePosition, baseOrientationEuler);
        visualizer.SetChest(chestPosition);
        visualizer.SetLeftArm(leftArmJointsPosition);
        visualizer.SetRightArm(rightArmJointsPosition);
        visualizer.SetLeftGripper(leftArmGripperPosition);
        visualizer.SetRightGripper(rightArmGripperPosition);
        visualizer.SetCamera(cameraPitchJoint, cameraYawJoint);
    }

    // Reads and Stores the target positions from the StateReader
    public virtual void ReadState() {}

}
