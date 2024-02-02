using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowCreator : MonoBehaviour
{
    public Vector3 startPoint;
    public Vector3 endPoint;
    public GameObject arrowPrefab; // Prefab for the arrowhead, if desired

    public GameObject arrow;

    void Start()
    {
        // CreateArrow();
        arrow = new GameObject("Arrow");
    }

    void CreateArrow()
    {
        // Create a new GameObject for the arrow
        arrow = new GameObject("Arrow");

        // Add LineRenderer component to the arrow GameObject
        // LineRenderer lineRenderer = arrow.AddComponent<LineRenderer>();

        // // Set the material and color for the arrow
        // lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        // lineRenderer.startColor = Color.red;
        // lineRenderer.endColor = Color.red;

        // // Set the width of the arrow
        // lineRenderer.startWidth = 0.1f;
        // lineRenderer.endWidth = 0.1f;

        // // Optionally, instantiate an arrowhead if prefab is provided
        // if (arrowPrefab != null)
        // {
        //     GameObject arrowhead = Instantiate(arrowPrefab, endPoint, Quaternion.identity, arrow.transform);
        //     arrowhead.name = "Arrowhead";
        // }

        // // Set the positions of the arrow's start and end points
        // UpdateArrowPosition();
    }

    void UpdateArrowPosition()
    {
        if (arrow != null)
        {
            // Set the positions of the arrow's start and end points
            LineRenderer lineRenderer = arrow.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);

            // Optionally, update the arrowhead position and rotation based on the endpoint
            if (arrowPrefab != null)
            {
                GameObject arrowhead = arrow.transform.Find("Arrowhead").gameObject;
                if (arrowhead != null)
                {
                    arrowhead.transform.position = endPoint;
                    arrowhead.transform.LookAt(startPoint);
                }
            }
        }
    }

    // Call this method whenever you want to update the arrow's position
    public void SetArrowPoints(Vector3 newStartPoint, Vector3 newEndPoint)
    {
        startPoint = newStartPoint;
        endPoint = newEndPoint;

        UpdateArrowPosition();
    }
}
