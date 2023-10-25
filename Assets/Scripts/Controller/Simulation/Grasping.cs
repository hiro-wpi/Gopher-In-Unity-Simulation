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
    private Transform target;

    void Start() {}

    void FixedUpdate() 
    {
        // If grasping, update the transform of the object
        if (!IsGrasping || objectRb == null)
        {
            return;
        }

        // Update the position of the object
        ObjectVelocityTrackingUpdate();
    }

    private void ObjectVelocityTrackingUpdate()
    {
        // Linear velocity tracking
        // delta
        var positionDelta = endEffector.transform.position - target.position;
        // velocity
        objectRb.velocity = positionDelta / Time.fixedDeltaTime;

        // Angular velocity tracking
        // delta
        var rotationDelta = endEffector.transform.rotation * Quaternion.Inverse(
            target.rotation
        );
        rotationDelta.ToAngleAxis(out var angle, out var rotationAxis);
        // velocity
        if (angle > 180f)
        {
            angle -= 360f;
        }
        if (Mathf.Abs(angle) > Mathf.Epsilon)
        {
            objectRb.angularVelocity = (
                rotationAxis * (angle * Mathf.Deg2Rad)
            ) / Time.fixedDeltaTime;
        }
    }

    // Getters
    public GameObject GetEndEffector()
    {
        return endEffector.gameObject;
    }

    public GameObject GetGraspedObject()
    {
        return graspedObject;
    }

    public GameObject GetGraspTarget()
    {
        return target?.gameObject;
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

        // Set up target
        graspable.SetGrasping(this);
        target = graspable.AttachTransform;
        target.position = endEffector.transform.position;
        target.rotation = endEffector.transform.rotation;
    }

    public void Detach()
    {
        IsGrasping = false;
        // No grasping object
        if (graspedObject == null)
        {
            return;
        }

        graspedObject.GetComponent<Graspable>().SetGrasping(null);
        target = null;
        graspedObject = null;
        objectRb = null;
        objectMass = 0f;
    }
}
