using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxObjectInView : MonoBehaviour
{
    // Start is called before the first frame update

    public DrawBoundingBox drawBoundingBox;
    public GetObjectsInView getObjectsInView;

    private Camera mainCamera;

    public GraphicalInterface graphicalInterface;

    private GameObject targetObject = null;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // Make Sure the references are set
        if (drawBoundingBox == null)
        {
            Debug.LogError("DrawBoundingBox reference is missing!");
            return;
        }

        if (getObjectsInView == null)
        {
            Debug.LogError("GetObjectsInView reference is missing!");
            return;
        }

        if (mainCamera == null)
        {
            // Get all the active cameras referanced in the graphical interface
            Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();

            // We have cameras
            if(cameras.Length > 0)
            {
                // Passing in the camera objects
                mainCamera = cameras[0];
                drawBoundingBox.cameraToUse = mainCamera;
                getObjectsInView.mainCamera = mainCamera;
            }
            else
            {
                // No cameras found
                return;
            }
        }

        // If there are no objects in view, set the targetObject to null
        if(getObjectsInView.graspableObjectsInView.Count == 0) // empty
        {
            targetObject = null;
            drawBoundingBox.targetObject = null;
        }
        else
        {
            if (targetObject == null)
            {
                // Get the first object in the list
                if (getObjectsInView.graspableObjectsInView.Count > 0)
                {
                    targetObject = getObjectsInView.graspableObjectsInView[0].gameObject;
                    drawBoundingBox.targetObject = targetObject;
                }
            }
        }


        // if (targetObject == null)
        // {
        //     // Get the first object in the list
        //     if (getObjectsInView.graspableObjectsInView.Count > 0)
        //     {
        //         targetObject = getObjectsInView.graspableObjectsInView[0].gameObject;
        //         drawBoundingBox.targetObject = targetObject;
        //     }
        // }

        // // update if the targetObject is out of view to another object
        // if (targetObject != null)
        // {
        //     // if (!getObjectsInView.IsGraspableObjectInView(targetObject))
        //     // {
        //     //     // Get the first object in the list
        //     //     if (getObjectsInView.graspableObjectsInView.Count > 0)
        //     //     {
        //     //         targetObject = getObjectsInView.graspableObjectsInView[0].gameObject;
        //     //         drawBoundingBox.targetObject = targetObject;
        //     //     }
        //     // }
        // }

    }
}
