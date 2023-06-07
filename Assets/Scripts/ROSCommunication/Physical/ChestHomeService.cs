using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.GopherRosClearcore;

/// <summary>
///     This script sends a service request to home the chest
/// <summary>
public class ChestHomeService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string chestHomeServiceName = "z_chest_home";

    // Message
    private HomingRequest homeCommand;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<HomingRequest, HomingResponse>(
            chestHomeServiceName
        );
    }

    void Update() {}

    // Request service to home the chest
    public void SendChestHomeRequest()
    {
        HomingRequest homingRequest = new HomingRequest(true);
        // Request service
        ros.SendServiceMessage<HomingResponse>(
            chestHomeServiceName, homingRequest, HomeCommandCallback
        );
    }

    private void HomeCommandCallback(HomingResponse response) {}
}
