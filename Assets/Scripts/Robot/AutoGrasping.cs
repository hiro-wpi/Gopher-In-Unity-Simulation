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
                HighlightUtils.UnhighlightObject(targetObject);
            
            // Graspbable object sturcture
            // Target object with Rigidbody -> Model with collider
            //                       |->  gameObject with Graspable related scripts
            targetObject = other.attachedRigidbody.gameObject;
            armControlManager.target = targetObject.GetComponentInChildren<Graspable>();

            HighlightUtils.HighlightObject(targetObject);
        }
    }
    // TODO
    // When released, the previously highlighted (before grasping) objects will be 
    // unhighligted. need to find a way to solve this.
    private void OnTriggerExit(Collider other) 
    {
        if (other.attachedRigidbody != null && 
            other.attachedRigidbody.gameObject.tag == "GraspableObject" &&
            other.attachedRigidbody.gameObject == targetObject)
        {
            HighlightUtils.UnhighlightObject(targetObject);
            targetObject = null;
            armControlManager.target = null;
        }
    }
}
