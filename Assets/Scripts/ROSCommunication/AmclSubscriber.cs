using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Unity.Robotics.Core;
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
    public string AmclBotTopicName = "/pose_unity_bot";

    private TwistStampedPublisher twistStampedPublisher;
    private TwistMsg twistMsg;

    private Vector3 unity_coords;

    private double init_x = 0.0;
    private double init_y = 0.0;

    private double delta_x;
    private double delta_y;

    private double v_x;
    private double v_y;
    private double twist_x;
    private double twist_y;

    private double init_time;
    private double curr_time; 
    private double delta_time;

    //making sphere gameobject for testing
    private GameObject test_sphere;

    public ArticulationWheelController wheelController;

    struct EulerAngles{
        public double roll;
        public double pitch; 
        public double yaw;
    }

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        //ros.RegisterPublisher<Pose>(TwistTopicName);
        
        // Initialize message
        twistMsg = new TwistMsg
        {
            // linear
        };
        
        //initializing sphere gameobject
        // test_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // test_sphere.transform.localScale = new Vector3(1.0f ,1.0f ,1.0f);


        unity_coords = new Vector3(0.0f, 0.0f, 0.0f);

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

        EulerAngles angles = quaternion_to_euler(pose.pose.pose.orientation.w, pose.pose.pose.orientation.x, pose.pose.pose.orientation.y, pose.pose.pose.orientation.z);

        delta_x = x - init_x;
        delta_y = y - init_y;

        curr_time = Time.time;
        delta_time = init_time-curr_time; 
        init_time = curr_time;

        v_x = delta_x/delta_time;
        v_y = delta_y/delta_time;

        twist_x = v_x;
        twist_y = v_y;

        unity_coords.x = (float)(-1.0*y);
        unity_coords.z = (float)(-1.0*x);

        // Debug.Log(unity_coords);

        // gameObject.transform.parent.transform.parent.position = unity_coords;
        // test_sphere.transform.position = unity_coords;
        // Debug.Log(test_sphere.transform.position);

    }

    private EulerAngles quaternion_to_euler(double w, double x, double y, double z){
        EulerAngles angles;

        double sinr_cosp = 2*(w*x + y*z);
        double cosr_cosp = 1 - (2*(x*x + y*y));
        angles.roll = Math.Atan2(sinr_cosp, cosr_cosp);

        double sinp = 2*(w*y - z*x);
        if(sinp > 1){
            sinp = 1;
        }
        if(sinp < -1){
            sinp = -1;
        }
        angles.pitch = Math.Asin(sinp);

        double siny_cosp = 2*(w*z + x*y);
        double cosy_cosp = 1 - 2*(y*y + z*z);
        angles.yaw = Math.Atan2(siny_cosp, cosy_cosp);

        return angles;

    }

}
