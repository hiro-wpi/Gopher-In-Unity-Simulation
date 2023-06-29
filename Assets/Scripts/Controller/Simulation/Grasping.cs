using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     This script is used to fix the Unity physic
///     for complicated grasping.
///
///     The graspable objects will be attached 
//      to the endeffector fingers once they 
///     get in touch with the two pads of it.
/// </summary>
public class Grasping : MonoBehaviour
{
    [field:SerializeField, ReadOnly] 
    public bool IsGrasping { get; private set; } = false;

    // End effector toolframe
    // Object would be attached to this game object
    [SerializeField] private ArticulationBody endEffector;

    // Grasping object
    private GameObject graspableObject;
    private GameObject objectOriginalParent;
    private float objectMass = 0.0f;
    private RigidbodyConstraints objectConstrains;
    private NavMeshObstacle[] navMeshObstacles;

    void Start() {}

    void Update() {}

    // Getters
    public GameObject GetEndEffector()
    {
        return endEffector.gameObject;
    }

    public float GetGraspedObjectMass()
    {
        return objectMass;
    }

    public GameObject GetGraspedObject()
    {
        return graspableObject;
    }

    // Attach and detach object
    public void Attach(GameObject gameObject)
    {
        // Check validity
        if (IsGrasping || graspableObject == gameObject || gameObject == null)
        {
            return;
        }

        // Valid object to grasp
        IsGrasping = true;
        graspableObject = gameObject;

        // Change parent to toolframe
        objectOriginalParent = graspableObject.transform.parent?.gameObject;
        graspableObject.transform.parent = endEffector.transform;

        // Get rigidbody mass and remove it
        Rigidbody rb = graspableObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            objectMass = rb.mass;
            objectConstrains = rb.constraints;
            Destroy(rb);
        }
        else
        {
            objectMass = 0.0f;
        }

        // TEMP - may change once global planner is no longer the Unity one
        // Activate nav mesh obstacle if any
        navMeshObstacles = graspableObject.GetComponentsInChildren<NavMeshObstacle>();
        foreach (NavMeshObstacle obstacle in navMeshObstacles)
        {
            obstacle.carving = true;
        }

        // Remove highligh if any
        // HighlightUtils.UnhighlightObject(graspableObject);
    }

    public void Detach()
    {
        IsGrasping = false;
        // No grasping object
        if (graspableObject == null)
        {
            return;
        }

        // Change back parent
        graspableObject.transform.parent = objectOriginalParent?.transform;

        // Add back rigidbody
        if (objectMass != 0.0f)
        {
            Rigidbody rb = graspableObject.AddComponent<Rigidbody>();
            rb.mass = objectMass;
            rb.constraints = objectConstrains;
            objectMass = 0.0f;
            objectConstrains = RigidbodyConstraints.None;
        }

        // TEMP - may change once global planner is no longer the Unity one
        // Inactivate nav mesh obstacle if any
        foreach (NavMeshObstacle obstacle in navMeshObstacles)
        {
            obstacle.carving = false;
        }

        graspableObject = null;
    }
}
