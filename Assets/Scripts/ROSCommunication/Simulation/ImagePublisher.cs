using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

/// <summary>
///     This script publishes camera view to ROS
///     using Unity Async GPU Readback
/// </summary>
public class ImagePublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string cameraTopicName = "camera/color/image_raw";
    [SerializeField] private string cameraInfoTopicName = "camera/color/camera_info";
    [SerializeField] private string frameId = "camera_link";

    // Sensor
    [SerializeField] private Camera cam;
    [SerializeField] private int width = 1280;
    [SerializeField] private int height = 720;

    public enum ImageFormat { RGB, Depth }
    [SerializeField] private ImageFormat format;

    // Message
    private ImageMsg image;
    private CameraInfoMsg cameraInfo;
    private byte[] imageBytes;
    private float[] depthFloats;
    // request image with GPU readback
    private CommandBuffer cmd;
    private RenderTexture renderTexture;
    private RenderTexture rgbTexture;
    private RenderTexture depthTexture;
    private AsyncGPUReadbackRequest request;
    // rate
    [SerializeField] private int publishRate = 10;
    private float publishPeriod;
    private float elapsedTime = 0.0f;
    private bool shouldPublish = false;

    // Unity running environment
    private bool isOpenGL;

    void Start()
    {
        /*
        // Check to make sure the camera type matches the image format
        Debug.Assert((
            (format == ImageFormat.RGB && cam.tag != "DepthCamera") 
            || (format == ImageFormat.Depth && cam.tag == "DepthCamera") 
        ), "Camera type does not match the image format");
        */

        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(cameraTopicName);
        ros.RegisterPublisher<CameraInfoMsg>(cameraInfoTopicName);

        // Initialize container
        cmd = new CommandBuffer();
        rgbTexture = new RenderTexture(
            width, height, 24, RenderTextureFormat.ARGB32
        );
        depthTexture = new RenderTexture(
            width, height, 24, RenderTextureFormat.RFloat
        );
        renderTexture = (format == ImageFormat.RGB)? rgbTexture : depthTexture;

        // Initialize image message
        image = new ImageMsg();
        image.header = new HeaderMsg(
            Clock.GetCount(), new TimeStamp(Clock.time), frameId
        );
        image.height = (uint) height;
        image.width = (uint) width;
        image.encoding = (format == ImageFormat.RGB)? "rgba8" : "32FC1";
        image.step = (uint) width * 4;
        imageBytes = new byte[width * height * 4];
        depthFloats = new float[width * height];

        // Initialize image info message
        cameraInfo = new CameraInfoMsg();
        cameraInfo.header = image.header;
        cameraInfo.height = (uint) height;
        cameraInfo.width = (uint) width;
        cameraInfo.distortion_model = "plumb_bob";
        // camera parameters
        double cx = cameraInfo.width / 2.0;
        double cy = cameraInfo.height / 2.0;
        double fx = cameraInfo.width / (
            2.0 * Math.Tan(cam.fieldOfView * Math.PI / 360.0)
        );
        double fy = fx;
        cameraInfo.K = new double[9] {
            fx, 0, cx,
            0, fy, cy,
            0, 0, 1
        };
        cameraInfo.D = new double[5] { 0, 0, 0, 0, 0 };
        cameraInfo.R = new double[9] { 
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        };
        cameraInfo.P = new double[12] { 
            fx, 0, cx,
            0,  0, fy,
            cy, 0, 0,
            0,  1, 0
        };
        
        // Rate
        publishPeriod = 1.0f / publishRate;

        // Check Unity running environment
        // If the project is run under OpenGL, the image needs to be flipped
        // https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
        isOpenGL = SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL");
    }

    void OnDestroy()
    {
        cmd.Release();
    }

    void FixedUpdate()
    {
        // Check publish time
        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime >= publishPeriod)
        {
            shouldPublish = true;
            elapsedTime -= publishPeriod;
        }
    }

    void Update()
    {
        if (cam.targetTexture == null)
        {
            return;
        }

        // If publish time is reached => new request
        if (shouldPublish && request.done)
        {
            shouldPublish = false;

            // Request to read camera target texture
            cmd.Clear();

            // Flip the image or not
            if (!isOpenGL)
            {
                cmd.Blit(cam.targetTexture, renderTexture);
            }
            else
            {
                // Blit the target texture with a vertical flip 
                // (scale by -1 in Y direction, offset 1 in Y direction)
                cmd.Blit(
                    cam.targetTexture, renderTexture, 
                    new Vector2(1, -1), new Vector2(0, 1)
                );
            }

            Graphics.ExecuteCommandBuffer(cmd);
            request = AsyncGPUReadback.Request(renderTexture);
        }

        // When the requested frame is acquired => process and publish
        // (The result is accessible only for a single frame 
        //  is then disposed of in the following frame.)
        if (request.done && !request.hasError)
        {
            // Update image messages
            image.header = new HeaderMsg(
                Clock.GetCount(), new TimeStamp(Clock.time), frameId
            );
            cameraInfo.header = image.header;

            // Convert request data to byte array
            // If RGB, copy directly
            if (format == ImageFormat.RGB)
            {
                request.GetData<byte>().CopyTo(imageBytes);
            }
            // If depth, scale back to meter unit
            // and convert to byte array
            if (format == ImageFormat.Depth)
            {
                request.GetData<float>().CopyTo(depthFloats);
                for (int i = 0; i < depthFloats.Length; i++)
                {
                    // If the depth is 0, out of far range
                    // Set it to infinity (https://ros.org/reps/rep-0117.html)
                    if (depthFloats[i] == 0)
                    {
                        depthFloats[i] = float.PositiveInfinity;
                    }
                    // Scale by the camera far plane for meter values
                    else
                    {
                        depthFloats[i] *= cam.farClipPlane;
                    }
                }
                Buffer.BlockCopy(
                    depthFloats, 0, imageBytes, 0, imageBytes.Length
                );
            }

            // Set the data of the image message
            image.data = imageBytes;

            ros.Publish(cameraTopicName, image);
            ros.Publish(cameraInfoTopicName, cameraInfo);
        }
    }
}
