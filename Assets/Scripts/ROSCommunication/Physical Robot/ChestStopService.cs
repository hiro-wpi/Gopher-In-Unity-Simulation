using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.GopherRosClearcore;

/// <summary>
///     This script sends a service request to home the chest
/// <summary>
public class ChestStopService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string chestStopServiceName = "z_chest_stop";

    // Message
    private HomingRequest stopCommand;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<StopRequest, StopResponse>( 
            chestStopServiceName
        );
    }

    void Update() {}

    // Request service to home the chest
    public void SendChestStopRequest()
    {
        StopRequest stopRequest = new StopRequest(true);
        // Request service
        ros.SendServiceMessage<StopResponse>(
            chestStopServiceName, stopRequest, StopCommandCallback
        );
    }

    private void StopCommandCallback(StopResponse response) {}
}
