using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;

/// <summary>
///     This script provides an arm control manager with two modes: 
///     manual control (velocity control) and automatic grasping
/// </summary>
public class ArmControlManager : MonoBehaviour
{
    public ArticulationJointController jointController;
    public ArticulationGripperController gripperController;
    public NewtonIK newtonIK;

    // Articulation Bodies Presets
    private static float IGNORE_VAL = -100f;
    // presets
    public bool flipPresetAngles;
    public JointAngles[] presets = 
    {
        // preset 1 is the default joint home position
        // preset 2 vertical grasping pose
        new JointAngles(new float[] {0.7f, -Mathf.PI/2, -Mathf.PI/2f, 
                                    -1.2f, 0f, -1.0f, Mathf.PI/2}),
        // preset 3 and 4 only read the last joint
        new JointAngles(new float[] {IGNORE_VAL, IGNORE_VAL, IGNORE_VAL, 
                                     IGNORE_VAL,IGNORE_VAL, IGNORE_VAL, Mathf.PI}),
        new JointAngles(new float[] {IGNORE_VAL, IGNORE_VAL, IGNORE_VAL, 
                                     IGNORE_VAL,IGNORE_VAL, IGNORE_VAL, Mathf.PI/2}),
        // preset 5 is for seconday camera view
        new JointAngles(new float[] {0.91f, -1.13f, -0.85f,
                                    -1.66f, 0.09f, -0.85f, 0.26f}),
        // preset 6 is for narrow pose
        new JointAngles(new float[] {-2.1f, -1.9f, -1.8f, 
                                      2f, -0.4f, -1f, Mathf.PI/2})
    };

    // Grasping
    public Grasping grasping;
    private ArticulationCollisionDetection leftCollision;
    private ArticulationCollisionDetection rightCollision;
    private bool gripperClosed = false;
    // grasping affects wheel velocity (if wheel attached)
    public ArticulationWheelController wheelController;
    public string wheelSpeedLimitID = "grasping limit";

    // ENUM for mode CONTROL or TARGET
    private enum Mode
    {
        Control,
        Target
    }
    private Mode mode;
    
    // Manual IK input - velocity control
    private float[] jointAngles;
    public Vector3 deltaPosition = Vector3.zero;
    public Vector3 deltaRotation = Vector3.zero;
    // Automatic grasping with IK solver
    private Coroutine currentCoroutine;
    public AutoGraspable target;

    void Start()
    {
        // Home joints at the beginning
        jointController.HomeJoints();
        gripperController.OpenGripper();

        // Init manual control and automation params
        jointAngles = jointController.GetCurrentJointTargets();
        mode = Mode.Control;

        // Init gripper setting
        leftCollision = gripperController.leftFinger.
            gameObject.GetComponentInChildren<ArticulationCollisionDetection>();
        rightCollision = gripperController.rightFinger.
            gameObject.GetComponentInChildren<ArticulationCollisionDetection>();
    }


