using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectSelector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Camera mainCamera;

    public GraphicalInterface graphicalInterface;
    public GameObject mainCameraDisplayObject;

    public GameObject graspableObjects;
    public Vector3 objectPosition;

    // Click and Drag Flags
    private Vector2 dragStartPosition;
    private Vector2 clickPosition;

    private Vector2 selectedPosition = Vector2.zero;
    private bool isDragging;


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
    }

    // Event Handlers
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer Down");
        dragStartPosition = eventData.position;
        clickPosition = eventData.position;
        selectedPosition = Vector2.zero;
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging)
        {
            // Drag Selection
            Vector2 dragCenter = (dragStartPosition + eventData.position) / 2f;
            selectedPosition = dragCenter;
        }
        else
        {
            // Normal Click
            selectedPosition = clickPosition;
        }

        Debug.Log("OnPointerUp: " + selectedPosition);
        // Check if the mouse is not over any UI element, excluding the Canvas
        if(IsPointerOverMainCameraDisplay(selectedPosition.x, selectedPosition.y))
        {
            Ray ray = mainCamera.ScreenPointToRay(selectedPosition);
            RaycastHit hit;

            // Perform the raycast against the 3D objects
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object has a collider
                if (hit.collider != null)
                {
                    // Handle the selection of the object (e.g., print its name)

                    if(hit.collider.gameObject.GetComponent<Graspable>() != null)
                    {
                        // Debug.Log("Selected Object: " + hit.collider.gameObject.name);
                        graspableObjects = hit.collider.gameObject;
                        objectPosition = hit.collider.gameObject.transform.position;
                    }

                }
            }
        }
    }

    // Check if the mouse is on the main camera display
    bool IsPointerOverMainCameraDisplay(float mouseXPosition, float mouseYPosition)
    {
        // Check if the mouse is over any UI element, excluding the Canvas
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(mouseXPosition, mouseYPosition);

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (RaycastResult result in results)
        {

            if (result.gameObject.GetComponent<Canvas>() != null)
            {
                // Ignore the Canvas as a UI element
                
                continue;
            }

            if (result.gameObject.name == mainCameraDisplayObject.name)
            {
                // Mouse is on the main camera display
                // Debug.Log("Mouse is over the main camera display");
                return true;
            }

            
            if(result.gameObject.name == "BoundingBoxPanel")
            {
                // Mouse is over a Graspable object
                return true;
            }

            // Mouse is over a UI element
            return false;
        }

        // Mouse is not over any UI element
        return true;
    }


}
