using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawBoundingBox : MonoBehaviour
{
    public Camera cameraToUse;  // Reference to the camera rendering the scene
    public Canvas canvas;       // Reference to the canvas where you want to draw the box
    public GameObject targetObject;  // Reference to the GameObject for which you want to draw the box
    public Color boxColor = Color.red;  // Color of the box

    void Start()
    {
    }

    void Update()
    {
        if (cameraToUse == null)
        {
            Debug.LogError("Camera reference is missing!");
            return;
        }

        if (canvas == null)
        {
            Debug.LogError("Canvas reference is missing!");
            return;
        }

        if (targetObject == null)
        {
            Debug.LogError("Target object reference is missing!");
            return;
        }

        // Draw the bounding box on the canvas
        DrawBoundingBoxOnCanvas();
    }

    

    void DrawBoundingBoxOnCanvas()
    {
        // Get the bounds of the target object in world space
        Bounds objectBounds = GetWorldBounds(targetObject);

        Debug.Log(cameraToUse.WorldToScreenPoint(targetObject.transform.position));

        // Convert the bounds to screen coordinates
        Vector3 minScreenPoint = cameraToUse.WorldToScreenPoint(objectBounds.min);
        Vector3 maxScreenPoint = cameraToUse.WorldToScreenPoint(objectBounds.max);

        // Calculate the size of the box in screen space
        Vector3 boxSize = maxScreenPoint - minScreenPoint;

        // Create a RectTransform for the canvas
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Create a UI panel to represent the bounding box
        GameObject boundingBoxPanel = new GameObject("BoundingBoxPanel");
        RectTransform boundingBoxRect = boundingBoxPanel.AddComponent<RectTransform>();
        Image boundingBoxImage = boundingBoxPanel.AddComponent<Image>();

        // Set the position and size of the panel on the canvas
        boundingBoxRect.SetParent(canvasRect);
        boundingBoxRect.anchorMin = new Vector2(minScreenPoint.x / Screen.width, minScreenPoint.y / Screen.height);
        boundingBoxRect.anchorMax = new Vector2(maxScreenPoint.x / Screen.width, maxScreenPoint.y / Screen.height);
        boundingBoxRect.sizeDelta = new Vector2(boxSize.x, boxSize.y);

        // Convert anchored position to canvas space
        Vector2 canvasPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, minScreenPoint, cameraToUse, out canvasPosition);
        boundingBoxRect.anchoredPosition = canvasPosition + new Vector2(boundingBoxRect.rect.width / 2, boundingBoxRect.rect.height / 2);

        // Set the color of the bounding box
        boundingBoxImage.color = boxColor;

        Debug.Log($"minScreenPoint: {minScreenPoint}, maxScreenPoint: {maxScreenPoint}, boxSize: {boxSize}, canvasPosition: {canvasPosition}");

    }



    Bounds GetWorldBounds(GameObject obj)
    {
        // Get the bounds in world space
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }
        else
        {
            Debug.LogWarning("Renderer component not found on the target object. Using a default bounds.");
            return new Bounds(obj.transform.position, Vector3.one);
        }
    }
}
