using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script is used to set graspable objects
///     and set it as a target of arm controller
///     
///     The target can be automatically set when the object
///     is in the graspable zone (defined by the collider)
///     when ProximityGraspableCheck is set to true
///     
///     Or set ProximityGraspableCheck to be false
///     This can be simply used as an utils 
///     by calling GetGraspAndHoverPoint()
/// </summary>
public class AutoGrasping : MonoBehaviour
{
    [SerializeField] private ArmController armController;
    [SerializeField] private Grasping grasping;
    private GameObject targetObject;

    // Set to true to enable proximity graspable check
    public bool ProximityGraspableCheck = false;

    // Container for hovering and grasping target
    private Transform targetHoverPoint;
    private Transform targetGraspPoint;
    // flag
    private bool checkHoverReached = false;

    void Start() {}

    void Update() {}

    public (Transform, Transform) GetHoverAndGraspTransforms(GameObject go)
    {
        if (go.GetComponent<AutoGraspable>() == null)
        {
            Debug.Log("Not a auto graspable object");
            return (null, null);
        }

        AutoGraspable[] autoGraspables = 
            go.GetComponentsInChildren<AutoGraspable>();

        // If found multiple graspable points,
        // select the one with min position distance
        float minDistance = 1000;
        int selectedIndex = 0;
        for (int i = 1; i < autoGraspables.Length; ++i)
        {
            float distance = (
                autoGraspables[i].GetGraspPosition() - transform.position
            ).magnitude;
            if (minDistance > distance)
            {
                minDistance = distance;
                selectedIndex = i;
            }
        }

        // Find hover goal and grasp goal
        // given the current end effector pose
        var (targetHoverTf, targetGraspTf) = 
            autoGraspables[selectedIndex].GetHoverAndGrapPoint(
                grasping.GetEndEffector().transform.position,
                grasping.GetEndEffector().transform.rotation
            );

        return (targetHoverTf, targetGraspTf);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (!ProximityGraspableCheck)
        {
            return;
        }

        // Collider with other objects
        GameObject otherObject = other.attachedRigidbody?.gameObject;
        if (otherObject == null)
        {
            return;
        }
        if (otherObject.GetComponent<AutoGraspable>() == null)
        {
            return;
        }

        // If a new targetSetTargetObject is found 
        // but it is still in the old target graspable zone
        if (targetObject != otherObject)
        {
            if (targetObject != null)
            {
                CancelCurrentTargetObject();
            }
        }
        // If it is the same target
        else
        {
            return;
        }

        // Graspbable object sturcture
        SetTarget(otherObject);
    }

    private void OnTriggerStay()
    {
        if (!ProximityGraspableCheck)
        {
            return;
        }

        if (targetObject == null || checkHoverReached == false)
        {
            return;
        }

        // Check if the hover point is reached
        if (
            Utils.IsPoseClose(
                targetHoverPoint,
                grasping.GetEndEffector().transform,
                0.01f, 
                0.1f
            )
        )
        {
            // Hover point reached
            // Set grasping target
            armController.SetAutonomyTarget(
                targetGraspPoint.position, 
                targetGraspPoint.rotation
            );
            checkHoverReached = false;
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (!ProximityGraspableCheck)
        {
            return;
        }

        GameObject otherObject = other.attachedRigidbody?.gameObject;
        if (otherObject == null)
        {
            return;
        }
        if (
            otherObject.GetComponent<AutoGraspable>() != null
            && otherObject == targetObject)
        {
            CancelCurrentTargetObject();
        }
    }

    private void SetTarget(GameObject go)
    {
        // Find hover goal and grasp goal
        var (hoverPoint, graspPoint) = GetHoverAndGraspTransforms(go);
        if (hoverPoint == null || graspPoint == null)
        {
            return;
        }

        // Valid -> set as target
        targetObject = go;
        targetHoverPoint = hoverPoint;
        targetGraspPoint = graspPoint;

        // Set hover target
        armController.SetAutonomyTarget(
            targetHoverPoint.position, 
            targetHoverPoint.rotation
        );
        checkHoverReached = true;
    }

    public void CancelCurrentTargetObject()
    {
        targetObject = null;
        checkHoverReached = false;
        armController.CancelAutonomyTarget();
    }
}
