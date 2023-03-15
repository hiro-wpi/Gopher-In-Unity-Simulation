using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;


/// <summary>
///     This script subscribes and display an image
///     sent from ROS.
/// </summary>
public class CompressedImageSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string compressedImageTopicName = 
        "camera/color/image_raw/compressed";
    
    // Message
    private Texture2D texture2D;
    private bool isMessageReceived;

    // Display
    [field:SerializeField] public RenderTexture TargetTexture { get; set; }

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();

        // Initialize message
        texture2D = new Texture2D(1, 1);
        isMessageReceived = false;

        // Subscriber
        ros.Subscribe<CompressedImageMsg>(compressedImageTopicName, ReceiveImage);
    }

    void Update()
    {
        // Copy to a target texture for display if received
        if (isMessageReceived && TargetTexture != null)
        {
            Graphics.Blit(texture2D, TargetTexture);
            isMessageReceived = false;
        }
    }

    private void ReceiveImage(CompressedImageMsg compressedImage)
    {
        // this leads to memory leak
        // texture2D = MessageExtensions.ToTexture2D(compressedImage);
        texture2D.LoadImage(compressedImage.data);
        isMessageReceived = true;
    }
}
