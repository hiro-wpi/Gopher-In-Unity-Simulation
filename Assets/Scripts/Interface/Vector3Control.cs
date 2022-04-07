using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Vector3 Control", menuName = "Controls/Vector3 Control")]
public class Vector3Control : ScriptableObject
{
    public KeyCode moveForward = KeyCode.W;
    public KeyCode moveBackward = KeyCode.S;
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;
    public KeyCode moveUp = KeyCode.Q;
    public KeyCode moveDown = KeyCode.E;
    public float moveSpeed = 0.1f;

    public Vector3 GetVector3()
    {
        Vector3 vector = Vector3.zero;

        if (Input.GetKey(moveForward))
        {
            vector += Vector3.forward;
        }
        if (Input.GetKey(moveBackward))
        {
            vector += Vector3.back;
        }
        if (Input.GetKey(moveLeft))
        {
            vector += Vector3.left;
        }
        if (Input.GetKey(moveRight))
        {
            vector += Vector3.right;
        }
        if (Input.GetKey(moveUp))
        {
            vector += Vector3.up;
        }
        if (Input.GetKey(moveDown))
        {
            vector += Vector3.down;
        }

        return vector * moveSpeed;
    }
}
