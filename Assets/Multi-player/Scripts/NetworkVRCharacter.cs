using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
///    Defines a network player.
///    Adjust game object's input and visual if not the owner
/// </summary>
public class NetworkVRCharacter : NetworkBehaviour
{
    [SerializeField] private GameObject xROrigin;
    [SerializeField] private AnimateVRCharacter animateVRCharacter;
    [SerializeField] private GameObject characterRigRoot;
    
    [SerializeField] private Vector3 spawnPositionLower;
    [SerializeField] private Vector3 spawnPositionUpper;

    [SerializeField] private Quaternion spawnRotation = Quaternion.identity;

    public override void OnNetworkSpawn()
    {
        // Spawn in a random position
        transform.position = new Vector3(
            Random.Range(spawnPositionLower.x, spawnPositionUpper.x), 
            Random.Range(spawnPositionLower.y, spawnPositionUpper.y), 
            Random.Range(spawnPositionLower.z, spawnPositionUpper.z)
        );
        transform.rotation = spawnRotation;

        if (!IsOwner)
        {
            // Disable non-owner input
            DisableNonOwnerInput();

            // Disable camera and fix character visual property
            AdjustCameraInput();
        }
    }

    private void DisableNonOwnerInput()
    {
        // Disable all components in XR origin to stop getting undesired input
        // Keep only the network related components
        foreach (
            var component in xROrigin.GetComponentsInChildren<MonoBehaviour>()
        ) {
            if (!(component is NetworkObject) 
                && !(component is ClientNetworkTransform)
            ) {
                component.enabled = false;
            }
        }
        animateVRCharacter.enabled = false;

        // Prevent owner's VR controller assets from being disabled
        var actionManager = xROrigin.GetComponent<InputActionManager>();
        actionManager?.EnableInput();
    }

    private void AdjustCameraInput()
    {
        // Disable camera
        foreach(var camera in xROrigin.GetComponentsInChildren<Camera>())
        {
            camera.enabled = false;
        }

        // Make all the character rig back to the default layer
        // to allow the owner to see the non-owning players body normaly
        int defaultLayer = LayerMask.NameToLayer("Default");
        characterRigRoot.layer = defaultLayer;
        foreach (Transform child in characterRigRoot.transform)
        {
            child.gameObject.layer = defaultLayer;
        }
    }

    // This is necessary to have the object's physics enabled in advance
    public void OnHoverEntered(HoverEnterEventArgs eventArgs)
    {
        if (IsOwner)
        {
            Debug.Log("Enable physics for potential future interaction");
            var networkObjectSelected = eventArgs.interactableObject.
                transform.GetComponent<NetworkObject>();
            if (networkObjectSelected != null)
            {
                Rigidbody rb = networkObjectSelected.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
            }
        }
        else 
        {
            Debug.Log("Not Owner: ???");
        }
    }

    public void OnHoverExited(HoverExitEventArgs eventArgs)
    {
        if (IsOwner)
        {
            Debug.Log("Physics stay how it should be");
            var networkRigidbody = eventArgs.interactableObject.
                transform.GetComponent<NetworkRigidbody>();
            if (networkRigidbody != null)
            {
                // Actually calling UpdateOwnershipAuthority() which is private
                // Call OnGainedOwnership() or OnLostOwnership() instead
                networkRigidbody.OnGainedOwnership();
            }
        }
        else 
        {
            Debug.Log("Not Owner: ???");
        }
    }

    public void OnSelectEntered(SelectEnterEventArgs eventArgs)
    {
        if (IsOwner)
        {
            Debug.Log("Owner: Ownership requested");
            NetworkObject networkObjectSelected = eventArgs.interactableObject.
                transform.GetComponent<NetworkObject>();
            if (networkObjectSelected != null)
            {
                RequestGrabbableOwnershipServerRpc(
                    OwnerClientId, networkObjectSelected
                );
            }
        }
        else 
        {
            Debug.Log("Not Owner: Ownership cannot be requested");
        }
    }

    [ServerRpc]
    public void RequestGrabbableOwnershipServerRpc(
        ulong newOwnerClientId, NetworkObjectReference networkObjectReference
    ) {
        if (networkObjectReference.TryGet(out NetworkObject networkObject))
        {
            Debug.Log($"Server: Object ownership requested by {newOwnerClientId}");

            networkObject.ChangeOwnership(newOwnerClientId);
            Debug.Log($"Server: Object ownership changed to {newOwnerClientId}");
        }
        else
        {
            Debug.Log("Server: Unable to change ownership");
        }
    }

    public void OnSelectExited(SelectExitEventArgs eventArgs)
    {
        // Nothing to do
    }
}
