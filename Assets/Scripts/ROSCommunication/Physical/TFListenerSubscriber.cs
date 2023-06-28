using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using RosMessageTypes.GopherNavigation;

/// <summary>
///     This scripts focuses on interfacing with the tf listener in ROS.
///     It acts as a subscriber and listen in for the transform.
///
///     It is required to send in the child and parent frames
///     of the needed transformation to initiate the node.
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
        "map";
    [SerializeField] private string childFrame = 
        "gopher/chassis_link";

    [SerializeField, ReadOnly] private Vector3 position;
    [SerializeField, ReadOnly] private Vector3 rotationEuler;
    private Quaternion rotationQuaternion;

    // Message
    private TFListenerServiceRequest tfListenerServiceRequest;
    private TransformStampedMsg transformMsg;
    [field: SerializeField]
    public bool IsValidTransform { get; private set; } = false;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();

        // Connect to the ROS Service
        ros.RegisterRosService<TFListenerServiceRequest, TFListenerServiceResponse>(
            tfListenerServiceName
        );
        // Connect to the ROS Topic
        ros.Subscribe<TransformStampedMsg>(tfListenerTopicName, TFListenerCallback);

        // Initialize service request
        SetFramesCommand();
    }

    void Update() {}

    // Handles the published transformation msgs from the tfListener Node
    public void TFListenerCallback(TransformStampedMsg msg)
    {
        position = msg.transform.translation.From<FLU>();
        rotationQuaternion = msg.transform.rotation.From<FLU>();
        rotationEuler = rotationQuaternion.eulerAngles;
    }

    // Request Service to Start Sending Transform with given frames
    public void SetFramesCommand(string parentTF = "", string childTF = "")
    {
        if (parentTF != "")
        {
            parentFrame = parentTF;
        }
        if (childTF != "")
        {
            childFrame = childTF;
        }

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
        IsValidTransform = response.isValidTransform;        
    }

    public (Vector3, Vector3) GetPose()
    {
        return (position, rotationEuler);
    }
}
