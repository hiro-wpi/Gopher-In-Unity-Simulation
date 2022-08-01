using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicalInterface : MonoBehaviour
{
    // Active
    private bool active;

    // UI
    public GameObject[] interfaces;
    public GameObject speedometerPointer;
    public GameObject steeringWheel;
    public GameObject battery;
    public GameObject controlDisplay;
    public GameObject locationInfo;
    public GameObject recordIcon;
    public GameObject helpDisplay;
    
    // Robot
    private GameObject robot;
    private Localization localization;
    private StateReader stateReader;
    // main camera
    public GameObject cameraDisplay;
    private Vector2 cameraResolution;
    private RectTransform cameraDisplayRect;
    private RenderTexture cameraRendertexture;
    // secondary camera
    public GameObject secondayCameraDisplay;
    private RectTransform secondayCameraDisplayRect;
    private RenderTexture secondayCameraRendertexture;
    // camera selection
    private CameraSystem cameraSystem;
    private int numCameras;
    private int cameraIndex;
    private int secondaryCameraIndex;

    // Minimap
    public GameObject minimapDisplay;
    private Vector2 minimapResolution;
    private RectTransform minimapDisplayRect;
    private RenderTexture minimapRendertexture;
    private GameObject minimapCameraObject;
    private Camera minimapCamera;
    private float[] minimapSizes;
    private int minimapSizeIndex;

    // Map
    private RenderTexture mapRendertexture;
    private GameObject mapCameraObject;
    private Camera mapCamera;
    
    // battery
    private float robotSpawnedTime;
    private Slider batterySlider;

    // control mode
    public GameObject controlModeLeftDisplay;
    public GameObject controlModeRightDisplay;
    public GameObject controlModeBaseDisplay;
    private GopherControl gopherController;
   
    // localization
    private GameObject map;
    private Vector3 tempMapPosition;
    private Quaternion tempMapRotation;
    private bool displayMapInMain = false;
    private TextMeshProUGUI locationText;

    // Task panel
    private Task currentTask;
    public GameObject taskDescriptionPanel;
    public GameObject taskStatusPanel;
    private TextMeshProUGUI taskDescriptionPanelText;
    private TextMeshProUGUI taskStatusPanelText;
    // Scan
    public GameObject barCodeScanDisplay;
    // Input field
    public GameObject inputField;
    private TMP_InputField input;

    // Pop up message 
    public GameObject messagePanel;
    private TextMeshProUGUI messagePanelText;

    // Help
    private TextMeshProUGUI helpDisplayText;
    
    // FPS
    private int FPS;
    private float FPSSum;
    private int FPSCount;

    void Start() 
    {
        // Activation
        SetUIActive(false);

        // Camera displays
        // main render texture
        cameraDisplayRect = cameraDisplay.GetComponent<RectTransform>();
        cameraResolution = new Vector2((int)cameraDisplayRect.rect.width, 
                                       (int)cameraDisplayRect.rect.height);
        cameraRendertexture = new RenderTexture((int)cameraResolution.x, 
                                                (int)cameraResolution.y, 24);
        cameraDisplay.GetComponent<RawImage>().texture = cameraRendertexture;

        // secondary render texture
        secondayCameraDisplayRect = secondayCameraDisplay.GetComponent<RectTransform>();
        secondayCameraRendertexture = new RenderTexture((int)secondayCameraDisplayRect.rect.width, 
                                                        (int)secondayCameraDisplayRect.rect.height, 24);
        secondayCameraDisplay.GetComponent<RawImage>().texture = secondayCameraRendertexture;
        
        // minimap render texture
        minimapDisplayRect = minimapDisplay.GetComponent<RectTransform>();
        minimapResolution = new Vector2(minimapDisplayRect.rect.width, 
                                        minimapDisplayRect.rect.height);
        minimapRendertexture = new RenderTexture((int)minimapResolution.x, 
                                                 (int)minimapResolution.y, 24);
        minimapDisplay.GetComponent<RawImage>().texture = minimapRendertexture;

        // map
        mapRendertexture = new RenderTexture((int)cameraResolution.x, 
                                             (int)cameraResolution.y, 24);

        // Robot status UI
        // battery
        batterySlider = battery.GetComponentInChildren<Slider>();
        // Location
        locationText = locationInfo.GetComponentInChildren<TextMeshProUGUI>();
        // minimap
        minimapSizes = new float[] {5f, 8f, 12f};
        minimapSizeIndex = 0;

        // task status UI
        taskDescriptionPanelText = taskDescriptionPanel.GetComponentInChildren<TextMeshProUGUI>();
        taskStatusPanelText = taskStatusPanel.GetComponentInChildren<TextMeshProUGUI>();

        // User input
        input = inputField.GetComponentInChildren<TMP_InputField>();

        // Pop up message
        messagePanelText = messagePanel.GetComponentInChildren<TextMeshProUGUI>();

        // Help display
        helpDisplayText = helpDisplay.GetComponentInChildren<TextMeshProUGUI>();

        // FPS
        FPSCount = 0;
        FPSSum = 0;
        InvokeRepeating("UpdateFPS", 1.0f, 0.5f);
    }


    void Update()
    {
        // FPS
        FPSCount += 1;
        FPSSum += 1.0f / Time.deltaTime;

        if (robot == null)
            SetUIActive(false);
        // UI not activated
        if (!active)
            return;
        
        // Hotkeys
        if (Input.GetKeyDown(KeyCode.H)) 
            ChangeHelpDisplay();
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab))
            SwitchCameraView();
        else if (Input.GetKeyDown(KeyCode.Tab))
            ChangeCameraView();
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.M))
            ChangeMinimapView();
        else if (Input.GetKeyDown(KeyCode.M))
            ZoomMap();
        else if (Input.GetKeyDown(KeyCode.B))
            ChangeBarCodeScanDisplay();

        // Update UI
        UpdateRobotStatus();
        UpdateTaskStatus();
    }

    public void SetUIActive(bool active)
    {
        this.active = active;
        foreach (GameObject component in interfaces)
        {
            component.SetActive(active);
        }
    }
    

    // UPDATE INFO
    private void UpdateTaskStatus()
    {
        // Change UI according to task
        if (currentTask == null)
            return;
        taskDescriptionPanelText.text = currentTask.taskDescription;
        taskStatusPanelText.text = "Task Duration: " 
                                 + string.Format("{0:0.00}", currentTask.GetTaskDuration()) + " s."
                                 + "\n\n"
                                 + "Current Task Status: " + "\n"
                                 + currentTask.GetTaskStatus();
    }

    private void UpdateRobotStatus()
    {
        // Change UI according to robot status
        // speed
        UpdateLinearSpeed(stateReader.linearVelocity[2]);
        UpdateAngularSpeed(stateReader.angularVelocity[1]);
        // battery 
        UpdateBattery(stateReader.durationTime);
        // control mode
        UpdateControlMode(gopherController.Mode);
        // location

        UpdateLocalization(localization.position, localization.rotation);
    }

    private void UpdateLinearSpeed(float linearSpeed, float multiplier=1f)
    {
        // linear speed range from 0 -> 1.5 m/s / 0 -> 270°
        linearSpeed = Mathf.Abs(Mathf.Clamp(linearSpeed, -1.5f, 1.5f));
        linearSpeed = - multiplier * linearSpeed * 180f;
        // update
        speedometerPointer.transform.rotation = Quaternion.Euler(0, 0, linearSpeed);
    }
    private void UpdateAngularSpeed(float angularSpeed, float multiplier=1f)
    {
        // angular speed range from -1 -> 1 rad/s / -57° -> 57°
        angularSpeed = Mathf.Clamp(angularSpeed, -2f, 2f);
        angularSpeed = - multiplier * angularSpeed * Mathf.Rad2Deg;
        // update
        steeringWheel.transform.rotation = Quaternion.Euler(0, 0, angularSpeed);
    }

    private void UpdateControlMode(GopherControl.ControlMode controlMode)
    {
        // Control mode display
        controlModeLeftDisplay.SetActive(false);
        controlModeRightDisplay.SetActive(false);
        controlModeBaseDisplay.SetActive(false);
        switch (controlMode)
        {
            case GopherControl.ControlMode.LeftArm:
                controlModeLeftDisplay.SetActive(true);
                break;
            case GopherControl.ControlMode.RightArm:
                controlModeRightDisplay.SetActive(true);
                break;
            case GopherControl.ControlMode.Base:
                controlModeBaseDisplay.SetActive(true);
                break;
            default:
                break;
        }

        // Help display
        UpdateHelpDisplay(controlMode);
    }

    private void UpdateLocalization(Vector3 position, Vector3 rotation)
    {
        // location name
        string location = HospitalMapUtil.GetLocationName(position);
        locationText.text = "Current - " + location;
        // update map in minimap display
        map.transform.position = position - robot.transform.position + new Vector3(0f, -3f, 0f);
        // map.transform.rotation = Quaternion.Euler(rotation - robot.transform.rotation.eulerAngles); 
    }

    private void UpdateBattery(float duration)
    {
        // Convert duration time to battery
        // (0, 100)min -> (1.0, 0.0)
        float value = (100 - duration / 60f) / 100f;
        batterySlider.value = Mathf.Clamp(value, 0f, 1f);
    }

    private void UpdateHelpDisplay(GopherControl.ControlMode controlMode)
    {
        helpDisplayText.text = "Switch Control\n" + "  Arrow ← ↓ →\n"
                             + "Camera Control\n" + "  Arrow ↑\n";
        switch (controlMode)
        {
            case GopherControl.ControlMode.LeftArm:
                helpDisplayText.text += "Joint Position Control\n" + "  WA/SD/QE\n"
                                      + "Joint Rotation Control\n" + "  85/46/79\n"
                                      + "Preset\n" + "  F1 F2 F3 F4 F5\n"
                                      + "Gripper Close/Open\n" + "  Space\n";
                break;
            case GopherControl.ControlMode.RightArm:
                helpDisplayText.text += "Joint Position Control\n" + "  WA/SD/QE\n"
                                      + "Joint Rotation Control\n" + "  85/46/79\n"
                                      + "Preset\n" + "  F1 F2 F3 F4 F5\n"
                                      + "Gripper Close/Open\n" + "  Space\n";
                break;
            case GopherControl.ControlMode.Base:
                helpDisplayText.text += "Base Control\n" + "  WA/SD\n";
                break;
            default:
                break;
        }
    }

    
    // Setup ROBOT AND TASK
    public void SetRobot(GameObject robot)
    {
        // Robot
        this.robot = robot;
        cameraSystem = robot.GetComponentInChildren<CameraSystem>();
        localization = robot.GetComponentInChildren<Localization>();
        stateReader = robot.GetComponentInChildren<StateReader>();
        gopherController = robot.GetComponentInChildren<GopherControl>();
        
        // Cameras
        cameraSystem.enabled = true;
        numCameras = cameraSystem.cameras.Length;
        cameraSystem.DisableAllCameras();
        // turn on main camera and secondary camera
        cameraIndex = cameraSystem.GetIndex("Main");
        secondaryCameraIndex = cameraSystem.GetIndex("Right");
        cameraSystem.SetTargetRenderTexture(cameraIndex, cameraRendertexture);
        cameraSystem.EnableCamera(cameraIndex);
        cameraSystem.SetTargetRenderTexture(secondaryCameraIndex, secondayCameraRendertexture);
        cameraSystem.EnableCamera(secondaryCameraIndex);
        
        // Map
        map = GameObject.FindGameObjectWithTag("Map");

        // Minimap camera game object
        minimapCameraObject = new GameObject("Minimap Camera");
        minimapCameraObject.transform.parent = robot.transform;
        minimapCameraObject.transform.localPosition = new Vector3(0f, 5f, 1f);
        minimapCameraObject.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
        // minimap camera settings
        minimapCamera = minimapCameraObject.AddComponent<Camera>();
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = minimapSizes[minimapSizeIndex];
        minimapCamera.cullingMask = LayerMask.GetMask("Robot", "Map");
        // minimap framerate
        minimapCamera.targetTexture = minimapRendertexture;
        CameraFrameRate fr = minimapCameraObject.AddComponent<CameraFrameRate>();
        fr.cam = minimapCamera;
        fr.targetFrameRate = 5;

        // Map camera game object
        mapCameraObject = new GameObject("Map Camera");
        mapCameraObject.transform.parent = this.transform;
        mapCameraObject.transform.position = new Vector3(0f, 5f, 0f);
        mapCameraObject.transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
        // map camera setting
        mapCamera = mapCameraObject.AddComponent<Camera>();
        mapCamera.enabled = false;
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = 14f;
        mapCamera.cullingMask = LayerMask.GetMask("Robot", "Map");
        mapCamera.targetTexture = mapRendertexture;

        SetUIActive(true);
    }

    public void SetTask(Task task)
    {
        currentTask = task;
    }


    // GUI ENABLED
    public void SetRecordIconActive(bool active)
    {
        recordIcon.SetActive(active);
    }

    public void ChangeHelpDisplay()
    {
        helpDisplay.SetActive(!helpDisplay.activeSelf);
    }

    public void ChangeBarCodeScanDisplay()
    {
        barCodeScanDisplay.SetActive(!barCodeScanDisplay.activeSelf);
    }


    // GUI FUNCTIONS
    public void ChangeCameraView()
    {
        // If same as secondary view
        if ( ((cameraIndex + 1) % numCameras) == secondaryCameraIndex )
        {
            SwitchCameraView();
        }
        else
        {
            // Disable current camera
            cameraSystem.DisableCamera(cameraIndex);
            // Enable next camera
            cameraIndex = (cameraIndex + 1) % numCameras;
            cameraSystem.EnableCamera(cameraIndex);
            cameraSystem.SetTargetRenderTexture(cameraIndex, cameraRendertexture);
        }
    }

    public void SwitchCameraView()
    {
        cameraSystem.DisableAllCameras();
        // Switch main index and secondary index
        int temp = secondaryCameraIndex;
        secondaryCameraIndex = cameraIndex;
        cameraIndex = temp;
        // update main view
        cameraSystem.SetTargetRenderTexture(cameraIndex, cameraRendertexture);
        cameraSystem.EnableCamera(cameraIndex);
        // update secondary camera
        cameraSystem.SetTargetRenderTexture(secondaryCameraIndex, secondayCameraRendertexture);
        cameraSystem.EnableCamera(secondaryCameraIndex);
    }

    public void ChangeMinimapView()
    {
        // Next avaliable camera size
        minimapSizeIndex = (minimapSizeIndex + 1) % minimapSizes.Length;
        minimapCamera.orthographicSize = minimapSizes[minimapSizeIndex];
    }

    public void ZoomMap()
    {
        if (displayMapInMain)
        {
            map.transform.position = tempMapPosition;
            map.transform.rotation = tempMapRotation;
            // set main view back
            cameraSystem.EnableCamera(cameraIndex);
            mapCamera.enabled = false;
            cameraDisplay.GetComponent<RawImage>().texture = cameraRendertexture;
        }
        else
        {
            tempMapPosition = map.transform.position;
            tempMapRotation = map.transform.rotation;
            // set main view to map view
            cameraSystem.DisableCamera(cameraIndex);
            mapCamera.enabled = true;
            cameraDisplay.GetComponent<RawImage>().texture = mapRendertexture;
        }
        displayMapInMain = !displayMapInMain;
    }

    public void ShowPopUpMessage(string message, float duration=1.5f)
    {
        messagePanelText.text = message;
        StartCoroutine(PopUpMessageCoroutine(duration));
    }
    private IEnumerator PopUpMessageCoroutine(float duration)
    {
        messagePanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        messagePanel.SetActive(false);
    }

    public void CheckInput()
    {
        // Let task handle the input
        if (currentTask != null)
            currentTask.CheckInput(input.text);
        // Clear input
        input.text = "";
    }

    public Camera GetCurrentMainCamera()
    {
        if (robot != null)
            return cameraSystem.cameras[cameraIndex];
        else
            return null;
    }


    // FPS
    private void UpdateFPS()
    {
        FPS = (int)(FPSSum / FPSCount);
        FPSSum = 0;
        FPSCount = 0;
    }
}
