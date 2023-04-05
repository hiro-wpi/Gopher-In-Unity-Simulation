using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

/// <summary>
///     This script subscribes laser scan
/// </summary>
public class LaserScanSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string laserScanTopicName = "base_scan";

    // laserScan
    private LaserScanMsg laserScan;
    private bool islaserScanReceived;

    // Container
    private float[] scanAngleInfo;
    [field:SerializeField, ReadOnly] public float[] Angles { get; private set; }
    [field:SerializeField, ReadOnly] public float[] Ranges { get; private set; }

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();

        // Initialize laserScan
        // islaserScanReceived = false;

        // Subscriber
        ros.Subscribe<LaserScanMsg>(laserScanTopicName, ReceiveLaserScan);
    }

    void Update() { }

    private void ReceiveLaserScan(LaserScanMsg laserScan)
    {   
        // Store range data
        Ranges = laserScan.ranges;

        // Store angle data
        // if first time or the scan angle info is changed
        if ((scanAngleInfo == null) || 
            (scanAngleInfo[0] != laserScan.ranges.Length) || 
            (scanAngleInfo[1] != laserScan.angle_min) || 
            (scanAngleInfo[2] != laserScan.angle_increment))
        {
            // update angle data
            Angles = new float[laserScan.ranges.Length];
            // negate the angle as it is right-handed in ROS
            float angle = laserScan.angle_min;
            for (int i = 0; i < laserScan.ranges.Length; i++)
            {
                Angles[i] = angle;
                angle += laserScan.angle_increment;
            }
            // save angle info
            scanAngleInfo = new float[3];
            scanAngleInfo[0] = laserScan.ranges.Length;
            scanAngleInfo[1] = laserScan.angle_min;
            scanAngleInfo[2] = laserScan.angle_increment;
        }
    }
}
