using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.GopherRosClearcore;


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
    public void SendChestHomeService()
    {
        // homeCommand.command = true;
        HomingRequest homingRequest = new HomingRequest(true);
        // Request service
        ros.SendServiceMessage<HomingResponse>(
            chestHomeServiceName, homingRequest, HomeCommandCallback
        );
    }

    // Callback function for service response
    private void HomeCommandCallback(HomingResponse response)
    {
        // Debug.Log("Home command response: " + response.state);
    }
}
