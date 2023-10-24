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
    private GameObject objectOriginalParent;
    private Transform target;

    // Rigidbody settings
    private Rigidbody objectRb;
    private float objectMass = -1.0f;
    private bool wasKinematic;
    private bool usedGravity;
    private float oldDrag;
    private float oldAngularDrag;

    void Start() {}

    void FixedUpdate() 
    {
        // If grasping, update the position of the object
        if (!IsGrasping || objectRb == null)
        {
            return;
        }

        // Update the position of the object
        ObjectVelocityTrackingUpdate();
    }

    private void ObjectVelocityTrackingUpdate()
    {
        // Do linear velocity tracking
        // delta
        var positionDelta = endEffector.transform.position - target.position;
        // velocitysss
        objectRb.velocity = positionDelta / Time.fixedDeltaTime;

        // Do angular velocity tracking
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

        // Set up target
        target = graspable.AttachTransform;
        target.position = endEffector.transform.position;
        target.rotation = endEffector.transform.rotation;

        // Change parent to none
        objectOriginalParent = graspedObject.transform.parent?.gameObject;
        graspedObject.transform.parent = null;

        objectMass = objectRb.mass;
        // Set up rigidbody
        // SetupRigidbodyGrasp(objectRb);
    }

    public void Detach()
    {
        IsGrasping = false;
        // No grasping object
        if (graspedObject == null)
        {
            return;
        }

        // Change parent rb
        graspedObject.transform.parent = objectOriginalParent?.transform;

        // Set rigidbody back
        // SetupRigidbodyDrop(objectRb);

        target = null;
        graspedObject = null;
        objectRb = null;
    }

    private void SetupRigidbodyGrasp(Rigidbody rigidbody)
    {
        // Keep current Rigidbody settings
        objectMass = rigidbody.mass;
        wasKinematic = rigidbody.isKinematic;
        usedGravity = rigidbody.useGravity;
        oldDrag = rigidbody.drag;
        oldAngularDrag = rigidbody.angularDrag;

        // New setting
        rigidbody.isKinematic = false;
        rigidbody.useGravity = false;
        rigidbody.drag = 0f;
        rigidbody.angularDrag = 0f;
    }

    private void SetupRigidbodyDrop(Rigidbody rigidbody)
    {
        // Restore Rigidbody settings
        rigidbody.isKinematic = wasKinematic;
        rigidbody.useGravity = usedGravity;
        rigidbody.drag = oldDrag;
        rigidbody.angularDrag = oldAngularDrag;
    }
}
