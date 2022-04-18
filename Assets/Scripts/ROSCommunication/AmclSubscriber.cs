using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;

/// <summary>
///     This script subscribes to twist command
///     and use robot controller to 
/// </summary>
public class AmclSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    public string AmclTopicName = "/amcl_pose";

    private double init_x = 0.0;
    private double init_y = 0.0;

    private double delta_x;
    private double delta_y;

    private double v_x;
    private double v_y;
    private double linear_vel;

    private double init_time;
    private double curr_time; 
    private double delta_time;

    public ArticulationWheelController wheelController;
    
    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        
        ros.Subscribe<PoseWithCovarianceStampedMsg>(AmclTopicName, updatePose);
        init_time = Time.time; 

    }

    void LateUpdate(){
    }

    private void FixedUpdate()
    {
        // wheelController.SetRobotVelocity(targetLinearVelocity, targetAngularVelocity);
    }

    private void updatePose(PoseWithCovarianceStampedMsg pose)
    {
        double x = pose.pose.pose.position.x;
        double y = pose.pose.pose.position.y;

        delta_x = x - init_x;
        delta_y = y - init_y;

        curr_time = Time.time;
        delta_time = init_time-curr_time;
        init_time = curr_time;

        v_x = delta_x/delta_time;
        v_y = delta_y/delta_time;
        linear_vel = Math.Sqrt(v_x*v_x + v_y*v_y);

        Debug.Log(linear_vel);


    }
}
