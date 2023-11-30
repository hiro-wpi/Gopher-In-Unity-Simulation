using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

/// <summary>
///    Attach all the children game objects with non-kinematic rigidbody
///    with network transform and netwrok rigidbody components
/// </summary>
public class AttachNetworkRigidbody : MonoBehaviour
{
    void Awake()
    {
        // Check if a Network Object componenet is attached
        if (GetComponent<NetworkObject>() == null)
        {
            throw new System.Exception(
                "There should be a NetworkObject component attached"
            );
        }

        // Get all the rigidbodies in the scene
        var rigidbodies = gameObject.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rigidbodies)
        {
            // If the rigidbody is not kinematic
            if (rb.isKinematic == false)
            {
                // Add network components to the game object
                var netTf = rb.gameObject.AddComponent<NetworkTransform>();
                var netRb = rb.gameObject.AddComponent<NetworkRigidbody>();
            }
        }
    }

    // void Start() {}

    // void Update() {}
}
