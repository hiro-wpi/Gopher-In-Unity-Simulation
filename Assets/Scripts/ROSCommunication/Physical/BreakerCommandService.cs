using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Power;


/***
This was intended for controlling the breakers on the freight research base.
The breakers we are concerned about are:

/base_breaker
/aux1_breaker
/aux2_breaker

Replace the name in Unity accourdingly
***/


public class BreakerCommandService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string breakerName = "base_breaker";

    // Message
    private BreakerCommandRequest breakerCommand;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<BreakerCommandRequest, BreakerCommandResponse>(
            breakerName
        );
    }

    void Update() {}

    // Kill the breaker
    public void KillBreaker()
    {
        SendBreakerCommandService(false);
    }

    // activate the breaker
    public void activateBreaker()
    {
        SendBreakerCommandService(true);
    }

    // Reset the breaker
    public void resetBreaker()
    {
        SendBreakerCommandService(false);
        SendBreakerCommandService(true);
    }


    // Send Command to the breaker
    public void SendBreakerCommandService(bool command)
    {
        BreakerCommandRequest breakerRequest = new BreakerCommandRequest(command);
        ros.SendServiceMessage<BreakerCommandResponse>(
            breakerName, breakerRequest, breakerCallback
        );
    }

    public void breakerCallback(BreakerCommandResponse response)
    {
        // Debug.Log("breaker command response: " + response.status);
    }


    // Turn on the breaker
    // Reset the breaker

    // // Request service to home the chest
    // public void SendChestHomeService()
    // {
    //     // homeCommand.command = true;
    //     HomingRequest homingRequest = new HomingRequest(true);
    //     // Request service
    //     ros.SendServiceMessage<HomingResponse>(
    //         chestHomeServiceName, homingRequest, HomeCommandCallback
    //     );
    // }

    // // Callback function for service response
    // private void HomeCommandCallback(HomingResponse response)
    // {
    //     // Debug.Log("Home command response: " + response.state);
    // }
}
