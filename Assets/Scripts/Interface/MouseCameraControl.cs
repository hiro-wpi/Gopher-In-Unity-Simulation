using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script detects mouse position
///     use it to control the active camera
/// </summary>
public class MouseCameraControl : MonoBehaviour
{
    public float mouseSensitivity = 150f;

    public ArticulationBody cameraYawJoint;
    public ArticulationBody cameraPitchJoint;
    public ArticulationJointController jointController;
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
        Cursor.lockState = CursorLockMode.Locked;
        
        yawOffsetDeg = yawOffset * Mathf.Rad2Deg;
        pitchOffsetDeg = pitchOffset * Mathf.Rad2Deg;
        yawRotation = yawOffsetDeg;
        pitchRotation = pitchOffsetDeg;
    }

    // Update is called once per frame
    void Update()
    {
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

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
        
        jointController.SetJointTarget(cameraYawJoint, yawRotation, speed);
        jointController.SetJointTarget(cameraPitchJoint, pitchRotation, speed);
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
        bool yawHomed = cameraYawJoint.xDrive.target == yawOffsetDeg;
        bool pitchHomed = cameraPitchJoint.xDrive.target == pitchOffsetDeg;

        if (!yawHomed)
            jointController.SetJointTarget(cameraYawJoint, yawOffsetDeg, speed);
        if (!pitchHomed)
            jointController.SetJointTarget(cameraPitchJoint, pitchOffsetDeg, speed);
        if (yawHomed && pitchHomed)
            return true;
        return false;
    }
}
