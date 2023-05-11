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
public class TwistSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    public string twistTopicName = "cmd_vel";

    // public ArticulationWheelController wheelController;
    public ArticulationBaseController baseController;
    private Vector3 targetLinearSpeed;
    private Vector3 targetAngularSpeed;

    private bool pause = false;
    
    
    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();

        targetLinearSpeed = new Vector3();
        targetAngularSpeed = new Vector3();
        
        ros.Subscribe<TwistMsg>(twistTopicName, UpdateVelocity);
    }
    
    void FixedUpdate()
    {
        // wheelController.SetRobotSpeedStep(targetLinearSpeed, targetAngularSpeed);

        if(pause)
        {
            return;
        }

        baseController.SetVelocity(targetLinearSpeed, targetAngularSpeed);
    }

    private void UpdateVelocity(TwistMsg twist)
    {
        targetLinearSpeed = twist.linear.From<FLU>();
        targetAngularSpeed = twist.angular.From<FLU>();
    }

    public void Pause(bool stop = true)
    {
        pause = stop;
        if(pause)
        {
            baseController.SetVelocity(Vector3.zero, Vector3.zero);
        }
    }

    // void FixedUpdate()
    // {
    //     wheelController.SetRobotSpeedStep(targetLinearSpeed, targetAngularSpeed);
    // }

    // private void UpdateVelocity(TwistMsg twist)
    // {
    //     targetLinearSpeed = twist.linear.From<FLU>().z;
    //     targetAngularSpeed = twist.angular.From<FLU>().y;
    // }
}
