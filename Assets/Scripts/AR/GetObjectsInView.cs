using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetObjectsInView : MonoBehaviour
{
    public Camera mainCamera;  // Reference to the camera rendering the scene
    public List<Transform> graspableObjectsInView = new List<Transform>();  // List to store Graspable objects in view

    // public GraphicalInterface graphicalInterface;
    void Start()
    {
        // Automatically add all objects with a Graspable component to the list
        AutoPopulateGraspableObjectsInView();
    }

    void Update()
    {

        if(mainCamera == null)
        {
            return;
        }

        UpdateGraspableObjectsInViewList();

        Debug.Log(graspableObjectsInView.Count);
    }

    void UpdateGraspableObjectsInViewList()
    {
        graspableObjectsInView.Clear();

        foreach (Graspable graspable in FindObjectsOfType<Graspable>())
        {
            if (IsGraspableObjectInView(graspable.gameObject))
            {
                graspableObjectsInView.Add(graspable.transform);
            }
        }

        // Now graspableObjectsInView list contains the Graspable objects in the camera's view
    }

    public bool IsGraspableObjectInView(GameObject obj)
    {
        if (mainCamera != null)
        {
            // Get the viewport position of the object
            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(obj.transform.position);

            // Check if the object is within the camera's view
            return viewportPosition.x >= 0 && viewportPosition.x <= 1 &&
                   viewportPosition.y >= 0 && viewportPosition.y <= 1 && viewportPosition.z > 0;
        }
        else
        {
            Debug.LogError("Camera reference is missing!");
            return false;
        }
    }

    void AutoPopulateGraspableObjectsInView()
    {
        // Automatically add all objects with a Graspable component to the list
        foreach (Graspable graspable in FindObjectsOfType<Graspable>())
        {
            graspableObjectsInView.Add(graspable.transform);
        }
    }
}
