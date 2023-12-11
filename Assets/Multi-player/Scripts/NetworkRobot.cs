using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

/// <summary>
///    Defines a network robot
///    Adjust joints if not owner
/// </summary>
public class NetworkRobot : NetworkBehaviour
{
    // Arictulation scripts under which to disable
    [SerializeField] private GameObject plugins;
    // Articulation scripts whom should not be disabled
    [SerializeField] private GameObject[] pluginsToKeep;

    // Robot
    [SerializeField] private GameObject robotRoot;
    private ArticulationBody[] articulationChain;
    private ArticulationDrive[] drives;
    [SerializeField, ReadOnly] private float[] jointAngles;

    // Robot joint angles Struct
    private struct Joints : INetworkSerializable {
        public float[] jointAngles;
        public void NetworkSerialize<T>(
            BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref jointAngles);
        }
    }
    // give read permissions to everyone and
    // give write permissions to the owner
    private NetworkVariable<Joints> networkJointAngles = 
        new NetworkVariable<Joints>(
            new Joints { jointAngles = new float[0] },
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Owner
        );

    public override void OnNetworkSpawn()
    {
        // Initialization
        InitArticulationBody();
        InitJointAngles();

        // Disable plugins if not owner
        DisablePlugins();
    }

    private void InitArticulationBody()
    {
        // Initialize articulation body
        // get non-fixed joints
        articulationChain = robotRoot.
            GetComponentsInChildren<ArticulationBody>();
        articulationChain = articulationChain.Where(
            joint => joint.jointType != ArticulationJointType.FixedJoint
        ).ToArray();
        // get drives
        drives = new ArticulationDrive[articulationChain.Length];
        for (int i = 0; i < articulationChain.Length; ++i)
        {
            drives[i] = articulationChain[i].xDrive;
        }
    }

    private void InitJointAngles()
    {
        // Init joint angles
        jointAngles = new float[articulationChain.Length];
        ReadJointAngles();
        networkJointAngles.Value = new Joints { 
            jointAngles = jointAngles 
        };
    }

    private void DisablePlugins()
    {
        // Disable game object if not the owner and
        // if not in the ToKeep list
        if (!IsOwner)
        {
            foreach (Transform plugin in plugins.transform)
            {
                // If the child is not in the ToKeep list
                plugin.gameObject.SetActive(false);
            }
            foreach (GameObject plugin in pluginsToKeep)
            {
                plugin.SetActive(true);
            }
        }
    }

    void FixedUpdate()
    {
        // Owner, update joint angles
        if (IsOwner)
        {
            ReadJointAngles();
            networkJointAngles.Value = new Joints { 
                jointAngles = jointAngles 
            };
        }

        // Non-owner, update the robot joints based on the value
        else
        {
            jointAngles = networkJointAngles.Value.jointAngles;
            SetJointAngles();
        }
    }

    private void ReadJointAngles()
    {
        for (int i = 0; i < articulationChain.Length; ++i)
        {
            jointAngles[i] = articulationChain[i].jointPosition[0];
        }
    }

    private void SetJointAngles()
    {
        if (jointAngles.Length != articulationChain.Length)
        {
            return;
        }

        for (int i = 0; i < articulationChain.Length; ++i)
        {
            drives[i].target = jointAngles[i];
            articulationChain[i].xDrive = drives[i];
        }
    }
}
