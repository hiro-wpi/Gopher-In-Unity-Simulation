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

    private Vector3 deltaPosition = Vector3.zero;
    private Vector3 deltaRotation = Vector3.zero;

    public void OnMove(InputAction.CallbackContext context)
    {
        deltaPosition = context.ReadValue<Vector3>() * moveSpeed;
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        deltaRotation = context.ReadValue<Vector3>() * rotateSpeed;
    }

    private void FixedUpdate()
    {
        float[] jointAngles = articulationJointController.GetCurrentJointTargets();
        float[] newJointAngles = newtonIK.SolveIK(jointAngles, deltaPosition, deltaRotation);
        articulationJointController.SetJointTargets(newJointAngles);
    }
}
