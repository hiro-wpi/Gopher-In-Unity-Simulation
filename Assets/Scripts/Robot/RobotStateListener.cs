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

    // Base
    public Vector3 basePosition;
    public Vector3 baseOrientationEuler;

    public Vector3 linearVelocity;
    public Vector3 angularVelocity;

    // Chest
    public float chestPosition;

    // LeftArm
    public float[] leftArmJointsPosition;

    // RightArm
    public float[] rightArmJointsPosition;

    // Camera
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
        //visualizer.SetBase_(linearVelocity, angularVelocity);
        visualizer.SetChest(chestPosition);
        visualizer.SetLeftArm(leftArmJointsPosition);
        visualizer.SetRightArm(rightArmJointsPosition);
        visualizer.SetCamera(cameraPitchJoint, cameraYawJoint);
    }

    public virtual void ReadState() {}

}
