using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.InputSystem;
using System;

using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;

public class AmclPublisher : MonoBehaviour
{
    private Transform publishedTransform;
    private ROSConnection ros;
    public String goal_topic_name = "/move_base"; 
    private Vector2 orientation;
    private PoseStampedMsg poseStamped;
    private string frameID = "model_pose";

    private float x;
    private float z;
    private float y;

    private Camera cam;

    void Start()
    {
        Cursor.visible = false;
        cam =  GameObject.Find("nav_cam").GetComponent<Camera>();

         // Get ROS connection static instance
        publishedTransform = this.transform;
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PoseStampedMsg>(goal_topic_name, ReceiveGoal);
        ros.RegisterPublisher<PoseStampedMsg>(goal_topic_name);
        
        orientation = new Vector3(0.0f, 0.0f, 0.0f);
        // Initialize message
        poseStamped = new PoseStampedMsg
        {
            header = new HeaderMsg(Clock.GetCount(),
                                   new TimeStamp(Clock.time), frameID)
        };
    }

    void Update()
    {
        this.transform.position = Input.mousePosition;
        Cursor.visible = false;
        if (Input.GetMouseButtonDown(0)){
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                x = hit.point.x;
                x = 0.9346f*x - 13.26f;

                z = hit.point.z;
                z = (float)(-0.0028f*(Math.Pow(z, 3)) - 0.0042f*(Math.Pow(z,2)) + 0.8118f*z - 8.141f);
                publishedTransform.position = new Vector3(x, 0.0f, z);
                PublishPoseStamped();
            }
            
        }
    }

    private void PublishPoseStamped()
    {
        poseStamped.header = new HeaderMsg(Clock.GetCount(), 
                                           new TimeStamp(Clock.time), frameID);

        poseStamped.pose.position = publishedTransform.position.To<FLU>();
        poseStamped.pose.orientation = publishedTransform.rotation.To<FLU>();
        Debug.Log(poseStamped.pose.position);
        ros.Publish(goal_topic_name, poseStamped);
    }

    private void ReceiveGoal(PoseStampedMsg pose)
    {
        publishedTransform = this.transform;
    }
}
