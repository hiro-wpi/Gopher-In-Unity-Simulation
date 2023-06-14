using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using RosMessageTypes.GopherNavigation

/// <summary>
///     This scripts focuses on interfacing with the tf listener object 
///     in gopher_navigation. This script sends in the child and parent frames
///     of the needed transformation to initciate the node. This script will
///     act as a subscriber and listen in for the transform.
/// </summary>
public class MoveBaseCancelGoalService : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communicationS
    [SerializeField] private string tfListenerServiceName = 
        "tfListener/SetChildAndParentFrames";
    [SerializeField] private string tfListenerTopicName = 
        "tfListener/...";

    // Message
    private TFListenerServiceRequest request;
    private TFListenerServiceResponse response;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<TFListenerServiceRequest, TFListenerServiceResponse>(
            tfListenerServiceName
        );
        ros.Subscriber(tfListenerTopicName, topicCallback);

        // Initialize service request
        emptyRequest = new EmptyRequest();
    }

    void Update() {}

    public void topicCallback(TransformStampedMsg)

    // Request service to cancel all goal(s) to move base
    public void CancelGoalCommand()
    {
        // Request service
        ros.SendServiceMessage<EmptyResponse>(
            moveBaseCancelGoalServiceName, emptyRequest, CancelGoalCallback
        );
    }

    private void CancelGoalCallback(EmptyResponse response) {}
}
