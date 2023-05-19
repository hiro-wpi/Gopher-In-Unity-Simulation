using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script is used to control the robotic end-effector. 
///     with simple two-finger prismatics grippers.
/// </summary>
public class ArticulationGripperController : MonoBehaviour
{
    // Assume two-finger prismatic joints
    [SerializeField] private ArticulationBody leftFinger;
    [SerializeField] private ArticulationBody rightFinger;
    private bool gripperClosed = false;

    // Grasping
    [SerializeField] private Grasping grasping;
    private ArticulationCollisionDetection leftCollision;
    private ArticulationCollisionDetection rightCollision;
    // grasping affects wheel velocity (if wheel controller attached)
    [SerializeField] private ArticulationBaseController baseController;
    [SerializeField] private string wheelSpeedLimitID = "grasping limit";

    void Start() 
    { 
        // Init grasping collision detection
        leftCollision = leftFinger.gameObject.
            GetComponentInChildren<ArticulationCollisionDetection>();
        rightCollision = rightFinger.gameObject.
            GetComponentInChildren<ArticulationCollisionDetection>();
    }

    void FixedUpdate()
    {
        // Graspable object detection
        CheckGrasping();
    }

    // Gripper position control
    public void SetGripper(float position)
    {
        // Ignore the actual value
        if (position > 0.5)
        {
            CloseGripper();
        }
        else
        {
            OpenGripper();
        }
    }

    public void CloseGripper()
    {
        // Close gripper
        SetGripperTarget(1.0f);
        gripperClosed = true;
    }

    public void OpenGripper()
    {
        // Open gripper
        SetGripperTarget(0.0f);
        gripperClosed = false;

        // Release grasping object
        grasping.Detach();
        // Resume speed if wheel controller attached
        baseController?.RemoveSpeedLimit(wheelSpeedLimitID);
    }

    public void ChangeGripperStatus()
    {
        if (gripperClosed)
        {
            OpenGripper();
        }
        else
        {
            CloseGripper();
        }
    }

    public void StopGripper()
    {
        ArticulationBodyUtils.StopJoint(leftFinger);
        ArticulationBodyUtils.StopJoint(rightFinger);
    }

    private void SetGripperTarget(float value)
    {
        // Get absolute position values
        float leftValue = Mathf.Lerp(
            leftFinger.xDrive.lowerLimit, leftFinger.xDrive.upperLimit, value
        );
        float rightValue = Mathf.Lerp(
            rightFinger.xDrive.lowerLimit, rightFinger.xDrive.upperLimit, value
        );

        // Set values
        ArticulationBodyUtils.SetJointTarget(leftFinger, leftValue);
        ArticulationBodyUtils.SetJointTarget(rightFinger, rightValue);
    }

    // Grasping detection
    private void CheckGrasping()
    {
        // Check only when gripper is closed 
        // but object is not yet grasped
        if (!gripperClosed || grasping.IsGrasping)
        {
            return;
        }
            
        // If both fingers in touch with the same graspable object
        if ((leftCollision.CollidingObject != null) && 
            (rightCollision.CollidingObject != null) &&
            (leftCollision.CollidingObject == rightCollision.CollidingObject) &&           
            (leftCollision.CollidingObject.tag == "GraspableObject"))
        {
            // Attach object to the tool frame
            grasping.Attach(leftCollision.CollidingObject);
            
            // Slow down wheel based on the object mass
            if (baseController != null)
            {
                float speedLimit = 1f - 0.1f * grasping.GetGraspedObjectMass();
                speedLimit = Mathf.Clamp(speedLimit, 0.1f, 1f);
                baseController.AddSpeedLimit(
                    new float[] { speedLimit, speedLimit, speedLimit, speedLimit }, 
                    wheelSpeedLimitID
                );
            }
        }
    }

    // Get grasping status
    public float GetGraspedObjectMass()
    {
        return grasping.GetGraspedObjectMass();
    }
}
