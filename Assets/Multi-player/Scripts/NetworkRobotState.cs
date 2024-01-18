using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;
using Unity.Robotics.UrdfImporter;

/// <summary>
///    Defines a network robot state that is used to synchronize the robot
///    between the server and the clients. (server -> client)
///    
///    Send robot's position, rotation and joints to the clients
///    Disable the plugins and colliders on the clients
///    Disable the camera if not the owner
///    
///    Sound is not synchronized yet
/// </summary>
public class NetworkRobotState : NetworkBehaviour
{
    // Articulation scripts under which to disable
    [SerializeField] private GameObject plugins;
    // Articulation scripts whom should not be disabled
    [SerializeField] private GameObject[] pluginsToKeep;

    // Robot
    [SerializeField] private GameObject articulationRoot;
    [SerializeField] private ArticulationBody[] articulationBodyToIgnore;
    private ArticulationBody[] articulationChain;

    // Network robot status
    [SerializeField, ReadOnly] 
    private NetworkVariable<Vector3> position = 
        new NetworkVariable<Vector3>(Vector3.zero);
    [SerializeField, ReadOnly] 
    private NetworkVariable<Quaternion> rotation = 
        new NetworkVariable<Quaternion>(Quaternion.identity);
    [SerializeField, ReadOnly]
    private NetworkList<float> jointAngles;
    [SerializeField, ReadOnly]  // for visualization
    private float[] jointAnglesArray = new float[0];

    void Awake()
    {
        jointAngles = new NetworkList<float>(new List<float>());
    }

    public override void OnNetworkSpawn()
    {
        // Initialization
        InitArticulationBody();
        // Disable plugins depending on the roles
        DisablePlugins();

        // Initialize network variable if is server
        if (IsServer)
        {
            position.Value = articulationRoot.transform.position;
            rotation.Value = articulationRoot.transform.rotation;
            jointAngles.Clear();
        }
        // visualization array
        jointAnglesArray = new float[articulationChain.Length];
    }

    private void InitArticulationBody()
    {
        // Initialize articulation body
        // get non-fixed joints and non-ignored joints
        articulationChain = articulationRoot.
            GetComponentsInChildren<ArticulationBody>();
        articulationChain = articulationChain.Where(joint => 
            joint.jointType != ArticulationJointType.FixedJoint
            && !articulationBodyToIgnore.Contains(joint)
        ).ToArray();
    }

    private void DisablePlugins()
    {
        // Disable game object control if not the server
        // and if not in the ToKeep list
        if (!IsServer)
        {
            // Disable scripts
            foreach (Transform plugin in 
                plugins.GetComponentsInChildren<Transform>())
            {
                plugin.gameObject.SetActive(false);
            }
            foreach (GameObject plugin in pluginsToKeep)
            {
                plugin.SetActive(true);
            }

            // Disable colliders
            foreach (var collider in
                articulationRoot.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }
        }

        // Disable camera if not the owner
        if (!IsOwner)
        {
            // disable cameras
            foreach(var camera in GetComponentsInChildren<Camera>())
            {
                camera.gameObject.SetActive(false);
            }
        }
    }

    void FixedUpdate()
    {
        // Server, update joint angles
        if (IsServer)
        {
            // read joint angles from articulation body
            ReadRobotStatus();
        }
        // Non-server, update the robot joints based on the value
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
        position.Value = articulationRoot.transform.position;
        rotation.Value = articulationRoot.transform.rotation;
        jointAngles.Clear();
        jointAnglesArray = new float[articulationChain.Length];

        for (int i = 0; i < articulationChain.Length; ++i)
        {
            jointAngles.Add(articulationChain[i].jointPosition[0]);
            jointAnglesArray[i] = articulationChain[i].jointPosition[0];
        }
    }

    private void TeleportRobot()
    {
        // No need to teleport
        if (articulationRoot.transform.position == position.Value
            && articulationRoot.transform.rotation == rotation.Value)
        {
            return;
        }

        // Teleport robot
        var root = articulationRoot.GetComponent<ArticulationBody>();
        root.TeleportRoot(position.Value, rotation.Value);
    }

    private void SetJointAngles()
    {
        if (jointAngles.Count != articulationChain.Length)
        {
            // Debug.Log("Joint angles count does not match");
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

            // Update joint angles value for visualization
            jointAnglesArray[i] = jointAngles[i];
        }
    }
}
