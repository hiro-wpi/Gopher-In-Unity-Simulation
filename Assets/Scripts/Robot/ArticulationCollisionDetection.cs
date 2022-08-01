using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script detects ArticulationBody collision with
///     ohter RididBody/ArticulationBody/collider. The result
///     is sent to the sensor "collisionReader".
/// </summary>
public class ArticulationCollisionDetection : MonoBehaviour
{
    // Robot
    public GameObject parent;
    // For reading collision names
    private CollisionReader collisionReader;
    private string selfName;
    private string otherName;
    // For checking object in touch with the collider
    public GameObject collidingObject;
    
    void Start()
    {
        selfName = gameObject.name;
    }

    public void setParent(GameObject p)
    {
        parent = p;
        collisionReader = p.GetComponentInChildren<CollisionReader>();
    }

    void OnCollisionEnter(Collision other)
    {
        // Ignore self-collision
        GameObject parentObject = other.gameObject.transform.root.gameObject;
        if(parent == parentObject)
            return;
        
        // Get name
        collidingObject = GetCollisionGameObject(other);
        otherName = collidingObject.name;

        // On collision
        if (collisionReader != null)
            collisionReader.OnCollision(selfName, otherName, 
                                        other.relativeVelocity.magnitude);
    }

    void OnCollisionExit(Collision other)
    {
        if (collidingObject == GetCollisionGameObject(other))
        {
            collidingObject = null;
        }
    }

    private GameObject GetCollisionGameObject(Collision other)
    {
        GameObject otherGameObject;
        // Find parent rigid body or articulation body
        Rigidbody otherRb = other.collider.attachedRigidbody;
        ArticulationBody otherAB = other.collider.attachedArticulationBody;
        if (otherRb != null)
            otherGameObject = otherRb.gameObject;
        else if (otherAB != null)
            otherGameObject = otherAB.gameObject;
        else
            otherGameObject = other.collider.gameObject;
        
        return otherGameObject;
    }
}
