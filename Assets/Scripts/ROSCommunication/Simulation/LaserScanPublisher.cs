using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

/// <summary>
///     This script publishes laser scan messages to ROS.
/// </summary>
public class LaserScanPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string laserTopicName = "base_scan";
    [SerializeField] private string laserLinkId = "laser_link";

    // Sensor
    [SerializeField] private Laser laser;

    // Message
    private LaserScanMsg laserScan;
    // rate
    [SerializeField] private int publishRate = 10;
    private Timer timer = new Timer(10);

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<LaserScanMsg>(laserTopicName);

        // Initialize messages
        var (updateRate, samples, angleMin, angleMax, rangeMin, rangeMax) = 
            laser.GetLaserScanParameters();
        float angleIncrement = (angleMax - angleMin) / (samples-1);
        float scanTime = 1f / updateRate;
        float timeIncrement = 0;  // assume no time between scans
        float[] intensities = new float[laser.ObstacleRanges.Length];
        laserScan = new LaserScanMsg
        {
            header = new HeaderMsg(
                0, new TimeStamp(Clock.time), laserLinkId
            ),
            angle_min       = angleMin,
            angle_max       = angleMax,
            angle_increment = angleIncrement,
            time_increment  = timeIncrement,
            scan_time       = scanTime,
            range_min       = rangeMin,
            range_max       = rangeMax,
            ranges          = laser.ObstacleRanges,      
            intensities     = intensities
        };

        // Use event based publishing to publish messages.
        // The publish rate is controlled by both 
        // the laser update rate, and the self publish rate.
        laser.ScanFinishedEvent += PublishScan;
        timer = new Timer(publishRate);
    }

    void FixedUpdate() 
    {
        timer.UpdateTimer(Time.fixedDeltaTime);
    }

    void OnDestroy()
    {
        laser.ScanFinishedEvent -= PublishScan;
    }

    private void PublishScan()
    {
        if (!timer.ShouldProcess)
        {
            return;
        }
        timer.ShouldProcess = false;

        // Publish message
        laserScan.header.Update();
        laserScan.ranges = laser.ObstacleRanges;

        ros.Publish(laserTopicName, laserScan);
    }
}
