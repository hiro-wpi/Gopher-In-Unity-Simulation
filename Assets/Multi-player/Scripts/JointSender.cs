using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class JointSender : NetworkBehaviour
{
    // Joints constructor
    // Give read permissions to everyone and give write permissions to the owner and the server
    private NetworkVariable<Joints> jointList = new NetworkVariable<Joints>(
        new Joints {
            leftJoint = 10,
            rightJoint = 15,
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    // Struct (Joints type) containing joints (float type)
    public struct Joints : INetworkSerializable {
        public float leftJoint;
        public float rightJoint;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref leftJoint);
            serializer.SerializeValue(ref rightJoint);
        }
    }

    // Only send values to network upon updates
    public override void OnNetworkSpawn()
    {
        jointList.OnValueChanged += (Joints previousValue, Joints newValue) => {
            Debug.Log(OwnerClientId + "; joint list: " + newValue.leftJoint + "; " + newValue.rightJoint);
        };
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.T)) 
            {
                // Update joint values below
                jointList.Value = new Joints {
                    leftJoint = 12,
                    rightJoint = 17,
                };
            }
        }
    }
}
