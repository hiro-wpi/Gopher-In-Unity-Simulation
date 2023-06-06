using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 
// Handles controlling a visualization of the Gopher Robot. 
// We assume that this should be seperate from the simulation.
//

public class RobotStateVisualizer : MonoBehaviour
{
    // Visulaization Placement in the World
    [SerializeField] private float verticalPosition;

    // Articulation Bodies
    [SerializeField] private GameObject rootArticulationBodyGameObject;
    [SerializeField] private ArticulationBody baseAb;
    
    // Articulation Controllers
    public ArticulationChestController chestController;
    public ArticulationJointController leftJointController;
    public ArticulationJointController rightJointController;
    public ArticulationGripperController leftGripperController;
    public ArticulationGripperController rightGripperController;
    public ArticulationCameraController cameraController;

    void Start() 
    {
        //Get the vertical placement of the visual in the Scene
        verticalPosition = rootArticulationBodyGameObject.transform.position.y;
    }

    void Update() {}

    // Set Functions for Controlling the Robot Visulaization

    public void SetBase(Vector3 targetPosition, Vector3 targetRotation)
    {
        baseAb = rootArticulationBodyGameObject.GetComponent<ArticulationBody>();
        if (baseAb != null)
        {
            // Move the base directly to the given pose
            baseAb.TeleportRoot(new Vector3(targetPosition.x, verticalPosition, targetPosition.z), Quaternion.Euler(targetRotation));
        }
    }

    public void SetChest(float target)
    {
        chestController.SetPosition(target);
    }

    public void SetLeftArm(float[] targets)
    {
        // Only Set Joints if we recieve all the joint positions, ignore otherwise
        if (targets.Length == 7)
        {
            leftJointController.SetJointTargets(targets);
        }     
    }
    public void SetRightArm(float[] targets)
    {
        // Only Set Joints if we recieve all the joint positions, ignore otherwise
        if (targets.Length == 7)
        {
            rightJointController.SetJointTargets(targets);
        }
    }

    public void SetLeftGripper(float[] targets)
    {
        if (targets.Length == 2)
        { 
            leftGripperController.SetGripperJointTarget(targets[0], targets[1]);
        }
    }

    public void SetRightGripper(float[] targets)
    {
        if (targets.Length == 2)
        {
            rightGripperController.SetGripperJointTarget(targets[0], targets[1]);
        }
    }

    public void SetCamera(float pitch, float yaw)
    {
        cameraController.SetPitchYawPosition(pitch, yaw);
    }

}
