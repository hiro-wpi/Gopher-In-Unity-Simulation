using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO
//      Find a better inplementation for changing the base footprint.
//      Used in Function Update()

public class RobotStateVisualizer : MonoBehaviour
{
    // Difference between the pose of the robot and visualization
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Vector3 rotationEulerOffset;

    // Articulation Bodies
    [SerializeField] private GameObject refRootArticulationBodyGameObject;
    [SerializeField] private GameObject rootArticulationBodyGameObject;
    public ArticulationBody baseAb;
    
    // Articulation Controllers
    public ArticulationChestController chestController;
    public ArticulationJointController leftJointController;
    public ArticulationGripperController leftGripperController;
    public ArticulationJointController rightJointController;
    public ArticulationGripperController rightGripperController;
    public ArticulationCameraController cameraController;


    void Start() 
    {
        positionOffset = Vector3.zero;
        rotationEulerOffset = Vector3.zero;

        if (refRootArticulationBodyGameObject != null)
        {
            updateOffsetWithGameobjects(refRootArticulationBodyGameObject, rootArticulationBodyGameObject);
        }
        

    }

    void Update() {}

    // Move the base to the desired location, based on the sent in Pose
    //      position -> real world coordinates
    //      orientation -> real world coordinates
    public void SetBase(Vector3 targetPosition, Vector3 targetRotation)
    {
        
        baseAb = rootArticulationBodyGameObject.GetComponent<ArticulationBody>();
        if (baseAb != null)
        {
            Vector3 position = targetPosition - positionOffset;
            Vector3 rotation = targetRotation - rotationEulerOffset;

            //baseAb.TeleportRoot(position, Quaternion.Euler(rotation));
            baseAb.TeleportRoot(new Vector3(targetPosition.x, -2f, targetPosition.z), Quaternion.Euler(targetRotation));
        }
        else
        {
            Debug.LogWarning("No Articulation Body Found in GameObject.");
        }
        
            
        // baseAb.velocity = Vector3.zero;
        // baseAb.angularVelocity = Vector3.zero;
        
    }

    public void SetBase_(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        Debug.Log("Using Velocity");
        baseAb = rootArticulationBodyGameObject.GetComponent<ArticulationBody>();
        if (baseAb == null)
        {
            Debug.LogWarning("No Articulation Body Found in GameObject.");
            return;
        }

        baseAb.velocity = linearVelocity;
        baseAb.angularVelocity = angularVelocity;

    }

    // Calc the transform between two different game objects
    public void updateOffsetWithGameobjects(GameObject fromGameObject, GameObject toGameObject)
    {
        // get the transformation from fromGameObject to toGameobject
        // Real - Visulaization
        positionOffset = fromGameObject.transform.position - toGameObject.transform.position;
        rotationEulerOffset = fromGameObject.transform.eulerAngles - toGameObject.transform.eulerAngles;

    }



    public void SetChest(float target)
    {
        chestController.SetPosition(target);
    }

    public void SetLeftArm(float[] targets)
    {
        // We expect to have 7 joints
        if (targets.Length == 7)
        {
            leftJointController.SetJointTargets(targets);
        }
        else
        {
            Debug.LogWarning("We are not sending in the correct amount of joints, " + targets.Length.ToString() + " joint are being sent.");
        }
        
    }
    public void SetRightArm(float[] targets)
    {
        // We expect to have 7 joints
        if (targets.Length == 7)
        {
            rightJointController.SetJointTargets(targets);
        }
        else
        {
            Debug.LogWarning("We are not sending in the correct amount of joints, " + targets.Length.ToString() + " joint are being sent.");
        }
    }

    public void SetCamera(float pitch, float yaw)
    {
        cameraController.SetPitchYawPosition(pitch, yaw);
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

}
