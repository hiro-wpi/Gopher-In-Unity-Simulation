using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO
// 2, Use MainCameraRenderTexture to get the screen size
// instead of hardcoding it (1440x810);
public class DrawBoundingBox : MonoBehaviour
{
    public Camera cameraToUse;
    public Canvas canvas;
    private RectTransform canvasRect;
    public GameObject targetObject;
    public Color boxColor = Color.red;
    public GameObject boundingBoxPanel = null;
    RectTransform boundingBoxUI;
    public Texture2D boundingBoxTexture;

    public RectTransform MainCameraDisplay = null;
    
    void Start()
    {
        
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
        if (targetObject == null)
        {
            return;
        }

        // Create a RectTransform for the canvas <-- GOOD
        canvasRect = canvas.GetComponent<RectTransform>();

        // Create a UI panel to represent the bounding box <-- GOOD
        boundingBoxPanel = new GameObject("BoundingBoxPanel");
        boundingBoxUI = boundingBoxPanel.AddComponent<RectTransform>();
        RawImage boundingBoxImage = boundingBoxPanel.AddComponent<RawImage>();
        boundingBoxPanel.transform.parent = canvas.transform;
    
        // Set the color of the bounding box <-- GOOD
        boundingBoxImage.color = boxColor;

        // IV Add texture to draw rectangle <-- GOOD
        boundingBoxImage.texture = boundingBoxTexture;

        UpdateBoundingBox();
    }

    private void UpdateBoundingBox()
    {
        if (targetObject == null)
        {
            // Hide the bounding box
            // boundingBoxUI.sizeDelta = new Vector2(0, 0);
            boundingBoxPanel.SetActive(false);
            return;
        }
        else
        {
            boundingBoxPanel.SetActive(true);
        }

        float canvasWidth = MainCameraDisplay.rect.width;
        float canvasHeight = MainCameraDisplay.rect.height;
        float buffer = 0.075f; // keep it between 0 and 0.1

        // Get the bounds of the target object in world space <-- GOOD
        Bounds objectBounds = GetWorldBounds(targetObject);

        // I - Convert the bounds to screen coordinates <-- GOOD / TODO
        Vector3 minScreenPoint = cameraToUse.WorldToViewportPoint(objectBounds.min);
        Vector3 maxScreenPoint = cameraToUse.WorldToViewportPoint(objectBounds.max);
        Vector3 centerScreenPoint = cameraToUse.WorldToViewportPoint(objectBounds.center);

        // 0 - hide the bounding box if the object is out of view of the canvas + buffer
        if (minScreenPoint.x < 0 + buffer || minScreenPoint.y < 0 + buffer || maxScreenPoint.x > 1 - buffer || maxScreenPoint.y > 1 - buffer)
        {
            // Hide the bounding box
            // boundingBoxUI.sizeDelta = new Vector2(0, 0);
            boundingBoxPanel.SetActive(false);
            return;
        }
        else
        {
            boundingBoxPanel.SetActive(true);
        }

        minScreenPoint.x = minScreenPoint.x * canvasWidth;
        minScreenPoint.y = minScreenPoint.y * canvasHeight;
        maxScreenPoint.x = maxScreenPoint.x * canvasWidth;
        maxScreenPoint.y = maxScreenPoint.y * canvasHeight;

        // II Calculate size of the bounding box in canvas space
        Vector2 centerPosition = new Vector2(
            (minScreenPoint.x + maxScreenPoint.x) / 2,
            (minScreenPoint.y + maxScreenPoint.y) / 2
        );
        Vector2 sizeInViewport = new Vector2(
            Mathf.Abs(maxScreenPoint.x - minScreenPoint.x), 
            Mathf.Abs(maxScreenPoint.y - minScreenPoint.y)
        ) * 2;

        // III Adjust position and size of the bounding box
        boundingBoxUI.anchoredPosition = new Vector2(
            (centerPosition.x) - (canvasRect.sizeDelta.x / 2), 
            (centerPosition.y) - (canvasRect.sizeDelta.y / 2)
        );
        boundingBoxUI.sizeDelta = sizeInViewport;
    }

    private Bounds GetWorldBounds(GameObject obj)
    {
        // Get the bounds in world space <-- GOOD
        Renderer renderer = obj.GetComponent<Renderer>();
        return renderer.bounds;
    }
}
