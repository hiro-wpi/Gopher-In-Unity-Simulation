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
    // [SerializeField] private ServiceCaller serviceCaller;

    // Velocity update rate
    [SerializeField] protected int updateRate = 60;
    protected float deltaTime;
    
    void Start()
    {
        // Keep publishing the velocity at a fixed rate
        deltaTime = 1.0f / updateRate;
        InvokeRepeating("PublishVelocity", 1.0f, deltaTime);
    }

    void Update() { }

    // Publish the current velocity
    private void PublishVelocity()
    {
        // Publish to ROS
        Vector3 vel = new Vector3(0, velFraction, 0);
        twistPublisher.PublishTwist(vel, new Vector3(0,0,0));
    }

    public override void HomeChest()
    {
        homeService.SendChestHomeService();
    }

    public override void MoveToPreset(int presetIndex)
    {
        Debug.Log("Attempted to go to preset");
    }

    public override void StopChest()
    {
        // stopService.SendChestStopService();
        twistPublisher.PublishTwist(new Vector3(0,0,0), new Vector3(0,0,0));
    }
}