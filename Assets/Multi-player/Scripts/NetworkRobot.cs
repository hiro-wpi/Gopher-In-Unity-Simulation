using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

/// <summary>
///    Defines a network robot
///    
///    Send robot's position, rotation and joints to the server if owner
///    Adjust robot's position, rotation and joints if not owner
/// </summary>
public class NetworkRobot : NetworkBehaviour
{
    // Arictulation scripts under which to disable
    [SerializeField] private GameObject plugins;
    // Articulation scripts whom should not be disabled
    [SerializeField] private GameObject[] pluginsToKeep;

    // Robot
    [SerializeField] private GameObject articulationRoot;
    [SerializeField] private ArticulationBody[] articulationBodyToIgnore;
    private ArticulationBody[] articulationChain;

    // Network robot status
    [SerializeField, ReadOnly] 
    private Vector3 position = Vector3.zero;
    [SerializeField, ReadOnly] 
    private Quaternion rotation = Quaternion.identity;
    [SerializeField, ReadOnly] 
    private float[] jointAngles = new float[0];

    // Network joint angles struct
    private struct Robot : INetworkSerializable {
        public Vector3 position;
        public Quaternion rotation;
        public float[] jointAngles;

        public void NetworkSerialize<T>(
            BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref jointAngles);
        }
    }
    private Robot RobotStruct = new Robot() 
    {
        position = Vector3.zero,
        rotation = Quaternion.identity,
        jointAngles = new float[0]
    };

    public override void OnNetworkSpawn()
    {
        // Initialization
        InitArticulationBody();
        ReadRobotStatus();

        // Disable plugins if not owner
        DisablePlugins();
    }

    private void InitArticulationBody()
    {
        // Initialize articulation body
        // get non-fixed joints
        articulationChain = articulationRoot.
            GetComponentsInChildren<ArticulationBody>();
        articulationChain = articulationChain.Where(joint => 
            joint.jointType != ArticulationJointType.FixedJoint
            && !articulationBodyToIgnore.Contains(joint)
        ).ToArray();
    }

    private void DisablePlugins()
    {
        // Disable game object if not the owner and
        // if not in the ToKeep list
        if (!IsOwner)
        {
            // disable scripts
            foreach (Transform plugin in 
                plugins.GetComponentsInChildren<Transform>())
            {
                plugin.gameObject.SetActive(false);
            }
            foreach (GameObject plugin in pluginsToKeep)
            {
                plugin.SetActive(true);
            }

            // disable cameras
            foreach(var camera in GetComponentsInChildren<Camera>())
            {
                camera.gameObject.SetActive(false);
            }
        }
    }

    void FixedUpdate()
    {
        // Owner, update joint angles
        if (IsOwner)
        {
            // read joint angles from articulation body
            ReadRobotStatus();

            // sync
            RobotStruct.position = position;
            RobotStruct.rotation = rotation;
            RobotStruct.jointAngles = jointAngles;
            UpdateRobotServerRpc(RobotStruct);
        }
        // Non-owner, update the robot joints based on the value
        else
        {
            // move robot
            TeleportRobot();
            // set joint angles to articulation body
            SetJointAngles();
        }
    }

    private void ReadRobotStatus()
    {
        position = articulationRoot.transform.position;
        rotation = articulationRoot.transform.rotation;

        jointAngles = new float[articulationChain.Length];
        for (int i = 0; i < articulationChain.Length; ++i)
        {
            jointAngles[i] = articulationChain[i].jointPosition[0];
        }
    }

    private void TeleportRobot()
    {
        // No need to teleport
        if (articulationRoot.transform.position == position
            && articulationRoot.transform.rotation == rotation)
        {
            return;
        }

        // Teleport robot
        var root = articulationRoot.GetComponent<ArticulationBody>();
        root.TeleportRoot(position, rotation);
    }

    private void SetJointAngles()
    {
        if (jointAngles.Length != articulationChain.Length)
        {
            return;
        }

        for (int i = 0; i < articulationChain.Length; ++i)
        {
            // If joint is revolute, convert to degree
            float target = (
                articulationChain[i].jointType 
                == ArticulationJointType.RevoluteJoint
            ) ? jointAngles[i] * Mathf.Rad2Deg : jointAngles[i];

            ArticulationBodyUtils.SetJointTarget(
                articulationChain[i], target
            );
        }
    }

    [ServerRpc]
    private void UpdateRobotServerRpc(Robot robot)
    {
        // Update server side
        if (!IsOwner)
        {
            position = robot.position;
            rotation = robot.rotation;
            jointAngles = robot.jointAngles;
        }
        // Update client side
        UpdateRobotClientRpc(robot);
    }

    [ClientRpc]
    private void UpdateRobotClientRpc(Robot robot)
    {
        if (!IsOwner)
        {
            position = robot.position;
            rotation = robot.rotation;
            jointAngles = robot.jointAngles;
        }
    }
}
