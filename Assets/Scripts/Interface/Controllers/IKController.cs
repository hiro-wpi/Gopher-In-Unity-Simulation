using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class IKController : MonoBehaviour
{
    public ArticulationJointController articulationJointController;
    public NewtonIK newtonIK;
    public float moveSpeed = 0.1f;
    public float rotateSpeed = 0.5f;

    // ENUM for mode CONTROL or TARGET
    public enum Mode
    {
        CONTROL,
        TARGET
    }

    public Mode mode = Mode.CONTROL;
    public Transform target;

    private Vector3 deltaPosition = Vector3.zero;
    private Vector3 deltaRotation = Vector3.zero;

    private float[] jointAngles = new float[] { -1.0f, -1.57f, -1.57f, -1.57f, 0.0f, 0.0f, -1.0f };

    public void OnMove(InputAction.CallbackContext context)
    {
        if (mode == Mode.CONTROL) deltaPosition = context.ReadValue<Vector3>() * moveSpeed;
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (mode == Mode.CONTROL) deltaRotation = context.ReadValue<Vector3>() * rotateSpeed;
    }

    private void FixedUpdate()
    {
        if (mode == Mode.TARGET)
        {
            // if (Input.GetKeyUp(KeyCode.R))
            // {
            Vector3 targetPosition = target.position;
            Quaternion targetRotation = target.rotation;
            jointAngles = newtonIK.SolveIK(jointAngles, targetPosition, targetRotation, false);

            // TODO: Need some way of lerping
            articulationJointController.SetJointTargets(jointAngles);
        }
        else
        {
            // float[] jointAngles = articulationJointController.GetCurrentJointTargets();
            // float[] newJointAngles = newtonIK.SolveIK(jointAngles, deltaPosition, deltaRotation);
            // articulationJointController.SetJointTargets(newJointAngles);
        }
    }
}
