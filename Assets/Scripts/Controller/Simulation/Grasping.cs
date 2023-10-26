using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     This script is used to fix the Unity physic
///     for complicated grasping.
///
///     The graspable objects will be "attached"
///     to the endeffector tool transform.
/// </summary>
public class Grasping : MonoBehaviour
{
    [field:SerializeField, ReadOnly] 
    public bool IsGrasping { get; private set; } = false;

    // End effector toolframe
    // Object would be attached to this game object
    [SerializeField] private ArticulationBody endEffector;

    // Grasping object
    private GameObject graspedObject;
    private Rigidbody objectRb;
    private float objectMass;

    void Start() {}

    void Update() {}

    // Getters
    public GameObject GetEndEffector()
    {
        return endEffector.gameObject;
    }

    public GameObject GetGraspedObject()
    {
        return graspedObject;
    }

    public float GetGraspedObjectMass()
    {
        return objectMass;
    }

    // Attach and detach object
    public void Attach(GameObject gameObject)
    {
        // Check validity
        Graspable graspable = gameObject?.GetComponent<Graspable>();
        Rigidbody rb = gameObject?.GetComponent<Rigidbody>();
        if (graspable == null
            || rb == null
            || IsGrasping
            || graspedObject == gameObject
        ){
            return;
        }

        // Valid, grasp it
        IsGrasping = true;
        graspedObject = gameObject;
        objectRb = rb;
        objectMass = objectRb.mass;

        // Set up grasping
        graspable.SetGrasping(this);
    }

    public void Detach()
    {
        IsGrasping = false;
        // No grasping object
        if (graspedObject == null)
        {
            return;
        }

        // Clear grasping
        Graspable graspable = graspedObject.GetComponent<Graspable>();
        graspable.ClearGrasping();

        graspedObject = null;
        objectRb = null;
        objectMass = 0f;
    }
}
