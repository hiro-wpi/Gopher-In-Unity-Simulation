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
    [SerializeField] private string frameId = "camera";

    // Unity running environment
    private bool isOpenGL;

    // Sensor
    [SerializeField] private Camera cam;
    [SerializeField] private int width = 1280;
    [SerializeField] private int height = 720;

    public enum ImageFormat { ARGB32, Depth }
    [SerializeField] private ImageFormat format;

    // Message
    private ImageMsg image;
    private CameraInfoMsg cameraInfo;
    private byte[] imageBytes;
    private byte[] depthBytes;
    // request image with GPU readback
    private CommandBuffer cmd;
    private RenderTexture renderTexture;
    private RenderTexture tempTexture;
    private AsyncGPUReadbackRequest request;

    private float publishTime = 0.1f;
    private float elapsedTime = 0.0f;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(cameraTopicName);
        ros.RegisterPublisher<CameraInfoMsg>(cameraInfoTopicName);
        
        // Check Unity running environment
        isOpenGL = SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL");

        // Initialize container
        cmd = CommandBufferPool.Get("ImagePublisher");
        renderTexture = new RenderTexture(
            width, height, 24, RenderTextureFormat.ARGB32
        );
        tempTexture = RenderTexture.GetTemporary(renderTexture.descriptor);
        cam.targetTexture = renderTexture;

        // Initialize image message
        image = new ImageMsg();
        image.header = new HeaderMsg(
            Clock.GetCount(), new TimeStamp(Clock.time), frameId
        );
        image.height = (uint) height;
        image.width = (uint) width;
        if (format == ImageFormat.ARGB32)
        {
            image.encoding = "rgba8";
            image.step = (uint) width * 4;
        }
        else
        {
            image.encoding = "32FC1";
            image.step = (uint) width;
        }
        imageBytes = new byte[width * height * 4];
        depthBytes = new byte[width * height * 2];

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
    }

    void OnDestroy()
    {
        cmd.Release();
        cmd.Dispose();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime < publishTime)
        {
            return;
        }
        elapsedTime -= Time.deltaTime;

        if (request.done)
        {
            if (!request.hasError)
            {
                // Update image messages
                image.header = new HeaderMsg(
                    Clock.GetCount(), new TimeStamp(Clock.time), frameId
                );
                cameraInfo.header = image.header;

                // Convert request data to byte array
                request.GetData<byte>().CopyTo(imageBytes);
                if (format == ImageFormat.ARGB32)
                {
                    image.data = imageBytes;
                }
                else
                {
                    var depth32Texture = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
                    // Copy the depth image to the 32-bit texture
                    Graphics.Blit(renderTexture, depth32Texture);
                    
                    if (isOpenGL)
                    {
                        VerticallyFlipRenderTexture(depth32Texture);
                    }

                    // Request a new readback from the 16-bit texture
                    request = AsyncGPUReadback.Request(depth32Texture);

                    // Wait for the request to complete
                    request.WaitForCompletion();

                    // Get the data from the request
                    imageBytes = request.GetData<byte>().ToArray();
                    /*
                    float[] data = request.GetData<float>().ToArray();
                    byte[] depthBytes = new byte[data.Length * sizeof(float)];
                    for (int i = 0; i < depthBytes.Length; i++)
                    {
                        depthBytes[i] *= 10; // Convert from meters to millimeters
                    }
                    Buffer.BlockCopy(data, 0, depthBytes, 0, depthBytes.Length);
                    */

                    // Set the data of the image message
                    image.data = imageBytes;
                }

                ros.Publish(cameraTopicName, image);
                ros.Publish(cameraInfoTopicName, cameraInfo);
            }

            // If the project is run under OpenGL, the image needs to be flipped
            // https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
            if (isOpenGL)
            {
                VerticallyFlipRenderTexture(renderTexture);
            }
            request = AsyncGPUReadback.Request(renderTexture);
        }
    }

    private void UpdateImage(Camera cameraObject)
    {
        if (!request.done)
        {
            request = AsyncGPUReadback.Request(renderTexture);
        }
    }

    // Utility function to flip render texture vertically
    private void VerticallyFlipRenderTexture(RenderTexture target)
    {
        // Perform flipping
        tempTexture = RenderTexture.GetTemporary(target.descriptor);
        // blit the target texture to a temporary render texture with a 
        // vertical flip (scale by -1 in Y direction, offset 1 in Y direction)
        
        // cmd.Clear();
        // cmd.Blit(target, tempTexture, new Vector2(1, -1), new Vector2(0, 1));
        // cmd.Blit(tempTexture, target);
        // Graphics.ExecuteCommandBuffer(cmd);
        Graphics.Blit(target, tempTexture, new Vector2(1, -1), new Vector2(0, 1));
        Graphics.Blit(tempTexture, target);

        // Release
        RenderTexture.ReleaseTemporary(tempTexture);
    }
}