    // Manual control
    void FixedUpdate()
    {
        // If in manual mode
        if (mode == Mode.Control)
        {
            UpdateManualControl();
        }
        // Grasping detection
        CheckGrasping();
    }
    private void UpdateManualControl()
    {
        // End effector position control
        if (deltaPosition != Vector3.zero || deltaRotation != Vector3.zero)
        {
            Quaternion deltaRotationQuaternion = Quaternion.Euler(deltaRotation);
            jointAngles = jointController.GetCurrentJointTargets();
            jointAngles = newtonIK.SolveVelocityIK(jointAngles, deltaPosition, deltaRotationQuaternion);
            jointController.SetJointTargets(jointAngles);
        }
        // Fixing joints when not controlling
        else
        {
            jointAngles = jointController.GetCurrentJointTargets();
            jointController.SetJointTargets(jointAngles);
        }
    }
    
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
                                                        wheelSpeedLimitID);
                }
        }
    }


    // Gripper functions
    public void ChangeGripperStatus()
    {
        if (gripperClosed)
            OpenGripper();
        else
            CloseGripper();
    }
    public void CloseGripper()
    {
        gripperController.CloseGripper();
        gripperClosed = true;
    }
    public void OpenGripper()
    {
        gripperController.OpenGripper();
        gripperClosed = false;
        grasping.Detach();
        // resume speed
        if (wheelController != null)
            wheelController.RemoveSpeedLimit(wheelSpeedLimitID);
    }


    // Move to Preset
    public bool MoveToPreset(int presetIndex)
    {
        // Do not allow auto moving when grasping heavy object
        if (grasping.isGrasping && grasping.GetGraspedObjectMass() > 1)
            return false;

        // Home Position
        if (presetIndex == 0)
            MoveToJointPosition(jointController.homePosition);
        // Presets
        else
            if (flipPresetAngles)
            {
                float[] angles = new float[presets[presetIndex-1].jointAngles.Length];
                for (int i = 0; i < angles.Length; ++i)
                {
                    int multiplier = -1;
                    if (presets[presetIndex-1].jointAngles[i] == IGNORE_VAL)
                        multiplier = 1;
                    angles[i] = multiplier * presets[presetIndex-1].jointAngles[i];
                }
                MoveToJointPosition(angles);
            }
            else
                MoveToJointPosition(presets[presetIndex-1].jointAngles);
        return true;
    }
    private void MoveToJointPosition(float[] jointAngles)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(MoveToJointPositionCoroutine(jointAngles));
    }
    private IEnumerator MoveToJointPositionCoroutine(float[] jointPosition)
    {
        mode = Mode.Target;
        yield return new WaitUntil(() => 
            jointController.MoveToJointPositionStep(jointPosition) == true);
        mode = Mode.Control;
    }


    // Automatic grasping
    public void MoveToTarget(AutoGraspable target = null, float automationSpeed = 0.05f,
                             bool closeGripper = true, 
                             bool backToHoverPoint = true)
    {
        // Update target if given
        if (target != null)
            this.target = target;
        else
        {
            if (this.target == null)
            {
                Debug.Log("No target given.");
                return;
            }
        }

        // Run automation
        if (currentCoroutine != null) 
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(
            MoveToTargetCoroutine(automationSpeed, closeGripper, backToHoverPoint));
    }
    private IEnumerator MoveToTargetCoroutine(float automationSpeed = 0.05f,
                                              bool closeGripper = true, 
                                              bool backToHoverPoint = true)
    {
        // Lock manual control
        mode = Mode.Target;
        // containers
        Transform targetHoverPoint;
        Transform targetGrabPoint;
        Vector3 targetPosition;
        Quaternion targetRotation;
        float completionTime;

        // Get target
        (targetHoverPoint, targetGrabPoint) = 
            target.GetHoverAndGrapPoint(grasping.endEffector.transform.position,
                                        grasping.endEffector.transform.rotation);

        // 1, Move to hover point
        targetPosition = targetHoverPoint.position;
        targetRotation = targetHoverPoint.rotation;

        jointAngles = jointController.GetCurrentJointTargets();
        var (converged, targetJointAngles) =
            newtonIK.SolveIK(jointAngles, targetPosition, targetRotation);
        if (!converged)
        {
            Debug.Log("No valid IK solution given to hover point.");
            mode = Mode.Control;
            yield break;
        }

        // Lerp between points
        completionTime = (grasping.endEffector.transform.position - 
                          targetPosition).magnitude / automationSpeed;
        yield return LerpJoints(jointAngles, targetJointAngles, completionTime);

        // 2, Move to graspable target
        targetPosition = targetGrabPoint.position;
        targetRotation = targetGrabPoint.rotation;

        // Assume we got to the target
        jointAngles = targetJointAngles;
        (converged, targetJointAngles) = newtonIK.SolveIK(jointAngles, targetPosition, targetRotation);
        if (!converged)
        {
            Debug.Log("No valid IK solution given to grasping point.");
            mode = Mode.Control;
            yield break;
        }

        completionTime = (grasping.endEffector.transform.position - 
                          targetPosition).magnitude / automationSpeed;
        yield return LerpJoints(jointAngles, targetJointAngles, completionTime);

        // 3, Close the gripper
        // close the gripper
        if (closeGripper)
        {
            gripperController.CloseGripper();
        }

        // 4, Move back to hover point
        if (closeGripper && backToHoverPoint)
        {
            targetPosition = targetHoverPoint.position;
            targetRotation = targetHoverPoint.rotation;

            // Assume we got to the target
            jointAngles = targetJointAngles;
            (converged, targetJointAngles) = 
                newtonIK.SolveIK(jointAngles, targetPosition, targetRotation);
            if (!converged)
            {
                mode = Mode.Control;
                yield break;
            }

            // Lerp between points
            completionTime = (grasping.endEffector.transform.position - 
                              target.transform.position).magnitude / automationSpeed;
            yield return LerpJoints(jointAngles, targetJointAngles, completionTime);
        }
        
        // Give back to manual control
        mode = Mode.Control;
    }
    // Utils
    private IEnumerator LerpJoints(float[] currentAngles, 
                                   float[] targetJointAngles, 
                                   float seconds)
    {
        float elapsedTime = 0;
        // Keep track of starting angles
        var startingAngles = currentAngles.Clone() as float[];
        while (elapsedTime < seconds)
        {
            // lerp each joint angle in loop
            // calculate smallest difference between current and target joint angle
            // using atan2(sin(x-y), cos(x-y))
            for (var i = 0; i < targetJointAngles.Length; i++)
            {
                currentAngles[i] = Mathf.Lerp(startingAngles[i], 
                                              targetJointAngles[i], 
                                              (elapsedTime / seconds));
            }

            elapsedTime += Time.deltaTime;

            jointController.SetJointTargets(currentAngles);

            yield return new WaitForEndOfFrame();
        }
    }
}