using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;

/// <summary>
///     This script subscribes to twist command
///     and use base controller to move the base
/// </summary>
public class TwistSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string twistTopicName = "cmd_vel";

    // public ArticulationWheelController wheelController;
    [SerializeField] private ArticulationBaseController baseController;
    private Vector3 targetLinearSpeed;
    private Vector3 targetAngularSpeed;
    
    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();

        // Init
        targetLinearSpeed = new Vector3();
        targetAngularSpeed = new Vector3();
        
        ros.Subscribe<TwistMsg>(twistTopicName, UpdateVelocity);
    }
    
    void FixedUpdate()
    {
        baseController.SetVelocity(targetLinearSpeed, targetAngularSpeed);
    }

    private void UpdateVelocity(TwistMsg twist)
    {
        targetLinearSpeed = twist.linear.From<FLU>();
        targetAngularSpeed = twist.angular.From<FLU>();
    }
}
