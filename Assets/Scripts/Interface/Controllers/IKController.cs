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
    public enum Mode
    {
        CONTROL,
        TARGET
    }
    public Mode mode = Mode.CONTROL;

    // For CONTROL
    public float moveSpeed = 0.1f;
    public float rotateSpeed = 0.5f;
    private Vector3 deltaPosition = Vector3.zero;
    private Vector3 deltaRotation = Vector3.zero;

    // For TARGET
    public Grabbable target;

    // General
    private float[] jointAngles;
    private Coroutine coroutine;

    public void Start()
    {
        jointAngles = articulationJointController.homePosition.Clone() as float[];
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (mode == Mode.CONTROL) deltaPosition = context.ReadValue<Vector3>() * moveSpeed;
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (mode == Mode.CONTROL) deltaRotation = context.ReadValue<Vector3>() * rotateSpeed;
    }

    public void OnTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(MoveToTargets(target));
        }
    }
    private IEnumerator LerpJoints(float[] currentAngles, float[] targetJointAngles, float seconds)
    {
        float elapsedTime = 0;
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
        Debug.Log("Moving to hover...");
        Vector3 targetPosition = target.hoverPoint.position;
        Quaternion targetRotation = target.hoverPoint.rotation;
        (bool converged, float[] targetJointAngles) = newtonIK.SolveIK(jointAngles, targetPosition, targetRotation, false);
        if (!converged)
        {
            Debug.Log("IK did not converge");
            yield break;
        }
        yield return LerpJoints(jointAngles, targetJointAngles, 5.0f);

        // assume we got there
        jointAngles = targetJointAngles;

        Debug.Log("Moving to grab point...");
        targetPosition = target.grabPoint.position;
        targetRotation = target.grabPoint.rotation;
        (converged, targetJointAngles) = newtonIK.SolveIK(jointAngles, targetPosition, targetRotation, false);
        if (!converged)
        {
            Debug.Log("IK did not converge");
            yield break;
        }
        yield return LerpJoints(jointAngles, targetJointAngles, 5.0f);

        // close the gripper
        articulationGripperController.SetGrippers(1.0f);

        // move back to hover point
        targetPosition = target.hoverPoint.position;
        targetRotation = target.hoverPoint.rotation;
        (converged, targetJointAngles) = newtonIK.SolveIK(jointAngles, targetPosition, targetRotation, false);
        if (!converged)
        {
            Debug.Log("IK did not converge");
            yield break;
        }
        yield return LerpJoints(jointAngles, targetJointAngles, 5.0f);

        // assume we got there
        jointAngles = targetJointAngles;

        // then lerp back to home position
        Debug.Log("Moving to home position...");
        targetJointAngles = articulationJointController.homePosition.Clone() as float[];
        yield return LerpJoints(jointAngles, targetJointAngles, 5.0f);

        // assume we got there
        jointAngles = targetJointAngles;
    }

}
