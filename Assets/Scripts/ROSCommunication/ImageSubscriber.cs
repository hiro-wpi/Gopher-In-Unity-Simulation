using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class ImageSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    public string cameraTopicName = "camera/color/image_raw/compressed";

    // Display
    public MeshRenderer meshRenderer;
    
    // Message info
    private Texture2D texture2D;
    private byte[] imageData;
    private bool isMessageReceived;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.instance;

        // Display
        texture2D = new Texture2D(1, 1);
        meshRenderer.material = new Material(Shader.Find("Standard"));

        // Subscriber
        ros.Subscribe<CompressedImageMsg>(cameraTopicName, ReceiveImage);
        isMessageReceived = false;
    }

    void Update()
    {
        // Display image if received
        if (isMessageReceived)
        {
            texture2D.LoadImage(imageData);
            texture2D.Apply();
            meshRenderer.material.SetTexture("_MainTex", texture2D);
            isMessageReceived = false;
        }
    }

    private void ReceiveImage(CompressedImageMsg compressedImage)
    {
        imageData = compressedImage.data;
        isMessageReceived = true; 
    }
}
