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

    // Grasping
    [SerializeField] private Grasping grasping;
    private ArticulationCollisionDetection leftCollision;
    private ArticulationCollisionDetection rightCollision;
    private bool gripperClosed = false;

    // grasping affects wheel velocity (if wheel attached)
    [SerializeField] private ArticulationWheelController wheelController;
    [SerializeField] private string wheelSpeedLimitID = "grasping limit";

    void Start() 
    { 
        // Init gripper setting
        leftCollision = leftFinger.gameObject.GetComponentInChildren<ArticulationCollisionDetection>();
        rightCollision = rightFinger.gameObject.GetComponentInChildren<ArticulationCollisionDetection>();
    }

    void FixedUpdate()
    {
        // Grasping detection
        CheckGrasping();
    }

    // Gripper position control
    public void CloseGripper()
    {
        SetGripper(1.0f);
    }

    public void OpenGripper()
    {
        SetGripper(0.0f);
    }

    public void StopGripper()
    {
        ArticulationBodyUtils.StopJoint(leftFinger);
        ArticulationBodyUtils.StopJoint(rightFinger);
    }

    public void SetGripper(float value)
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
        // Graspable object detection
        if ((gripperClosed) && (!grasping.isGrasping))
        {
            // If both fingers are touching the same graspable object
            if ((leftCollision.collidingObject != null) && 
                (rightCollision.collidingObject != null) &&
                (leftCollision.collidingObject == rightCollision.collidingObject) &&           
                (leftCollision.collidingObject.tag == "GraspableObject") )

                grasping.Attach(leftCollision.collidingObject);
                // slow down wheel based on the object mass
                if (wheelController != null)
                {
                    float speedLimitPercentage = 
                        0.1f * (10f - grasping.GetGraspedObjectMass());
                    speedLimitPercentage = Mathf.Clamp(speedLimitPercentage, 0f, 1f);
                    wheelSpeedLimitID = wheelController.AddSpeedLimit(
                        new float[] 
                            {
                                1.0f * speedLimitPercentage, 
                                1.0f * speedLimitPercentage,
                                1.0f * speedLimitPercentage, 
                                1.0f * speedLimitPercentage
                            },
                            wheelSpeedLimitID
                    );
                }
        }
    }

    // Gripper functions
    public void ChangeGripperStatus()
    {
        if (gripperClosed)
            OpenGripper2();
        else
            CloseGripper2();
    }
    public void CloseGripper2()
    {
        CloseGripper();
        gripperClosed = true;
    }
    public void OpenGripper2()
    {
        OpenGripper();
        gripperClosed = false;
        grasping.Detach();
        // resume speed
        if (wheelController != null)
            wheelController.RemoveSpeedLimit(wheelSpeedLimitID);
    }
}
