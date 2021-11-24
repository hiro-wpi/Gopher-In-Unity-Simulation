using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GopherManager : MonoBehaviour
{
    // Robot
    public GameObject robotPrefab;
    public GameObject robotWithObjectPrefab;
    public GameObject robot;
    // Wheel
    private KeyboardWheelControl wheelController;
    // Camera
    public int cameraIndex;
    private Camera[] cameras;
    // mobility
    public bool cameraMobility;
    private MouseCameraControl[] cameraControllers;
    // field of view
    public int cameraFOVIndex;
    private float[] cameraFOV;
    private RenderTexture regularCameraRendertexture;
    private RenderTexture wideCameraRendertexture;
    public RenderTexture[] cameraRenderTextures;
    // framerate
    public int cameraDesiredFrameRate = 30; //TODO

    // Robot state
    public GopherDataRecorder dataRecorder;
    public bool isRecording;


    void Start()
    {
        // Camera
        cameraIndex = 0;
        cameraMobility = false;
        cameraFOVIndex = 0;
        cameraFOV = new float[] {69.4f, 91.1f};
        regularCameraRendertexture = new RenderTexture(1920, 1080, 24);
        wideCameraRendertexture = new RenderTexture (2560, 1080, 24);
        cameraRenderTextures = new RenderTexture[] {regularCameraRendertexture, 
                                                    wideCameraRendertexture};
        // If a robot is given
        if (robot != null)
            InitializeRobot();                                     
    }

    void Update()
    {}

    public void SpawnRobot(int prefabIndex = 0,
                           Vector3 position = new Vector3(),
                           Vector3 rotation = new Vector3())
    {
        if (robot != null)
            Destroy(robot);
            
        // Spawn robot
        if (prefabIndex == 0)
            robot = Instantiate(robotPrefab, position, Quaternion.Euler(rotation));
        else
            robot = Instantiate(robotWithObjectPrefab, position, Quaternion.Euler(rotation));

        // Initialization
        InitializeRobot();
    }

    private void InitializeRobot()
    {
        // Get components
        // wheel
        wheelController = robot.GetComponentInChildren<KeyboardWheelControl>();
        // camera !! make sure the cameras and controllers are acquired in the same order
        cameras = robot.GetComponentsInChildren<Camera>();
        cameraControllers = robot.GetComponentsInChildren<MouseCameraControl>();
        InitializeCameras();
        
        // Set up data recorder
        dataRecorder.setRobot(robot);
    }

    private void InitializeCameras()
    {
        // Viewpoint + FOV
        foreach (Camera camera in cameras)
        {
            camera.enabled = false;
            camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
            camera.fieldOfView = cameraFOV[cameraFOVIndex];
            camera.targetTexture = cameraRenderTextures[cameraFOVIndex];     
        }

        // Turn on camera
        cameras[cameraIndex].enabled = true;
        // InvokeRepeating("CameraRender", 0f, 1f/cameraDesiredFrameRate); //TODO

        // Camera control
        foreach (MouseCameraControl controller in cameraControllers)
            controller.enabled = false;

        cameraControllers[cameraIndex].enabled = cameraMobility;
        Cursor.lockState = (cameraMobility)? CursorLockMode.Locked:
                                             CursorLockMode.Confined;
    }

    public void ChangeCameraView()
    {
        if (robot == null) return;

        // Disable current camera
        cameras[cameraIndex].enabled = false;
        // cameraControllers[cameraIndex].HomeCameraJoints(); //TOFI
        bool mobility = cameraControllers[cameraIndex].enabled;
        cameraControllers[cameraIndex].enabled = false;

        // Enable next camera
        cameraIndex = (cameraIndex+1) % cameras.Length;
        cameras[cameraIndex].enabled = true;
        cameraControllers[cameraIndex].enabled = mobility;
    }

    public void ChangeCameraFOV()
    {
        if (robot == null) return;

        // Change FOV for all cameras
        cameraFOVIndex = (cameraFOVIndex+1) % cameraFOV.Length;
        foreach (Camera camera in cameras)
        {
            camera.enabled = false;
            camera.fieldOfView = cameraFOV[cameraFOVIndex];
            camera.targetTexture = cameraRenderTextures[cameraFOVIndex];
        }

        // Renable current camera
        cameras[cameraIndex].enabled = true;
    }

    public void ChangeCameraMobility()
    {
        if (robot == null) return;

        // Disable mobility of other cameras
        foreach (MouseCameraControl controller in cameraControllers)
            controller.enabled = false;
        
        // Change mobility of current camera
        cameraMobility = !cameraMobility;
        cameraControllers[cameraIndex].enabled = cameraMobility;
        Cursor.lockState = (cameraMobility)? CursorLockMode.Locked:
                                             CursorLockMode.Confined;
    }

    public void ChangeRobotSpeed()
    {
        if (robot == null) return;
        // Change wheel controller maximum speed
        if (wheelController.speed == 1.5f)
        {
            wheelController.speed = 1.0f;
            wheelController.angularSpeed = 1.0f;
        }
        else if (wheelController.speed == 1.0f)
        {
            wheelController.speed = 1.5f;
            wheelController.angularSpeed = 1.2f;
        }
    }
    public float GetRobotSpeed()
    {
        // Return robot maximum speed
        return wheelController.speed;
    }

    public void Record(string fileName = "Gopher")
    {
        // Start or stop record data
        if (!isRecording)
            dataRecorder.StartRecording(fileName);
        else
            dataRecorder.StopRecording();

        isRecording = !isRecording;
    }

    // TODO
    private void CameraRender()
    {
        if (cameras[cameraIndex] != null)
            cameras[cameraIndex].Render();
    }
}
