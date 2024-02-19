using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This class provides a wrapper for LineRenderer
///     to draw a line between waypoints.
///     
///     Draw or Clean waypoints with the public function
///     DrawLine(string id, Vector3[] waypoints)
///     RemoveLine(string id)
/// </summary>
public class DrawWaypoints : MonoBehaviour
{
    [SerializeField] private Material defaultLineMaterial;

    private Dictionary<string, LineRenderer> lineMap = new();
    private Dictionary<string, GameObject> lineObjects = new();

    void Start() { }

    void Update() { }

    public void DrawLine(
        string id,
        Vector3[] waypoints,
        float width = -1.0f,
        Material material = null
    )
    {
        LineRenderer lineRenderer;

        // Update existing line
        if (lineMap.ContainsKey(id))
        {
            // LineRenderer componenet
            lineRenderer = lineMap[id];

            // Ignore width if not given
            if (width > 0)
            {
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width;
            }

            // Ignore material if not given
            if (material != null)
            {
                lineRenderer.material = material;
            }
        }

        // Create new line
        else
        {
            // Make a game object to hold the line
            GameObject lineObject = new GameObject(id);
            lineObject.transform.SetParent(transform);
            lineObject.layer = LayerMask.NameToLayer("ARObject");
            lineObjects.Add(id, lineObject);

            // LineRenderer componenet
            lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.numCapVertices = 5;
            lineRenderer.numCornerVertices = 5;
            lineMap.Add(id, lineRenderer);

            // Set width default if not given
            if (width <= 0)
            {
                width = 0.1f;
            }
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;

            // Set material default if not given
            if (material == null)
            {
                material = defaultLineMaterial;
            }
            lineRenderer.material = material;
        }

        // Set waypoints
        lineRenderer.positionCount = waypoints.Length;
        lineRenderer.SetPositions(waypoints);
        lineRenderer.material = defaultLineMaterial;
    }

    public void RemoveLine(string id)
    {
        if (lineMap.ContainsKey(id))
        {
            Destroy(lineObjects[id]);
            lineMap.Remove(id);
            lineObjects.Remove(id);
        }
    }
}
