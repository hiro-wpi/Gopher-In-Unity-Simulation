using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script is used to fix the Unity physic
///     for complicated grasping.
///
///     The graspable objects will be "attached"
///     to the endeffector tool transform.
/// </summary>
public class Graspable : MonoBehaviour
{
    // Grasp transform
    private GameObject attachGameObject;
    public Transform AttachTransform;

    // Grasping object
    public Grasping currentGrasping;

    void Start() 
    {
        // Create attach point
        attachGameObject = new GameObject("Attach Transform");
        AttachTransform = attachGameObject.transform;

        // Initialization
        AttachTransform.parent = transform;
        AttachTransform.position = transform.position;
        AttachTransform.rotation = transform.rotation;
    }

    void Update() {}

    public void SetGrasping(Grasping grasping)
    {
        // If nothing is grasping this object
        if (currentGrasping == null)
        {
            currentGrasping = grasping;
        }

        // If the object is already grasped
        else
        {
            // Clear the previous grasping
            if (grasping == null)
            {
                currentGrasping = null;
            }

            // Replace the previous grasping
            else if (grasping != currentGrasping)
            {
                currentGrasping.Detach();
                currentGrasping = grasping;
            }
        }
    }
}