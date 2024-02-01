using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Highlights and Object on a 2D canvas
public class HighlightObject2D : MonoBehaviour
{
    
    // List
    public List<GameObject> objectsToHighlight = new List<GameObject>();
    private List<highlightObject> highlightPanels = new List<highlightObject>();

    
    // Converts from 3D to 2D
    [SerializeField] private Camera cameraToUse;
    [SerializeField] private RectTransform MainCameraDisplay = null;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicalInterface graphicalInterface;

    // Visual for the bounding box
    [SerializeField] private Texture2D boundingBoxTexture;
    [SerializeField] private Color boxColor = Color.red;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get the main camera if it is not already set
        if(cameraToUse == null)
        {
            // Get all the active cameras referanced in the graphical interface
            Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();

            // We have cameras
            if(cameras.Length > 0)
            {
                cameraToUse = cameras[0];
            }
            else
            {
                // No cameras found
                return;
            }
        }

        if(MainCameraDisplay == null || cameraToUse == null || canvas == null)
        {
            return;
        }

        foreach(highlightObject obj in highlightPanels)
        {
            obj.UpdateBoundingBox();
        }
    }

    public void AddHighlight(GameObject obj)
    {
        // if the object is not in the list, add it
        if (!objectsToHighlight.Contains(obj))
        {
            objectsToHighlight.Add(obj);
            // highlightPanels.Add(new RectTransform());
            // isVisable.Add(true);
            // highlightPanels.Add(new highlightObject(obj, true));
            highlightPanels.Add(new highlightObject(obj, true, cameraToUse, canvas, MainCameraDisplay, boundingBoxTexture, boxColor));
        }
    }

    public void RemoveHighlight(GameObject obj)
    {
        // if the object is in the list, remove it
        if (objectsToHighlight.Contains(obj))
        {
            int index = objectsToHighlight.IndexOf(obj);
            objectsToHighlight.RemoveAt(index);

            // cleanly destroy the gameobject
            highlightPanels[index].DestroyBoundingBox();
            highlightPanels.RemoveAt(index);
            
        }
    }

    public void ChangeVisability(GameObject obj, bool visable)
    {
        // if the object is in the list, change the visability
        if (objectsToHighlight.Contains(obj))
        {
            int index = objectsToHighlight.IndexOf(obj);
            highlightPanels[index].isVisable = visable;
        }
    }

    private class highlightObject
    {
        public GameObject targetObject;
        public GameObject boundingBoxPanel;
        public RectTransform boundingBoxUI;
        public bool isVisable;  // should the bounding box be visable

        // 3D to 2D
        private Camera cameraToUse;
        private Canvas canvas;
        private RectTransform MainCameraDisplay;
        private Texture2D boundingBoxTexture;
        private Color boxColor;
        private RectTransform canvasRect;


        public highlightObject(GameObject obj, bool isVisable, Camera cam, Canvas canvas, RectTransform MainCameraDisplay, Texture2D boundingBoxTexture, Color boxColor)
        {
            this.targetObject = obj;
            this.isVisable = isVisable;

            this.cameraToUse = cam;
            this.canvas = canvas;
            this.MainCameraDisplay = MainCameraDisplay;
            this.boundingBoxTexture = boundingBoxTexture;
            this.boxColor = boxColor;
            
            
            DrawBoundingBoxOnCanvas();
        }

        void DrawBoundingBoxOnCanvas()
        {
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

        public void UpdateBoundingBox()
        {
            // Do we want to show the bounding box
            if (isVisable)
            {
                boundingBoxPanel.SetActive(true);
            }
            else
            {
                // Hide the bounding box
                boundingBoxPanel.SetActive(false);
                return;
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

            // Hide the bounding box if the object is out of view of the canvas + buffer
            if (minScreenPoint.x < 0 + buffer || minScreenPoint.y < 0 + buffer || maxScreenPoint.x > 1 - buffer || maxScreenPoint.y > 1 - buffer)
            {
                // Hide the bounding box
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

        public void DestroyBoundingBox()
        {
            // Destroy the bounding box
            Destroy(boundingBoxPanel);
        }
    }

}
