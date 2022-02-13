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
    public GameObject parent;
    
    private CollisionReader collisionReader;
    private string selfName;
    private string otherName;
    
    void Start()
    {
        selfName = gameObject.name;
    }

    public void setParent(GameObject p)
    {
        parent = p;
        collisionReader = p.GetComponentInChildren<CollisionReader>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ignore self-collision
        GameObject parentObject = collision.gameObject.transform.root.gameObject;
        if(parent == parentObject)
            return;
        
        // Get name
        Rigidbody otherRb = collision.collider.attachedRigidbody;
        ArticulationBody otherAB = collision.collider.attachedArticulationBody;
        if (otherRb != null)
            otherName = otherRb.gameObject.name;
        else if (otherAB != null)
            otherName = otherAB.gameObject.name;
        else
            otherName = collision.collider.name;

        // On collision
        if (collisionReader != null)
            collisionReader.OnCollision(selfName, otherName, 
                                        collision.relativeVelocity.magnitude);
    }
}
