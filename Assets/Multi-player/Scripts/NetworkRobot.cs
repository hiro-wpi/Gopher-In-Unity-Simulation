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

    // Joint angles
    [SerializeField, ReadOnly] private float[] jointAngles = new float[0];
    // Network joint angles struct
    private struct Joints : INetworkSerializable {
        public float[] jointAngles;

        public void NetworkSerialize<T>(
            BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref jointAngles);
        }
    }
    private Joints jointsStruct = new Joints(){ 
        jointAngles = new float[0]
    };

    public override void OnNetworkSpawn()
    {
        // Initialization
        InitArticulationBody();
        ReadJointAngles();

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
            UpdateJointsServerRpc(jointsStruct);
        }
        // Non-owner, update the robot joints based on the value
        else
        {
            SetJointAngles(jointsStruct.jointAngles);
        }
    }

    private void ReadJointAngles()
    {
        jointAngles = new float[articulationChain.Length];
        for (int i = 0; i < articulationChain.Length; ++i)
        {
            jointAngles[i] = articulationChain[i].jointPosition[0];
        }
        jointsStruct.jointAngles = jointAngles;
    }

    private void SetJointAngles(float[] jointAngles)
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

    [ServerRpc]
    private void UpdateJointsServerRpc(Joints joints)
    {
        // Update server side
        if (!IsOwner)
        {
            jointAngles = joints.jointAngles;
        }
        // Update client side
        UpdateJointsClientRpc(joints);
    }

    [ClientRpc]
    private void UpdateJointsClientRpc(Joints joints)
    {
        if (!IsOwner)
        {
            jointAngles = joints.jointAngles;
        }
    }
}
