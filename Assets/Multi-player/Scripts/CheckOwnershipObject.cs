using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CheckOwnershipObject : MonoBehaviour
{
    public NetworkObject networkObject;
    public ulong ownerId;
    // Update is called once per frame
    void FixedUpdate()
    {
        ownerId = networkObject.OwnerClientId;
    }
}
