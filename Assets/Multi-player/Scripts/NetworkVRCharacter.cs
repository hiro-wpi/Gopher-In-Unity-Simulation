using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

/// <summary>
///    Defines a network player.
///    Disables client input for non-owning clients.
/// </summary>
public class NetworkVRCharacter : NetworkBehaviour
{
    [SerializeField] GameObject XROrigin;
    // [SerializeField] GameObject[] XROriginTrackingTargets;

    public override void OnNetworkSpawn()
    {
        // // Add client transform sync to tracking targets
        // foreach (var target in XROriginTrackingTargets)
        // {
        //     var netTf = target.AddComponent<ClientNetworkTransform>();
        //     netTf.SyncScaleX = false;
        //     netTf.SyncScaleY = false;
        //     netTf.SyncScaleZ = false;
        // }

        // Disable non-owner input
        if (IsClient && !IsOwner)
        {
            DisableNonOwnerInput();
        }
    }

    private void DisableNonOwnerInput()
    {
        // Disable all components in XR origin to stop getting undesired input
        // Keep only the network related components
        foreach (
            var component in XROrigin.GetComponentsInChildren<MonoBehaviour>()
        ) {
            if (!(component is NetworkObject) 
                && !(component is ClientNetworkTransform)
            ) {
                component.enabled = false;
            }
        }

        // Prevent owner's VR controller assets from being disabled
        var actionManager = XROrigin.GetComponent<InputActionManager>();
        actionManager?.EnableInput();
    }

    // public override void OnNetworkSpawn() => DisableClientInput();

    // private void DisableClientInput()
    // {
    //     if (IsClient && !IsOwner)
    //     {
    //         XROrigin.SetActive(false);

    //         // Prevent controller assets from being disabled
    //         InputActionManager actionManager = XROrigin.GetComponent<InputActionManager>();
    //         actionManager.EnableInput();
    //     }
    // }
}