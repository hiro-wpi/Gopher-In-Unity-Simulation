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
    private GameObject attachGameObject;
    public Transform AttachTransform;

    void Start() 
    {
        // Create attach point
        attachGameObject = new GameObject("AttachPoint");
        AttachTransform = attachGameObject.transform;

        // Initialization
        AttachTransform.parent = transform;
        AttachTransform.position = transform.position;
        AttachTransform.rotation = transform.rotation;
    }

    void Update() {}
}