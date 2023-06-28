using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using RosMessageTypes.Tf2;

/// <summary>
///     This script publishes ROS tf trees
///
///     Note: this script is not necessary since a more common way to
///     publish tree is by using robot_state_publisher on the ROS side
/// </summary>
public class ROSTransformTreePublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private List<string> globalFrameIds = 
        new List<string> { "odom" };
    private const string tfTopic = "/tf";

    // Robots
    public GameObject robot;
    private TransformTreeNode transformRoot;

    // Message 
    private TFMessageMsg tfMessage = new TFMessageMsg();
    // rate
    private int publishRate = 30;
    private Timer timer;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TFMessageMsg>(tfTopic);

        // Get robot transform tree
        transformRoot = new TransformTreeNode(robot);
        var tfMessageList = new List<TransformStampedMsg>();
        tfMessage = new TFMessageMsg(tfMessageList.ToArray());

        // Rate
        timer = new Timer(publishRate);
    }

    void FixedUpdate()
    {
        timer.UpdateTimer(Time.fixedDeltaTime);
        if (timer.ShouldProcess)
        {
            PublishTF();
            timer.ShouldProcess = false;
        }
    }

    private void PublishTF()
    {
        var tfMessageList = new List<TransformStampedMsg>();

        if (globalFrameIds.Count > 0)
        {
            var tfRootToGlobal = new TransformStampedMsg(
                new HeaderMsg(
                    0, 
                    new TimeStamp(Clock.time), 
                    globalFrameIds.Last()
                ),
                transformRoot.Name,
                transformRoot.Transform.To<FLU>()
            );
            tfMessageList.Add(tfRootToGlobal);
        }
        else
        {
            Debug.LogWarning(
                $"No {globalFrameIds} specified," + 
                "transform tree will be entirely local coordinates."
            );
        }

        // In case there are multiple "global" transforms 
        // that are effectively the same coordinate frame, 
        // treat this as an ordered list, first entry is the "true" global
        for (var i = 1; i < globalFrameIds.Count; ++i)
        {
            var tfGlobalToGlobal = new TransformStampedMsg(
                new HeaderMsg(0, new TimeStamp(Clock.time), globalFrameIds[i - 1]),
                globalFrameIds[i],
                // Initializes to identity transform
                new TransformMsg()
            );
            tfMessageList.Add(tfGlobalToGlobal);
        }

        PopulateTFList(tfMessageList, transformRoot);

        var tfMessage = new TFMessageMsg(tfMessageList.ToArray());
        ros.Publish(tfTopic, tfMessage);
    }

    private static void PopulateTFList(
        List<TransformStampedMsg> tfList, TransformTreeNode tfNode
    )
    {
        // TODO: Some of this could be done once and cached 
        // rather than doing from scratch every time
        // Only generate transform messages from the children, 
        // because This node will be parented to the global frame
        foreach (var childTf in tfNode.Children)
        {
            tfList.Add(ToTransformStamped(childTf));

            if (!childTf.IsALeafNode)
            {
                PopulateTFList(tfList, childTf);
            }
        }
    }

    private static TransformStampedMsg ToTransformStamped(TransformTreeNode node)
    {
        return new TransformStampedMsg(
            new HeaderMsg(
                0, 
                new TimeStamp(Clock.time), 
                node.Transform.parent.gameObject.name
            ),
            node.Transform.gameObject.name,
            new TransformMsg(
                // Using vector/quaternion To<>() because 
                // Transform.To<>() doesn't use localPosition/localRotation
                node.Transform.localPosition.To<FLU>(),
                node.Transform.localRotation.To<FLU>()
            )
        );
    }
}
