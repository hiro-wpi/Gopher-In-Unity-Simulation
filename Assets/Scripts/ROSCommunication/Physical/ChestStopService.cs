using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.GopherRosClearcore;


public class ChestStopService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string chestStopServiceName = "z_chest_stop";

    // Message
    private HomingRequest stopCommand;

    // Start is called before the first frame update
    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<StopRequest, StopResponse>( 
            chestStopServiceName
        );
    }

    void Update() { }

    // Request service to home the chest
    public void SendChestStopService()
    {
        Debug.Log("Stopping ...");

        StopRequest stopRequest = new StopRequest(true);
        // Request service
        ros.SendServiceMessage<StopResponse>(
            chestStopServiceName, stopRequest, StopCommandCallback
        );
    }

    // Callback function for service response
    private void StopCommandCallback(StopResponse response)
    {
        Debug.Log("Chest command response: " + response.state);
    }
}
