using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Power;


/// <summary>
///     This was intended for controlling the breakers on the freight research base.
///     The breakers we are concerned about are:
///
///     /base_breaker
///     /aux_breaker_1
///     /aux_breaker_2
///
///     Replace the name in Unity accordingly
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
    public void ResetBreaker(float delay = 1f)
    {
        StartCoroutine(ResetBreakerCoroutine(delay));
    }
    
    private IEnumerator ResetBreakerCoroutine(float delay)
    {
        BreakerOff();
        yield return new WaitForSeconds(delay);
        BreakerOn();
    }

    // Send Command to the breaker
    public void SendBreakerCommandService(bool command)
    {
        BreakerCommandRequest breakerRequest = new BreakerCommandRequest(command);
        ros.SendServiceMessage<BreakerCommandResponse>(
            breakerName, breakerRequest, BreakerCallback
        );
    }

    private void BreakerCallback(BreakerCommandResponse response) {}
}
