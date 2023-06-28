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
///     This script publishes camera view (RGB/Depth) to ROS
///     using Unity Async GPU Readback
///
///     As Depth values from Render texture are within [0, 1],
///     another shader is used to scaled it to original values.
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
    // rate
    [SerializeField] private int publishRate = 10;
    private Timer timer;

    // Request image with GPU readback
    private CommandBuffer cmd;
    private AsyncGPUReadbackRequest request;
    private RenderTexture renderTexture;
    private RenderTexture tempTexture;
    private Material depthScaler;

    // Unity running environment
    private bool isOpenGL;

    void Start()
    {
        // Check to make sure the camera type matches the image format
        Debug.Assert((
            (format == ImageFormat.RGB && cam.tag != "DepthCamera") 
            || (format == ImageFormat.Depth && cam.tag == "DepthCamera") 
        ), "Camera type does not match the image format");

        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(cameraTopicName);
        ros.RegisterPublisher<CameraInfoMsg>(cameraInfoTopicName);

        // Initialize image message
        image = new ImageMsg();
        image.header = new HeaderMsg(
            0, new TimeStamp(Clock.time), frameId
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
        timer = new Timer(publishRate);

        // Initialize sampler and container for acquiring image messages
        cmd = new CommandBuffer();
        if (format == ImageFormat.RGB)
        {
            // render texture
            renderTexture = new RenderTexture(
                width, height, 0, RenderTextureFormat.ARGB32
            );
        }
        else
        {
            // render texture
            renderTexture = new RenderTexture(
                width, height, 0, RenderTextureFormat.RFloat
            );
            // depth scaler
            depthScaler = new Material(Shader.Find("Custom/DepthScaler"));
            depthScaler.SetFloat("_FarClipPlane", cam.farClipPlane);
        }

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
        timer.UpdateTimer(Time.fixedDeltaTime);
    }

    void Update()
    {
        if (cam.targetTexture == null)
        {
            return;
        }

        // If publish time is reached => new request
        if (timer.ShouldProcess && request.done)
        {
            timer.ShouldProcess = false;

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
                    cam.targetTexture, 
                    renderTexture, 
                    new Vector2(1, -1), 
                    new Vector2(0, 1)
                );
            }

            // If depth, scale back to meter unit
            if (format == ImageFormat.Depth)
            {
                // create a temporary depth render texture
                tempTexture = RenderTexture.GetTemporary(
                    renderTexture.descriptor
                );
                // scale all values by camera far plane
                cmd.Blit(
                    renderTexture,
                    tempTexture,
                    depthScaler
                );
                cmd.Blit(tempTexture, renderTexture);
                // Release the temporary render texture
                RenderTexture.ReleaseTemporary(tempTexture);
            }

            Graphics.ExecuteCommandBuffer(cmd);
            request = AsyncGPUReadback.Request(renderTexture);
        }

        // When the requested frame is acquired => process and publish
        // (The result is accessible only for a single frame 
        //  is then disposed of in the following frame.)
        if (request.done && !request.hasError)
        {
            // Update image messages header
            image.header.Update();
            cameraInfo.header.Update();

            // Convert request data to byte array
            request.GetData<byte>().CopyTo(imageBytes);
            // Set the data of the image message
            image.data = imageBytes;

            ros.Publish(cameraTopicName, image);
            ros.Publish(cameraInfoTopicName, cameraInfo);
        }
    }
}
