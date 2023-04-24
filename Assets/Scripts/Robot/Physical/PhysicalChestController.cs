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
        Vector3 velocity = new Vector3(0, speedFraction, 0);
        twistPublisher.PublishTwist(velocity, new Vector3(0,0,0));
    }

    public override void StopChest()
    {
        twistPublisher.PublishTwist(Vector3.zero, Vector3.zero);
    }

    public override void HomeChest()
    {
        controlMode = ControlMode.Position;
        homeService.SendChestHomeService();
        controlMode = ControlMode.Speed;
    }

    // TODO
    // Send command to move to pre-defined positions
    public override void MoveToPreset(int presetIndex) 
    {

    }
    
    // Emergency stop
    public override void EmergencyStop()
    {
        stopService.SendChestStopService();
    }

    public override void EmergencyStopResume() 
    {
        // Physical motorized chest 
        // does not need to resume from emergency stop
    }
}
