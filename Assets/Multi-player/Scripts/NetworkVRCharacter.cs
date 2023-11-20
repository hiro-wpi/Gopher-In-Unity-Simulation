using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

/// <summary>
///    Defines a network player.
///    Disables client input for non-owning clients.
/// </summary>
public class NetworkVRCharacter : NetworkBehaviour
{
    [SerializeField] GameObject XROrigin;

    public override void OnNetworkSpawn()
    {
        DisableNonOwnerInput();
    }

    private void DisableNonOwnerInput()
    {
        // TODO there could be better solution
        if (!IsOwner)
        {
            // Disable input
            XROrigin.SetActive(false);

            // Prevent owner's VR controller assets from being disabled
            var actionManager = XROrigin.GetComponent<InputActionManager>();
            actionManager?.EnableInput();
        }
    }
}
