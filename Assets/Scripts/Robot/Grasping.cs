using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script is used to fix the Unity physic
///     for complicated grasping.
///     The graspable objects will be attached to the endeffector
///     once they get in touch with the two pads of it.
/// </summary>
public class Grasping : MonoBehaviour
{
    public ArticulationBody endEffector;

    public bool isGrasping;
    private GameObject graspableObject;
    private GameObject objectParent;
    private float objectRigidbodyMass = 0.0f;

    private AutoGraspable graspable;

    void Start()
    {
    }

    void Update()
    {
    }

    public void Attach(GameObject gameObject)
    {
        if (isGrasping || graspableObject == gameObject || gameObject == null)
            return;
        graspableObject = gameObject;
        
        // Remove rigidbody
        Rigidbody rb = graspableObject.GetComponent<Rigidbody>();
        if (rb != null)
            objectRigidbodyMass = rb.mass;
        else
            objectRigidbodyMass = 0.0f;
        Destroy(rb);

        // Change parent
        if (graspableObject.transform.parent == null)
            objectParent = null;
        else
            objectParent = graspableObject.transform.parent.gameObject;
        graspableObject.transform.parent = endEffector.transform;

        isGrasping = true;
    }

    public void Detach()
    {
        if (graspableObject == null)
            return;

        // Add back rigidbody 
        if (objectRigidbodyMass != 0.0f)
        {
            Rigidbody rb = graspableObject.AddComponent<Rigidbody>();
            rb.mass = objectRigidbodyMass;
        }
        
        // Change back parent
        if (objectParent == null)
            graspableObject.transform.parent = null;
        else
            graspableObject.transform.parent = objectParent.transform;

        graspableObject = null;
        isGrasping = false;
    }

    public float GetGraspedObjectMass()
    {
        return objectRigidbodyMass;
    }
}
