using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///    This script is used to control the robotic end-effector. 
///    Assuming simple two-finger prismatics grippers.
///    The gripper will close when the target is greater than 0.5.
///    
///    The actual grasping is done by the Grasping script, which
///    "fake" a grasping by moving the rigidbody of the 
///    grasped object directly.
/// </summary>
public class ArticulationGripperController : MonoBehaviour
{
    // Assume two-finger prismatic joints
    [SerializeField] private ArticulationBody leftFinger;
    [SerializeField] private ArticulationBody rightFinger;
    private bool gripperClosed = false;
    private float leftGripperCloseOffset = 0.0f;
    private float rightGripperCloseOffset = 0.0f;

    // Grasping
    [SerializeField] private Grasping grasping;
    [SerializeField] private float stableTimeToConsiderGrasping = 0.5f;
    private Coroutine graspingCoroutine = null;
    private ArticulationCollisionDetection leftCollision;
    private ArticulationCollisionDetection rightCollision;

    void Start() 
    {
        // Init grasping collision detection
        leftCollision = leftFinger.gameObject.
            GetComponentInChildren<ArticulationCollisionDetection>();
        rightCollision = rightFinger.gameObject.
            GetComponentInChildren<ArticulationCollisionDetection>();

        // Init grasping offset
        // When grasping, the gripper will close a little bit more
        // to maintain a reasonable grasping force
        leftGripperCloseOffset = (
            leftFinger.xDrive.upperLimit - leftFinger.xDrive.lowerLimit
        ) * 0.1f;
        rightGripperCloseOffset = (
            rightFinger.xDrive.upperLimit - rightFinger.xDrive.lowerLimit
        ) * 0.1f;
    }

    void FixedUpdate()
    {
        // If gripper is closed
        // Detect objects to grasp
        if (gripperClosed)
        {
            GraspingDetection();
        }
        // Gripper open
        else
        {
            StopTryingToGrasp();
        }
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
    private void GraspingDetection()
    {
        // If already grasping, check if the object is still in touch
        if (grasping.IsGrasping)
        {
            if (!IsTouchingGraspableObject())
            {
                grasping.Detach();
                gripperClosed = false;
            }
            return;
        }

        // If not grapsing and both fingers are in touch with 
        // the same graspable object for a certain amount of time, grasp it
        if (IsTouchingGraspableObject())
        {
            graspingCoroutine ??= StartCoroutine(TryingToGraspCoroutine());
        }
        else
        {
            StopTryingToGrasp();
        }
    }

    private IEnumerator TryingToGraspCoroutine()
    {
        // Wait for a certain amount of time
        yield return new WaitForSeconds(stableTimeToConsiderGrasping);

        // Attach object to the tool frame
        grasping.Attach(leftCollision.CollidingObject);

        // Keep gripper to its current position
        // but with extra offset to maintain grasping force
        ArticulationBodyUtils.SetJointTarget(
            leftFinger, leftFinger.jointPosition[0] + leftGripperCloseOffset
        );
        ArticulationBodyUtils.SetJointTarget(
            rightFinger, rightFinger.jointPosition[0] + rightGripperCloseOffset
        );

        // Clear coroutine
        graspingCoroutine = null;
    }

    private bool IsTouchingGraspableObject()
    {
        // If both fingers are in touch with the same graspable object
        return leftCollision.CollidingObject != null
            && rightCollision.CollidingObject != null
            && leftCollision.CollidingObject == rightCollision.CollidingObject
            && leftCollision.CollidingObject.GetComponent<Graspable>() != null;
    }

    private void StopTryingToGrasp()
    {
        // Stop the coroutine
        if (graspingCoroutine != null)
        {
            StopCoroutine(graspingCoroutine);
            graspingCoroutine = null;
        }
    }
}
