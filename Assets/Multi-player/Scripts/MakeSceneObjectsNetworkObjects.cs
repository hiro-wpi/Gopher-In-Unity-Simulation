using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

/// <summary>
///    Attach all the game objects in the scene 
///    with non-kinematic rigidbody 
///    with network objects
/// </summary>
public class MakeSceneObjectsNetworkObjects : MonoBehaviour
{
    void Awake()
    {
        // Get all the rigidbodies in the scene
        Rigidbody[] rigidbodies = GameObject.FindObjectsOfType(
            typeof(Rigidbody)
        ) as Rigidbody[];

        foreach (Rigidbody rb in rigidbodies)
        {
            // If the rigidbody is not kinematic
            if (rb.isKinematic == false)
            {
                // Add network components to the game object
                // var netObject = rb.gameObject.AddComponent<NetworkObject>();
                var netTf = rb.gameObject.AddComponent<NetworkTransform>();
                var netRb = rb.gameObject.AddComponent<NetworkRigidbody>();
            }
        }
    }

    // void Update() {}
}
