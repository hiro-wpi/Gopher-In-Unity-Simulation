using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A Goal Class used to check whether a goal is reached
/// </summary>
public class Goal : MonoBehaviour
{
    private Collider detectionCollider;

    // Whether to use OnTriggerEnter() and OnTriggerExit() to track objects
    // or simply use collider.bounds.contains() to check collision
    public bool useColliderTrigger = false;
    HashSet<GameObject> collidingObjects = new HashSet<GameObject>();


    void Start()
    {
        // A trigger collider used to
        // check if a given game object is inside
        detectionCollider = gameObject.GetComponent<Collider>();
        Debug.Assert(detectionCollider.isTrigger == true);
    }

    
    void OnTriggerEnter(Collider other) 
    {
        if (useColliderTrigger)
            // Store the object
            collidingObjects.Add(GetColliderGameObject(other));
    }
    void OnTriggerExit(Collider other) 
    {
        if (useColliderTrigger)
            // Remove the object
            collidingObjects.Remove(GetColliderGameObject(other));
    }
    private GameObject GetColliderGameObject(Collider collider)
    {
        // Get collider's gameObject
        GameObject colliderGameObject;
        // find parent rigid body or articulation body
        Rigidbody colliderRb = collider.attachedRigidbody;
        ArticulationBody colliderAB = collider.attachedArticulationBody;
        if (colliderRb != null)
            colliderGameObject = colliderRb.gameObject;
        else if (colliderAB != null)
            colliderGameObject = colliderAB.gameObject;
        else
            colliderGameObject = collider.gameObject;

        return colliderGameObject;
    }


    public bool CheckIfObjectReachedGoal(GameObject obj)
    {
        if (useColliderTrigger)
        {
            return collidingObjects.Contains(obj);
        }
        else
        {
            return detectionCollider.bounds.Contains(obj.transform.position);
        }
    }

    public float GetDistanceToGoal(GameObject obj)
    {
        return (obj.transform.position - transform.position).magnitude;
    }


    public void DisableGoalVisualEffect() 
    {
        // Disable all visual effect attached to the goal object
        // Highlight
        HighlightUtils.UnhighlightObject(gameObject);
        // Particle
        ParticleSystem[] particles = gameObject.GetComponentsInChildren<ParticleSystem>();
        if (particles != null && particles.Length > 0)
        {
            foreach (ParticleSystem particle in particles)
                particle.Stop();
        }
    }

    public void EnableGoalVisualEffect()
    {
        // Enable all visual effect attached to the goal object
        // Highlight
        HighlightUtils.ResumeObjectHighlight(gameObject);
        // Particle
        ParticleSystem[] particles = gameObject.GetComponentsInChildren<ParticleSystem>();
        if (particles != null && particles.Length > 0)
        {
            foreach (ParticleSystem particle in particles)
                particle.Play();
        }
    }
}
