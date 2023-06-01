using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO
//      Find a better inplementation for changing the base footprint.
//      Used in Function Update()

public class RobotStateVisualizer : MonoBehaviour
{
    // Articulation Bodies
    [SerializeField] private ArticulationBody baseAb;
    
    // Articulation Controllers
    // Right Gripper
    // Left Gripper
    public ArticulationChestController chestController;
    public ArticulationJointController leftJointController;
    public ArticulationJointController rightJointController;
    public ArticulationCameraController cameraController;


    void Start() {}

    void Update() {}

    public void SetBase(Vector3 position, Vector3 orientation)
    {
        baseAb = GetComponent<ArticulationBody>();
        baseAb.TeleportRoot(new Vector3(0.0f, 10.0f, 0.0f), Quaternion.identity);
        baseAb.velocity = Vector3.zero;
        baseAb.angularVelocity = Vector3.zero;
        
    }

    public void SetChest(float target)
    {
        chestController.SetPosition(target);
    }

    public void SetLeftArm(float[] targets)
    {
        leftJointController.SetJointTargets(targets);
    }
    public void SetRightArm(float[] targets)
    {
        rightJointController.SetJointTargets(targets);
    }

    public void SetCamera(float pitch, float yaw)
    {
        cameraController.SetPitchYawPosition(pitch, yaw);
    }
    
}
