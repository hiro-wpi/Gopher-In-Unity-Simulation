using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using RosMessageTypes.GopherNavigation;

/// <summary>
///     This scripts focuses on interfacing with the tf listener object 
///     in gopher_navigation. This script sends in the child and parent frames
///     of the needed transformation to initciate the node. This script will
///     act as a subscriber and listen in for the transform.
/// </summary>
public class TFListenerSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;

    // Variables required for ROS communication
    [SerializeField] private string tfListenerServiceName = 
        "tfListener/SetChildAndParentFrames";
    [SerializeField] private string tfListenerTopicName = 
        "tfListener/...";

    // Varibles required for the transform
    [SerializeField] private string parentFrame = 
        "/map";
    [SerializeField] private string childFrame = 
        "/gopher/base_link";

    public Vector3 position { get; private set; }
    public Vector3 rotationEuler { get; private set; }
    public Quaternion rotationQuaternion { get; private set; }

    // Message
    private TFListenerServiceRequest tfListenerServiceRequest;
    // private TFListenerServiceResponse tfListenerServicerResponse;

    // Transform 
    private TransformStampedMsg transformMsg;
    private bool isValidTransform;


    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<TFListenerServiceRequest, TFListenerServiceResponse>(
            tfListenerServiceName
        );

        ros.Subscribe<TransformStampedMsg>(tfListenerTopicName, tfListenerCallback);

        // Initialize service request
        tfListenerServiceRequest = new TFListenerServiceRequest();

        SetFramesCommand();
    }

    void Update() {}

    // Handles the published transformation msgs from the tfListener Node
    public void tfListenerCallback(TransformStampedMsg msg)
    {
        if(isValidTransform)
        {
            // Check if the parent and child are the same (should be)
            if(parentFrame == msg.header.frame_id & childFrame == msg.child_frame_id)
            {
                position = msg.transform.translation.From<FLU>();
                rotationQuaternion = msg.transform.rotation.From<FLU>();
                rotationEuler = msg.transform.rotation.From<FLU>().eulerAngles;
            }
            else
            {
                Debug.LogError("Requested Transformation Frames Does Not Match Recieved Frames. Ignoring Transformation");
            }
        }
        else
        {
            Debug.Log("Not a valid transform");
        }
            
    }

    // Request Service to Start Sending Transform
    public void SetFramesCommand()
    {
        // Init Msg
        tfListenerServiceRequest = new TFListenerServiceRequest(
            new string(parentFrame),
            new string(childFrame)
        );

        // Request service
        ros.SendServiceMessage<TFListenerServiceResponse>(
            tfListenerServiceName, tfListenerServiceRequest, SetFramesCallback
        );
    }

    private void SetFramesCallback(TFListenerServiceResponse response) 
    {
        isValidTransform = response.isValidTransform;        
    }
}
