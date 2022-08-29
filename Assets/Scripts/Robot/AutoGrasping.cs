using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
///     This script is used to detect graspable objects
///     and set it as a target of arm control manager
/// </summary>
public class AutoGrasping : MonoBehaviour
{
    public ArmControlManager armControlManager;
    private GameObject targetObject;
    private Color prevColor;

    void Start()
    {
    }

    void Update()
    {
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.attachedRigidbody != null && 
            other.attachedRigidbody.gameObject.tag == "GraspableObject")
        {
            // If a new target is found but still in the old target graspable zone
            if (targetObject != null && targetObject != other.attachedRigidbody.gameObject)
                CancelCurrentTargetObject();
            
            // Graspbable object sturcture
            // Target object with Rigidbody -> Model with collider
            //                       |->  gameObject with Graspable related scripts
            targetObject = other.attachedRigidbody.gameObject;
            SelectCurrentObject(targetObject);
        }
    }
    
    private void OnTriggerExit(Collider other) 
    {
        if (other.attachedRigidbody != null && 
            other.attachedRigidbody.gameObject.tag == "GraspableObject" &&
            other.attachedRigidbody.gameObject == targetObject)
        {
            CancelCurrentTargetObject();
        }
    }

    private void SelectCurrentObject(GameObject targetObject)
    {
        AutoGraspable[] autoGraspables = 
            targetObject.GetComponentsInChildren<AutoGraspable>();
        // Auto grasping not avaliable
        if (autoGraspables.Length == 0)
        {
            Debug.LogWarning("The object is not automatically graspable.");
            return;
        }

        // Highlight
        // if this object was alredy highlighted
        prevColor = HighlightUtils.GetHighlightColor(targetObject);
        HighlightUtils.HighlightObject(targetObject);

        // Select the one with min distance
        float minDistance = 1000;
        int selectedIndex = 0;
        for (int i = 0; i < autoGraspables.Length; ++i)
        {
            float distance = 
                (autoGraspables[i].grabPoint.position - transform.position).magnitude;
            if (minDistance > distance)
            {
                minDistance = distance;
                selectedIndex = i;
            }
        }
        armControlManager.target = autoGraspables[selectedIndex];
    }

    private void CancelCurrentTargetObject()
    {
        HighlightUtils.UnhighlightObject(targetObject);
        if (prevColor != Color.clear)
        {
            HighlightUtils.HighlightObject(targetObject, prevColor);
            prevColor = Color.clear;
        }
        targetObject = null;
        armControlManager.target = null;
    }
}
