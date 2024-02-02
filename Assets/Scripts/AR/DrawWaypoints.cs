using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWaypoints : MonoBehaviour
{
    // [SerializeField] private Material lineMaterial;
    private List<Vector3> waypoints = new List<Vector3>();
    private LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        // Add waypoints to the list
        waypoints.Add(new Vector3(-6.5f, 0.05f, -14f));
        waypoints.Add(new Vector3(-6.5f, 0.05f, -13f));
        waypoints.Add(new Vector3(-6.5f, 0.05f, -12f));
        waypoints.Add(new Vector3(-6.5f, 0.05f, -11f));
        waypoints.Add(new Vector3(-6.5f, 0.05f, -10f));

        // Create LineRenderer component
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = waypoints.Count;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // Set positions for the line
        lineRenderer.SetPositions(waypoints.ToArray());

        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

        lineRenderer.colorGradient = new Gradient();
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.blue;
    }

    // Update is called once per frame
    void Update()
    {
        // You can perform additional logic in the Update method if needed
    }
}
