using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using Unity.Netcode;
using System;

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
    // Robot
    [Header("Robot")]
    [SerializeField] private GameObject articulationRoot;
    [SerializeField] private ArticulationBody[] articulationBodyToIgnore = 
        new ArticulationBody[0];
    private ArticulationBody[] articulationChain = new ArticulationBody[0];

    [Header("Non-owner")]
    // Only owner needs to read the input from input action assets
    [SerializeField] private InputActionAsset[] inputActionsToDisable = 
        new InputActionAsset[0];

    [Header("Client")]
    // Only server needs to actually control the robot
    [SerializeField] private GameObject pluginsRootToDisable;
    [SerializeField] private GameObject[] pluginsToKeep = new GameObject[0];

    // Network robot status
    [Header("Network Robot Status")]
    [SerializeField, ReadOnly] 
    private NetworkVariable<Vector3> position = 
        new NetworkVariable<Vector3>(Vector3.zero);
    [SerializeField, ReadOnly] 
    private NetworkVariable<Quaternion> rotation = 
        new NetworkVariable<Quaternion>(Quaternion.identity);
    [SerializeField, ReadOnly]
    private NetworkList<float> jointList;
    // for visualization
    [SerializeField, ReadOnly]
    private float[] jointAngles = new float[0];

    // For smooth following
    [SerializeField] private bool interpolateBase = true;
    [SerializeField] private bool interpolateJoint = true;
    [SerializeField] private float interpolationBaseTime = 0.05f;
    [SerializeField] private float interpolationJointTime = 0.05f;
    private Vector3 postionVelocity = Vector3.zero;
    private Quaternion rotationVelocity = Quaternion.identity;
    private float[] smoothVelocities = new float[0];

    void Awake()
    {
        jointList = new NetworkList<float>(new List<float>());
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
            jointList.Clear();
        }
        // For smooth following if it is client
        else
        {
            smoothVelocities = new float[articulationChain.Length];
        }
        // visualization array
        jointAngles = new float[articulationChain.Length];
    }

    private void InitArticulationBody()
    {
        // Initialize articulation body
        articulationChain = articulationRoot.
            GetComponentsInChildren<ArticulationBody>();

        articulationChain = articulationChain.Where(joint =>
            // get non-fixed joints
            joint.jointType != ArticulationJointType.FixedJoint
            // get non-ignored joints
            && !articulationBodyToIgnore.Contains(joint)
        ).ToArray();
    }

    private void DisablePlugins()
    {
        // Disable control interface and cameras if not the owner
        if (!IsOwner)
        {
            // disable input
            foreach (var input in inputActionsToDisable)
            {
                input.Disable();
            }

            // disable cameras
            foreach(var camera in GetComponentsInChildren<Camera>())
            {
                camera.gameObject.SetActive(false);
            }
        }

        // Disable game object control if not the server
        // and if not in the ToKeep list
        if (!IsServer)
        {
            // Disable scripts
            foreach (Transform plugin in 
                pluginsRootToDisable.GetComponentsInChildren<Transform>())
            {
                plugin.gameObject.SetActive(false);
            }
            foreach (GameObject plugin in pluginsToKeep)
            {
                plugin.SetActive(true);
            }

            // Disable gravity
            foreach (var body in 
                articulationRoot.GetComponentsInChildren<ArticulationBody>())
            {
                body.useGravity = false;
            }
            // Disable colliders
            foreach (var collider in
                articulationRoot.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
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
        jointList.Clear();
        jointAngles = new float[articulationChain.Length];

        for (int i = 0; i < articulationChain.Length; ++i)
        {
            jointList.Add(articulationChain[i].jointPosition[0]);
            jointAngles[i] = articulationChain[i].jointPosition[0];
        }
    }

    private void TeleportRobot()
    {
        // No need to teleport
        if (
            Vector3.Distance(
                articulationRoot.transform.position, position.Value
            ) < 0.001f
            && Quaternion.Angle(
                articulationRoot.transform.rotation, rotation.Value
            ) < 0.01f
        ) {
            return;
        }

        Vector3 targetPosition = position.Value;
        Quaternion targetRotation = rotation.Value;

        // Smoothly move the robot to the target
        if (interpolateBase)
        {
            targetPosition = Vector3.SmoothDamp(
                articulationRoot.transform.position,
                position.Value,
                ref postionVelocity,
                interpolationBaseTime
            );
            targetRotation = Utils.QuaternionSmoothDamp(
                articulationRoot.transform.rotation,
                rotation.Value,
                ref rotationVelocity,
                interpolationBaseTime
            );
        }

        // Teleport robot
        var root = articulationRoot.GetComponent<ArticulationBody>();
        root.TeleportRoot(targetPosition, targetRotation);
    }

    private void SetJointAngles()
    {
        if (jointList.Count != articulationChain.Length)
        {
            // Debug.Log("Joint angles count does not match");
            return;
        }

        for (int i = 0; i < articulationChain.Length; ++i)
        {
            float target = jointList[i];

            // Smoothly move the joint to the target
            if (interpolateJoint)
            {
                target = Mathf.SmoothDamp(
                    articulationChain[i].jointPosition[0],
                    jointList[i],
                    ref smoothVelocities[i],
                    interpolationJointTime
                );
            }

            // If joint is revolute, convert to degree
            if (articulationChain[i].jointType 
                == ArticulationJointType.RevoluteJoint)
            {
                target *= Mathf.Rad2Deg;
            }
            ArticulationBodyUtils.SetJointTarget(
                articulationChain[i], target
            );

            // Update joint angles value for visualization
            jointAngles[i] = jointList[i];
        }
    }
}
