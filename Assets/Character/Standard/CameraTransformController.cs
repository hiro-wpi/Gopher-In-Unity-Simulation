using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransformController : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    public Transform controlledCamera;

    float xRotation = 0f;
    float yRotation = 0f;

    // Use this for initialization
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -60f, 60f);

        controlledCamera.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
