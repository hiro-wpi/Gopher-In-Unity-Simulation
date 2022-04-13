using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class GripperController : MonoBehaviour
{
    public ArticulationGripperController articulationGripperController;

    public void OnGrab(InputAction.CallbackContext context)
    {
        float move = context.ReadValue<float>();
        articulationGripperController.SetGrippers(move);
    }
}
