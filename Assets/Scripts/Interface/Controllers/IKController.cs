using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class IKController : MonoBehaviour
{
    public ArticulationJointController articulationJointController;
    public ArticulationGripperController articulationGripperController;
    public NewtonIK newtonIK;

    // ENUM for mode CONTROL or TARGET
    private enum Mode
    {
        CONTROL,
        TARGET,
        PENDING
    }
    private Mode mode = Mode.PENDING;

    // For CONTROL
    public float moveSpeed = 0.005f;
    public float rotateSpeed = 0.25f;
    private Vector3 deltaPosition = Vector3.zero;
    private Vector3 deltaRotation = Vector3.zero;

    // For TARGET
    public Grabbable target;

    // General
    private float[] jointAngles;
    private Coroutine coroutine;

    public void Start()
    {
        StartCoroutine(SwitchToControl());
    }

    private IEnumerator SwitchToControl()
    {
        yield return new WaitForSeconds(3);
        jointAngles = articulationJointController.GetCurrentJointTargets();
        mode = Mode.CONTROL;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (this.enabled)
        {
            deltaPosition = context.ReadValue<Vector3>() * moveSpeed;
        }
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (this.enabled)
        {
            deltaRotation = context.ReadValue<Vector3>() * rotateSpeed;
        }
    }

    public void FixedUpdate()
    {
        if (mode == Mode.CONTROL && this.enabled)
        {
            Quaternion deltaRotationQuaternion = Quaternion.Euler(deltaRotation);
            jointAngles = articulationJointController.GetCurrentJointTargets();
            jointAngles = newtonIK.SolveVelocityIK(jointAngles, deltaPosition, deltaRotationQuaternion);
            articulationJointController.SetJointTargets(jointAngles);
        }
    }

    public void OnTarget(InputAction.CallbackContext context)
    {
        // check if script is enabled
        // coroutine's are weird, need to check if self is enabled
        if (context.performed && this.enabled)
        {
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(MoveToTargets(target));
        }
    }

    private IEnumerator LerpJoints(float[] currentAngles, float[] targetJointAngles, float seconds)
    {
        float elapsedTime = 0;
        // Keep track of starting angles
        float[] startingAngles = currentAngles.Clone() as float[];
        while (elapsedTime < seconds)
        {
            // lerp each joint angle in loop
            // calculate smallest difference between current and target joint angle
            // using atan2(sin(x-y), cos(x-y))
            for (int i = 0; i < targetJointAngles.Length; i++)
            {
                currentAngles[i] = Mathf.Lerp(startingAngles[i], targetJointAngles[i], (elapsedTime / seconds));
            }
            elapsedTime += Time.deltaTime;

            articulationJointController.SetJointTargets(currentAngles);

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator MoveToTargets(Grabbable target)
    {
        // Lock controls
        mode = Mode.TARGET;

        Debug.Log("Moving to hover...");
        Vector3 targetPosition = target.hoverPoint.position;
        Quaternion targetRotation = target.hoverPoint.rotation;

        jointAngles = articulationJointController.GetCurrentJointTargets();
        (bool converged, float[] targetJointAngles) = newtonIK.SolveIK(jointAngles, targetPosition, targetRotation, false);
        if (!converged)
        {
            Debug.Log("IK did not converge");
            mode = Mode.CONTROL;
            yield break;
        }

        // Lerp between points
        yield return LerpJoints(jointAngles, targetJointAngles, 5.0f);

        Debug.Log("Moving to grab point...");
        targetPosition = target.grabPoint.position;
        targetRotation = target.grabPoint.rotation;

        // Assume we got to the target
        jointAngles = targetJointAngles;
        (converged, targetJointAngles) = newtonIK.SolveIK(jointAngles, targetPosition, targetRotation, false);
        if (!converged)
        {
            Debug.Log("IK did not converge");
            mode = Mode.CONTROL;
            yield break;
        }
        yield return LerpJoints(jointAngles, targetJointAngles, 5.0f);

        // close the gripper
        articulationGripperController.SetGrippers(1.0f);

        // move back to hover point
        targetPosition = target.hoverPoint.position;
        targetRotation = target.hoverPoint.rotation;

        // Assume we got to the target
        jointAngles = targetJointAngles;
        (converged, targetJointAngles) = newtonIK.SolveIK(jointAngles, targetPosition, targetRotation, false);
        if (!converged)
        {
            Debug.Log("IK did not converge");
            mode = Mode.CONTROL;
            yield break;
        }
        yield return LerpJoints(jointAngles, targetJointAngles, 5.0f);

        // assume we got there
        jointAngles = targetJointAngles;

        // then lerp back to home position
        Debug.Log("Moving to home position...");
        targetJointAngles = articulationJointController.homePosition;
        yield return LerpJoints(jointAngles, targetJointAngles, 5.0f);

        mode = Mode.CONTROL;
    }

}
