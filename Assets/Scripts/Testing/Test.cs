using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform t1;
    public Transform t2;

    void Start()
    {}

    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(GetAngleDifference(t1.rotation, t2.rotation));
            Debug.Log(Quaternion.Angle(t1.rotation, t2.rotation));
        }
        */
    }

    private float GetAngleDifference(Quaternion r1, Quaternion r2)
    {
        // Angle difference
        Quaternion rotationError = r1 * Quaternion.Inverse(r2);
        rotationError.ToAngleAxis(out float rotationAngle, out _);
        // Wrap and get magnitude
        rotationAngle = Mathf.Abs(Mathf.DeltaAngle(0f, rotationAngle));
        return rotationAngle;
    }
}
