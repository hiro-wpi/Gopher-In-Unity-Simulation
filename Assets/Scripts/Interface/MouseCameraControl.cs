using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script detects mouse position
///     use it to control the active camera
/// </summary>
public class MouseCameraControl : MonoBehaviour
{
    private bool controlEnabled;
    public float mouseSensitivity = 150f;

    public ArticulationJointController jointContoller;
    public ArticulationBody cameraYawJoint;
    public ArticulationBody cameraPitchJoint;
    public float speed = 1.0f;

    private float mouseX;
    private float mouseY;

    private float yawRotation = 0f;
    private float pitchRotation = 0f;

    public float angleLimit = 60f;

    public float yawOffset = 0f;
    public float pitchOffset = 0f;
    private float yawOffsetDeg;
    private float pitchOffsetDeg;


    // Use this for initialization
    void Start()
    {
        controlEnabled = false;
        
        yawOffsetDeg = yawOffset * Mathf.Rad2Deg;
        pitchOffsetDeg = pitchOffset * Mathf.Rad2Deg;
        yawRotation = yawOffsetDeg;
        pitchRotation = pitchOffsetDeg;
    }

    // Update is called once per frame
    void Update()
    {
        // Switch control on/off
        if (Input.GetKeyDown(KeyCode.C))
        {
            controlEnabled = !controlEnabled;
            if (controlEnabled)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.Confined;
        }

        // Get input from mouse
        if (controlEnabled)
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }
        else
        {
            mouseX = 0f;
            mouseY = 0f;
        }

        // Home robot
        if (Input.GetMouseButtonDown(2))
            HomeCameraJoints();
    }

    void FixedUpdate()
    {
        if (mouseX == 0 && mouseY == 0)
            return;
        
        yawRotation = cameraYawJoint.xDrive.target;
        pitchRotation = cameraPitchJoint.xDrive.target;
        yawRotation -= mouseX * mouseSensitivity * Time.fixedDeltaTime;
        pitchRotation += mouseY * mouseSensitivity * Time.fixedDeltaTime;
        yawRotation = Mathf.Clamp(yawRotation, 
                                  yawOffsetDeg-angleLimit, yawOffsetDeg+angleLimit);
        pitchRotation = Mathf.Clamp(pitchRotation, 
                                    pitchOffsetDeg-angleLimit, pitchOffsetDeg+angleLimit);
        
        jointContoller.SetJointTargetStep(cameraYawJoint, yawRotation*Mathf.Deg2Rad, speed);
        jointContoller.SetJointTargetStep(cameraPitchJoint, pitchRotation*Mathf.Deg2Rad, speed);
    }

    public void HomeCameraJoints()
    {
        StartCoroutine(HomeCameraJointsCoroutine());
    }
    private IEnumerator HomeCameraJointsCoroutine()
    {
        yield return new WaitUntil(() => HomeCameraAndCheck() == true);
    }
    private bool HomeCameraAndCheck()
    {
        bool yawHomed = Mathf.Abs(cameraYawJoint.xDrive.target - yawOffsetDeg) < 0.001;
        bool pitchHomed = Mathf.Abs(cameraPitchJoint.xDrive.target - pitchOffsetDeg) < 0.001;

        if (!yawHomed)
            jointContoller.SetJointTargetStep(cameraYawJoint, yawOffset, speed);
        if (!pitchHomed)
            jointContoller.SetJointTargetStep(cameraPitchJoint, pitchOffset, speed);
        if (yawHomed && pitchHomed)
            return true;
        return false;
    }
}
