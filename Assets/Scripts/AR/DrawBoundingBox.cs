using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawBoundingBox : MonoBehaviour
{
    public Camera cameraToUse;
    public Canvas canvas;
    public GameObject targetObject;
    public Color boxColor = Color.red;
    public GameObject boundingBoxPanel = null;
    private Texture2D boundingBoxTexture;

    void Start()
    {
        boundingBoxTexture = Resources.Load<Texture2D>("AR/AltButton_Normal");
    }

    void Update()
    {
        if (boundingBoxPanel == null)
        {
            DrawBoundingBoxOnCanvas();
        }
        else
        {
            UpdateBoundingBox();
        }
    }

    void DrawBoundingBoxOnCanvas()
    {
        // Get the bounds of the target object in world space <-- GOOD
        Bounds objectBounds = GetWorldBounds(targetObject);

        // 1 - Convert the bounds to screen coordinates <-- GOOD
        Vector3 minScreenPoint = cameraToUse.WorldToViewportPoint(objectBounds.min);
        Vector3 maxScreenPoint = cameraToUse.WorldToViewportPoint(objectBounds.max);

        minScreenPoint.x = minScreenPoint.x * 1440;
        minScreenPoint.y = 810 - minScreenPoint.y * 810;

        maxScreenPoint.x = maxScreenPoint.x * 1440;
        maxScreenPoint.y = 810 - maxScreenPoint.y * 810;

        // 2 - Calculate the size of the box in screen space <-- GOOD
        Vector3 boxSize = maxScreenPoint - minScreenPoint;

        // Create a RectTransform for the canvas <-- GOOD
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Create a UI panel to represent the bounding box <-- GOOD
        boundingBoxPanel = new GameObject("BoundingBoxPanel");
        RectTransform boundingBoxRect = boundingBoxPanel.AddComponent<RectTransform>();
        RawImage boundingBoxImage = boundingBoxPanel.AddComponent<RawImage>();

        // Set the position and size of the panel on the canvas <-- NOT GOOD
        boundingBoxRect.SetParent(canvasRect);
        boundingBoxRect.localScale = Vector3.one;
        boundingBoxRect.localPosition = minScreenPoint;
        boundingBoxRect.sizeDelta = new Vector2(boxSize.x, boxSize.y);

        // 3 - Convert anchored position to canvas space <-- NOT GOOD
        Vector2 minAnchor = new Vector2(minScreenPoint.x, minScreenPoint.y);
        Vector2 maxAnchor = new Vector2(maxScreenPoint.x, maxScreenPoint.y);
        minAnchor.x /= canvasRect.sizeDelta.x;
        minAnchor.y /= canvasRect.sizeDelta.y;
        maxAnchor.x /= canvasRect.sizeDelta.x;
        maxAnchor.y /= canvasRect.sizeDelta.y;
        boundingBoxRect.anchorMin = minAnchor;
        boundingBoxRect.anchorMax = maxAnchor;
    
        // Set the color of the bounding box <-- GOOD
        boundingBoxImage.color = boxColor;

        // 4 Add texture to draw rectangle <-- NOT GOOD
        boundingBoxImage.texture = boundingBoxTexture;    }

    private void UpdateBoundingBox()
    {
        // TODO - Constantly update the position and size of the bounding box
    }

    private Bounds GetWorldBounds(GameObject obj)
    {
        // Get the bounds in world space <-- GOOD
        Renderer renderer = obj.GetComponent<Renderer>();
        return renderer.bounds;
    }
}
