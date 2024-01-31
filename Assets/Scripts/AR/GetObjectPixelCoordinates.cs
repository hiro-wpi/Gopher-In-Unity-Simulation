using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetObjectPixelCoordinates : MonoBehaviour
{
    public Canvas canvas;  // Reference to the canvas

    public Camera mainCamera;  // Reference to the camera rendering the scene
    public List<Transform> objectsToTrack;  // Array of game objects you want to get coordinates for

    public GraphicalInterface graphicalInterface;

    public GameObject imageGameObject;  // Reference to the GameObject with the Image component

    void Start()
    {
    }
    void Update()
    {
        // Get the main camera if it is not already set
        if(mainCamera == null)
        {
            // Get all the active cameras referanced in the graphical interface
            Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();

            // We have cameras
            if(cameras.Length > 0)
            {
                mainCamera = cameras[0];
            }
            else
            {
                // No cameras found
                return;
            }
        }

        if (objectsToTrack.Count == 0)
        {
            AutoPopulateTrackedObjects();
        }


        foreach (Transform obj in objectsToTrack)
        {
            if (mainCamera != null)
            {
                // Get the world position of the object
                Vector3 worldPosition = obj.position;

                // Convert the world position to screen coordinates
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

                // Output the pixel coordinates on the canvas
                // Debug.Log($"{obj.name} - Screen Coordinates: {screenPosition}");

                // Display the image at the pixel coordinates
                DisplayImage(new Vector2(screenPosition.x, screenPosition.y));
            }
            else
            {
                Debug.LogError("Camera reference is missing!");
            }
        }
    }

    void AutoPopulateTrackedObjects()
    {
        // Find game objects with a specific component
        Component[] objectsWithComponent = FindObjectsOfType<Graspable>();
        foreach (Component component in objectsWithComponent)
        {
            if (!objectsToTrack.Contains(component.transform))
            {
                objectsToTrack.Add(component.transform);
            }
        }
    }

    void DisplayImage(Vector2 pixelCoordinates)
    {
        if (canvas != null)
        {
            // Set the position based on pixel coordinates
            RectTransform rectTransform = imageGameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = pixelCoordinates;

            // Optionally, set other properties like size, rotation, etc., as needed
            // rectTransform.sizeDelta = new Vector2(width, height);

            
        }
        else
        {
            Debug.LogError("Canvas or image sprite reference is missing!");
        }
    }



    
}
