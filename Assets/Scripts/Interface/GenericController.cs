using System;
using System.Collections.Generic;
using UnityEngine;

public class GenericController : MonoBehaviour
{
    public ArticulationJointController articulationJointController;
    public ArticulationGripperController articulationGripperController;
    public NewtonIK newtonIK;

    public Vector3Control positionControl;
    public Vector3Control rotationControl;

    public bool open = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 deltaPosition = positionControl.GetVector3();
        Vector3 deltaRotation = rotationControl.GetVector3();

        float[] jointAngles = articulationJointController.GetCurrentJointTargets();

        float[] newJointAngles = newtonIK.SolveIK(jointAngles, deltaPosition, deltaRotation);

        articulationJointController.SetJointTargets(newJointAngles);

        // use mouse click to grab and release
        if (Input.GetMouseButtonDown(0))
        {
            if (open)
            {
                articulationGripperController.CloseGrippers();
                open = false;
            }
            else
            {
                articulationGripperController.OpenGrippers();
                open = true;
            }
        }
    }
}
