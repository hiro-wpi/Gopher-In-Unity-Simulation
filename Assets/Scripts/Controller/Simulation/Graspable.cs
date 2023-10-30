using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script is used to fix the Unity physic
///     for complicated grasping.
///
///     The graspable objects will be "attached"
///     to the grasping given transform.
/// </summary>
public class Graspable : MonoBehaviour
{
    // Grasped attach transform
    private Rigidbody rigidBody;
    private Transform attachTransform;
    // What is grasping
    private Grasping currentGrasping;
    private Transform graspTransform;

    // Grasping simplification
    [SerializeField] bool disableGravity = false;
    private bool wasGravityEnabled;

    void Start() 
    {
        // Initialization
        rigidBody = GetComponent<Rigidbody>();
        wasGravityEnabled = rigidBody.useGravity;

        // Create attach point
        GameObject attachGameObject = new GameObject("Attach Transform");
        attachTransform = attachGameObject.transform;
        // initialization
        attachTransform.parent = transform;
        attachTransform.position = transform.position;
        attachTransform.rotation = transform.rotation;
    }

    void FixedUpdate()
    {
        // This object is grasped
        // try to move the attach transform to the target endeffector
        if (currentGrasping != null)
        {
            MoveObjectToTarget();
        }
    }

    private void MoveObjectToTarget()
    {
        // Difference
        Vector3 positionDelta = graspTransform.position - attachTransform.position;
        Quaternion rotationDelta = graspTransform.rotation * Quaternion.Inverse(
            attachTransform.rotation
        );

        // Set velocity
        // position
        rigidBody.velocity = positionDelta / Time.fixedDeltaTime;
        // rotation
        rotationDelta.ToAngleAxis(out var angle, out var rotationAxis);
        if (angle > 180f)
        {
            angle -= 360f;
        }
        if (Mathf.Abs(angle) > Mathf.Epsilon)
        {
            rigidBody.angularVelocity = (
                rotationAxis * (angle * Mathf.Deg2Rad)
            ) / Time.fixedDeltaTime;
        }
    }

    public void SetGrasping(Grasping grasping)
    {
        // If nothing is grasping this object
        if (currentGrasping == null)
        {
            // set new grasping
            currentGrasping = grasping;
        }

        // If the object is already grasped
        else
        {
            // replace the previous grasping
            currentGrasping.Detach();
            currentGrasping = grasping;
        }

        // Set up the tracking transforms
        graspTransform = currentGrasping.GetEndEffector().transform;
        attachTransform.position = graspTransform.position;
        attachTransform.rotation = graspTransform.rotation;

        // Disable gravity if needed
        if (disableGravity)
        {
            rigidBody.useGravity = false;
        }
    }

    public void ClearGrasping()
    {
        currentGrasping = null;

        if (disableGravity)
        {
            rigidBody.useGravity = wasGravityEnabled;
        }
    }
}
