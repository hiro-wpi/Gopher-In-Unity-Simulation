using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script is used to detect graspable objects
///     and set it as a target of arm control manager
/// </summary>
public class AutoGrasping : MonoBehaviour
{
    [SerializeField] private ArmController armController;
    [SerializeField] private Grasping grasping;
    private GameObject targetObject;
    // private Color prevColor;

    // Container for hovering and grasping target
    private Transform targetHoverPoint;
    private Transform targetGraspPoint;
    // flag
    private bool checkHoverReached = false;

    void Start() {}

    void Update() {}

    private void OnTriggerEnter(Collider other) 
    {
        if (other.attachedRigidbody?.gameObject.tag == "GraspableObject")
        {
            // If a new target is found but still in the old target graspable zone
            if (targetObject != null && 
                targetObject != other.attachedRigidbody.gameObject
            )
            {
                CancelCurrentTargetObject();
            }

            // Graspbable object sturcture
            // Target object with Rigidbody -> Model with collider
            //     |->  gameObject with Graspable related scripts
            targetObject = other.attachedRigidbody.gameObject;
            SetTargetObject(targetObject);
        }
    }

    private void OnTriggerStay()
    {
        if (targetObject == null || checkHoverReached == false)
        {
            return;
        }

        // Check if the hover point is reached
        if (Utils.IsPoseClose(
            targetHoverPoint,
            grasping.GetEndEffector().transform
        ))
        {
            // Set grasping target
            armController.SetTarget(
                targetGraspPoint.position, 
                targetGraspPoint.rotation
            );
            checkHoverReached = false;
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.attachedRigidbody?.gameObject.tag == "GraspableObject" &&
            other.attachedRigidbody?.gameObject == targetObject)
        {
            CancelCurrentTargetObject();
        }
    }

    private void SetTargetObject(GameObject targetObject)
    {
        // Get all auto graspable points
        AutoGraspable[] autoGraspables = 
            targetObject.GetComponentsInChildren<AutoGraspable>();

        // Auto grasping not avaliable
        if (autoGraspables.Length == 0)
        {
            Debug.LogWarning("The object is not automatically graspable.");
            return;
        }

        // Found multiple graspable points,
        // select the one with min distance
        float minDistance = 1000;
        int selectedIndex = 0;
        for (int i = 0; i < autoGraspables.Length; ++i)
        {
            float distance = 
                (autoGraspables[i].GetGraspPosition() - transform.position).magnitude;
            if (minDistance > distance)
            {
                minDistance = distance;
                selectedIndex = i;
            }
        }

        // Set goal
        (targetHoverPoint, targetGraspPoint) = 
            autoGraspables[selectedIndex].GetHoverAndGrapPoint(
                grasping.GetEndEffector().transform.position,
                grasping.GetEndEffector().transform.rotation
            );

        armController.SetTarget(
            targetHoverPoint.position, 
            targetHoverPoint.rotation
        );
        checkHoverReached = true;

        /*
        // Highlight
        // if this object was alredy highlighted
        prevColor = HighlightUtils.GetHighlightColor(targetObject);
        HighlightUtils.HighlightObject(targetObject);
        */
    }

    private void CancelCurrentTargetObject()
    {
        /*
        HighlightUtils.UnhighlightObject(targetObject);
        if (prevColor != Color.clear)
        {
            HighlightUtils.HighlightObject(targetObject, prevColor);
            prevColor = Color.clear;
        }
        */

        targetObject = null;
        checkHoverReached = false;
        armController.CancelTarget();
    }
}
