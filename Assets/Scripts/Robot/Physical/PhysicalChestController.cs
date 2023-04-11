using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script send Unity input for chest control 
///     to ROS as Twist message.
///
///     The current velocity is published to ROS at a fixed rate.
/// </summary>
public class PhysicalChestController : ChestController
{
    // ROS communication
    [SerializeField] private TwistPublisher twistPublisher;
    [SerializeField] private ChestHomeService homeService;
    [SerializeField] private ChestStopService stopService;
    [SerializeField] private BreakerCommandService chestBreakerService;

    // Velocity publish rate
    [SerializeField] protected int publishRate = 60;

    
    
    void Start()
    {
        // Keep publishing the velocity at a fixed rate
        InvokeRepeating("PublishVelocity", 1.0f, 1.0f / publishRate);
    }

    void Update() {}

    // Publish the current velocity
    private void PublishVelocity()
    {
        if (controlMode != ControlMode.Speed) 
        {
            return;
        }
        // Publish to ROS
        Debug.Log("PhysicalChestController << PublishVelocity Function");
        Debug.Log(speedFraction);
        Vector3 velocity = new Vector3(0, speedFraction, 0);
        twistPublisher.PublishTwist(velocity, new Vector3(0,0,0));
    }

    public override void StopChest()
    {
        // stopService.SendChestStopService();
        twistPublisher.PublishTwist(Vector3.zero, Vector3.zero);
    }

    public override void HomeChest()
    {
        controlMode = ControlMode.Position;
        homeService.SendChestHomeService();
        controlMode = ControlMode.Speed;
    }

    public override void MoveToPreset(int presetIndex) {}

    // Breaker
    // Issue with Intergration
    // public override void ChestBreaker(float value)
    // {
    //     if (value == 0)
    //     {
    //         chestBreakerService.SendBreakerCommandService(false);
    //     }
        
    //     if (value == 1)
    //     {
    //         chestBreakerService.SendBreakerCommandService(true);
    //     }
    // }
    
}
