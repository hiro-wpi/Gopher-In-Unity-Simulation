using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugKeyboardControl : MonoBehaviour
{
    // Mode to determine control transform or rigidbody
    public enum ControlMode
    {
        Transform,
        Rigidbody
    }
    [SerializeField] private ControlMode controlMode = ControlMode.Transform;

    void Start() {}

    void Update()
    {
        // Check control input axis
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Change either transform or rigidbody
        if (controlMode == ControlMode.Transform)
        {
            transform.position += new Vector3(x, 0f, z) * Time.deltaTime;
        }
        else if (controlMode == ControlMode.Rigidbody)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(x, 0f, z);
        }
    }
}
