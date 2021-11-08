using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script detects mouse position
///     use it to control the active camera
/// </summary>
public class KeyboardJointControl : MonoBehaviour
{
    public GameObject robotRoot;

    public ArticulationBody joint1;
    public ArticulationBody joint3;
    public ArticulationJointController jointController;
    public GameObject holdObject;
    private Rigidbody objectRigidbody;
    public float speed = 0.15f;
    private float speedDeg;

    private float targetJoint1Speed;
    private float targetJoint3Speed;

    private float joint1Rotation = 0f;
    private float joint3Rotation = 0f;
    public float joint1Offset = 0f;
    public float joint3Offset = 0f;
    private float joint1OffsetDeg;
    private float joint3OffsetDeg;

    public float[] objectRangeX;
    public float[] objectRangeZ;
    private Vector3 objectPos;


    // Use this for initialization
    void Start()
    {
        objectRigidbody = holdObject.GetComponent<Rigidbody>();

        speedDeg = speed * Mathf.Rad2Deg;

        joint1OffsetDeg = joint1Offset * Mathf.Rad2Deg;
        joint3OffsetDeg = joint3Offset * Mathf.Rad2Deg;
        joint1Rotation = joint1OffsetDeg;
        joint3Rotation = joint3OffsetDeg;

        objectPos = new Vector3(objectRangeX[1], 0, objectRangeZ[1]);
    }

    // Update is called once per frame
    void Update()
    {
        targetJoint1Speed = 0;
        targetJoint3Speed = 0;

        // Get key input
        if (Mathf.Abs(objectPos[0] - objectRangeX[0]) < 0.01)
        {
            if (Input.GetKey(KeyCode.I))
            {
                targetJoint1Speed = 1;
            }
            else if (Input.GetKey(KeyCode.K))
            {
                targetJoint1Speed = -1;
            }
        }
        if (Mathf.Abs(objectPos[2] - objectRangeZ[1]) < 0.01)
        {
            if (Input.GetKey(KeyCode.L))
            {
                targetJoint3Speed = 1;
            }
            else if (Input.GetKey(KeyCode.J))
            {
                targetJoint3Speed = -1;
            }
        }
    }

    void FixedUpdate()
    {
        objectPos += new Vector3(targetJoint3Speed, 0, targetJoint1Speed) * Time.fixedDeltaTime * speed;

        float x = Mathf.Clamp(objectPos[0], objectRangeX[0], objectRangeX[1]);
        float z = Mathf.Clamp(objectPos[2], objectRangeZ[0], objectRangeZ[1]);
        objectPos = new Vector3(x, 0, z);

        Vector3 objectGlobalPos = robotRoot.transform.TransformDirection(objectPos);
        objectRigidbody.MovePosition(robotRoot.transform.position + objectGlobalPos);

        joint1Rotation = joint1.xDrive.target;
        joint3Rotation = joint3.xDrive.target;
        joint1Rotation += speedDeg * targetJoint1Speed * Time.fixedDeltaTime * 2.0f;
        joint3Rotation -= speedDeg * targetJoint3Speed * Time.fixedDeltaTime * 1.8f;

        joint1Rotation = Mathf.Clamp(joint1Rotation, 52f, 91f);
        joint3Rotation = Mathf.Clamp(joint3Rotation, 60f, 104f);
        
        jointController.SetJointTarget(joint1, joint1Rotation, speedDeg);
        jointController.SetJointTarget(joint3, joint3Rotation, speedDeg);
    }
}
