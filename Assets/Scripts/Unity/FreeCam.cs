using System;
using UnityEngine;

/// <summary>
///     A simple free camera to be added to a Unity game object.
///     Keys:
///     wasd / arrows	- movement
///     q/e 			- down/up (local space)
///     r/f 			- up/down (world space)
///     pageup/pagedown	- up/down (world space)
///     hold shift		- enable fast movement mode
///     right mouse  	- enable free look
///     mouse			- free look / rotation
/// </summary>
public class FreeCam : MonoBehaviour
{
    // Normal speed of camera movement.
    [SerializeField] private float movementSpeed = 10f;
    
    // Speed of camera movement when shift is held down
    [SerializeField] private float fastMovementSpeed = 100f;

    // Sensitivity for free look.
    [SerializeField] private float freeLookSensitivity = 3f;

    // Amount to zoom the camera when using the mouse wheel.
    [SerializeField] private float zoomSensitivity = 10f;

    // Amount to zoom the camera when using the mouse wheel (fast mode).
    [SerializeField] private float fastZoomSensitivity = 50f;

    // Set to true when free looking (on right mouse button).
    private bool looking;

    void Update()
    {
        var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        var speed = fastMode ? fastMovementSpeed : movementSpeed;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            transform.position = transform.position + -transform.right * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            transform.position = transform.position + transform.right * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            transform.position = transform.position + transform.forward * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            transform.position = transform.position + -transform.forward * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Q))
            transform.position = transform.position + -transform.up * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.E))
            transform.position = transform.position + transform.up * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
            transform.position = transform.position + Vector3.up * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
            transform.position = transform.position + -Vector3.up * speed * Time.deltaTime;

        if (looking)
        {
            var newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * freeLookSensitivity;
            var newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * freeLookSensitivity;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }

        var axis = Input.GetAxis("Mouse ScrollWheel");
        if (axis != 0)
        {
            var sensitivity = fastMode ? fastZoomSensitivity : zoomSensitivity;
            transform.position = transform.position + transform.forward * axis * sensitivity;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
            StartLooking();
        else if (Input.GetKeyUp(KeyCode.Mouse1)) StopLooking();
    }

    void OnDisable()
    {
        StopLooking();
    }

    // Enable free looking.
    void StartLooking()
    {
        looking = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Disable free looking.
    void StopLooking()
    {
        looking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
