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

    // Sensor
    [SerializeField] private Camera cam;
    [SerializeField] private int width = 1280;
    [SerializeField] private int height = 720;

    // Message
    private ImageMsg image;
    private CameraInfoMsg cameraInfo;
    // request image with GPU readback
    private RenderTexture renderTexture;
    private AsyncGPUReadbackRequest request;
    private byte[] imageBytes;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(cameraTopicName);
        ros.RegisterPublisher<CameraInfoMsg>(cameraInfoTopicName);

        renderTexture = new RenderTexture(
            width, height, 24, RenderTextureFormat.ARGB32
        );
        cam.targetTexture = renderTexture;

        // Initialize image message
        image = new ImageMsg();
        image.header = new HeaderMsg(
            Clock.GetCount(), new TimeStamp(Clock.time), frameId
        );
        image.height = (uint) height;
        image.width = (uint) width;
        image.encoding = "rgba8";
        image.step = (uint) width * 4;
        imageBytes = new byte[width * height * 4];

        // Initialize image info message
        cameraInfo = new CameraInfoMsg();
        cameraInfo.header = image.header;
        cameraInfo.height = (uint) height;
        cameraInfo.width = (uint) width;
        cameraInfo.distortion_model = "plumb_bob";

        double cx = cameraInfo.width / 2.0;
        double cy = cameraInfo.height / 2.0;
        double fx = cameraInfo.width / System.Math.Tan(69.4 / 2.0);
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

    void Update()
    {
        if (request.done)
        {
            if (!request.hasError)
            {
                // Convert request data to byte array
                request.GetData<byte>().CopyTo(imageBytes);

                // Update image messages
                image.header = new HeaderMsg(
                    Clock.GetCount(), new TimeStamp(Clock.time), frameId
                );
                image.data = imageBytes;
                cameraInfo.header = image.header;

                ros.Publish(cameraTopicName, image);
                ros.Publish(cameraInfoTopicName, cameraInfo);
            }
            
            // If the project is run under OpenGL, the image needs to be flipped
            // https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
            if (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL"))
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
    private static void VerticallyFlipRenderTexture(RenderTexture target)
    {
        RenderTexture temp = RenderTexture.GetTemporary(target.descriptor);
        // Blit the target texture to a temporary render texture with a 
        // vertical flip (scale by -1 in Y direction, offset 1 in Y direction)
        Graphics.Blit(target, temp, new Vector2(1, -1), new Vector2(0, 1));
        Graphics.Blit(temp, target);
        RenderTexture.ReleaseTemporary(temp);
    }
}
