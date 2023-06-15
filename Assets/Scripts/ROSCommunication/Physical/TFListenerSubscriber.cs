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
        "tf_listener/set_parent_and_child";
    [SerializeField] private string tfListenerTopicName = 
        "tf_listener/transform";

    // Varibles required for the transform
    [SerializeField] private string parentFrame = 
        "/map";
    [SerializeField] private string childFrame = 
        "/gopher/chassis_link";

    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 rotationEuler;
    private Quaternion rotationQuaternion;

    // Message
    private TFListenerServiceRequest tfListenerServiceRequest;
    // private TFListenerServiceResponse tfListenerServicerResponse;

    // Transform 
    private TransformStampedMsg transformMsg;
    [SerializeField] private bool isValidTransform;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        // Connect to the ROS Service
        ros.RegisterRosService<TFListenerServiceRequest, TFListenerServiceResponse>(
            tfListenerServiceName
        );

        // Connect to the ROS Topic
        ros.Subscribe<TransformStampedMsg>(tfListenerTopicName, tfListenerCallback);

        // Initialize service request
        tfListenerServiceRequest = new TFListenerServiceRequest();

        // position = Vector3.zero;
        // rotationEuler = Vector3.zero;
        // rotationQuaternion = new Quaternion();
    }

    void Update() {}

    // Handles the published transformation msgs from the tfListener Node
    public void tfListenerCallback(TransformStampedMsg msg)
    {
        // Debug.Log("Here");
        // Check if the parent and child are the same (should be)
        position = msg.transform.translation.From<FLU>();
        rotationQuaternion = msg.transform.rotation.From<FLU>();
        rotationEuler = msg.transform.rotation.From<FLU>().eulerAngles;
        // Debug.Log(rotationQuaternion);
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

    public (Vector3, Vector3) GetPose()
    {
        return (position, rotationEuler);
    }

}
