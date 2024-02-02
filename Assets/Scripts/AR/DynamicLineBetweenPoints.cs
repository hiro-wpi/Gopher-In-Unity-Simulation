using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DynamicLineBetweenPoints : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    void Update()
    {
        UpdateArrow();
    }

    void UpdateArrow()
    {
        if (startPoint != null && endPoint != null)
        {

            // if the start and end point are zero, reset the arrow
            if (startPoint.position == new Vector3(0, 0, 0) && endPoint.position == new Vector3(0, 0, 0))
            {
                resetArrow();
                return;
            }

            // Get the direction and distance between the two points
            Vector3 direction = endPoint.position - startPoint.position;
            float distance = direction.magnitude;

            // Set the position to the midpoint between the two points
            transform.position = (startPoint.position + endPoint.position) / 2f;

            // Set the scale to represent the line between the two points
            transform.localScale = new Vector3(0.1f, 0.1f, distance);

            // Rotate the main body of the arrow to face along the line
            transform.rotation = Quaternion.LookRotation(direction);

            // Optionally, you can remove the collider from the main body if not needed
            if (GetComponent<Collider>() != null)
            {
                Destroy(GetComponent<Collider>());
            }

            // Optionally, you can rotate the arrowhead to face along the line
            RotateEndPoint(direction);
        }
        else
        {
            Debug.LogError("Start Point or End Point not assigned!");
        }
    }

    // Call this method whenever you want to rotate the endPoint
    public void RotateEndPoint(Vector3 eulerAngles)
    {
        if (endPoint != null)
        {
            // endPoint.rotation = Quaternion.Euler(eulerAngles);
            endPoint.rotation = Quaternion.LookRotation(eulerAngles);
        }
        else
        {
            Debug.LogError("End Point not assigned!");
        }
    }

    public void resetArrow()
    {
        transform.position = new Vector3(0, 0, 0);
        transform.localScale = new Vector3(0, 0, 0);
    }
    // public Transform startPoint;
    // public Transform endPoint;

    // void Update()
    // {
    //     UpdateLine();
    // }

    // void UpdateLine()
    // {
    //     if (startPoint != null && endPoint != null)
    //     {
    //         // Get the direction and distance between the two points
    //         Vector3 direction = endPoint.position - startPoint.position;
    //         float distance = direction.magnitude;

    //         // Set the position to the midpoint between the two points
    //         transform.position = (startPoint.position + endPoint.position) / 2f;

    //         // Set the scale to represent the line between the two points
    //         transform.localScale = new Vector3(0.1f, 0.1f, distance);

    //         // Rotate the cube by 90 degrees
    //         transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 0f, 0f);

    //         // Optionally, you can remove the collider from the cube if not needed
    //         if (GetComponent<Collider>() != null)
    //         {
    //             Destroy(GetComponent<Collider>());
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("Start Point or End Point not assigned!");
    //     }
    // }
}
