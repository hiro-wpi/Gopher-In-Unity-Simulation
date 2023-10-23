using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Rosgraph;

/// <summary>
///     This script publishes simulation time
/// </summary>
public class ROSClockPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;

    // Clock mode
    [SerializeField, ReadOnly] private Clock.ClockMode clockMode;

    // Message
    private TimeMsg timeMsg;
    // rate
    [SerializeField] private int publishRate = 50;
    private Timer timer = new Timer(50);

    // Only one Clock is allowed
    void OnValidate()
    {
        var clocks = FindObjectsOfType<ROSClockPublisher>();
        if (clocks.Length > 1)
        {
            Debug.LogWarning(
                "There should only be one clock publishers in the scene" + 
                "Using the last one found."
            );
            for (int i = 0; i < clocks.Length - 1; i++)
            {
                clocks[i].enabled = false;
            }
        }
    }

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ClockMsg>("clock");

        Clock.Mode = clockMode;

        // Initialize message
        timeMsg = new TimeMsg();

        // Rate
        timer = new Timer(publishRate);
    }

    void FixedUpdate()
    {
        timer.UpdateTimer(Time.fixedDeltaTime);
        if (timer.ShouldProcess)
        {
            PublishClock();
            timer.ShouldProcess = false;
        }
    }

    private void PublishClock()
    {
        TimeStamp timeStamp = new TimeStamp(Clock.time);
        timeMsg = new TimeMsg
        {
            sec = timeStamp.Seconds,
            nanosec = timeStamp.NanoSeconds
        };

        ros.Publish("clock", timeMsg);
    }
}
