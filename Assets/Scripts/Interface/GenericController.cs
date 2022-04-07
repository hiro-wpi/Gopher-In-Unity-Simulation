using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class GenericController : MonoBehaviour
{
    public ArticulationJointController articulationJointController;
    public ArticulationGripperController articulationGripperController;
    public NewtonIK newtonIK;
    public float moveSpeed = 0.1f;
    public float rotateSpeed = 0.5f;

    public bool open = false;

    private Vector3 deltaPosition = Vector3.zero;
    private Vector3 deltaRotation = Vector3.zero;

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 move = context.ReadValue<Vector2>();
        deltaPosition.x = move.x * moveSpeed;
        deltaPosition.y = move.y * moveSpeed;
    }

    public void OnHeight(InputAction.CallbackContext context)
    {
        float move = context.ReadValue<float>();
        deltaPosition.z = move * moveSpeed;
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        Vector2 move = context.ReadValue<Vector2>();
        deltaRotation.x = move.y * rotateSpeed;
        deltaRotation.y = move.x * rotateSpeed;
    }

    public void OnSpin(InputAction.CallbackContext context)
    {
        float move = context.ReadValue<float>();
        deltaRotation.z = move * rotateSpeed;
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        float move = context.ReadValue<float>();
        articulationGripperController.SetGrippers(move);
    }

    void RunIK()
    {
        float[] jointAngles = articulationJointController.GetCurrentJointTargets();
        float[] newJointAngles = newtonIK.SolveIK(jointAngles, deltaPosition, deltaRotation);
        articulationJointController.SetJointTargets(newJointAngles);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RunIK();
    }
}
