using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Power;


/// <summary>
/// This was intended for controlling the breakers on the freight research base.
/// The breakers we are concerned about are:

/// /base_breaker
/// /aux_breaker_1
/// /aux_breaker_2

/// Replace the name in Unity accourdingly
/// <summary>
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
    public void BreakerOff()
    {
        SendBreakerCommandService(false);
    }

    // Activate the breaker
    public void BreakerOn()
    {
        SendBreakerCommandService(true);
    }

    // Reset the breaker
    // TODO Add a delay in between sent messages, the breaker doesnt turn off fully
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

    public void breakerCallback(BreakerCommandResponse response) {}
}
