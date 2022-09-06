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
    private AutoNavigation autoNavigation;
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
    // laser
    private Laser laser;

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
    private Vector3 prevClickPoint = Vector3.zero;
    
    // battery
    private float robotSpawnedTime;
    private Slider batterySlider;

    // control mode
    public GameObject controlModeCameraDisplay;
    public GameObject controlModeLeftDisplay;
    public GameObject controlModeRightDisplay;
    public GameObject controlModeBaseDisplay;
    private GopherControl gopherControl;
    // camera control selection
    public GameObject headCameraViewingDisplay;
    public GameObject leftCameraViewingDisplay;
    public GameObject rightCameraViewingDisplay;
    public GameObject backCameraViewingDisplay;
    // ik
    private GameObject leftEndEffectorRef; 
    private GameObject rightEndEffectorRef;
    private NewtonIK leftIKSolver; 
    private NewtonIK rightIKSolver;
   
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
    
    // Timer
    public GameObject timerPanel;
    private TextMeshProUGUI timerPanelText;
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
        minimapSizes = new float[] {3f, 6f, 9f};
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

        // Timer
        timerPanelText = timerPanel.GetComponentInChildren<TextMeshProUGUI>();
        // FPS
        FPSCount = 0;
        FPSSum = 0;
        InvokeRepeating("UpdateFPS", 1.0f, 0.5f);
    }


    public void SetUIActive(bool active)
    {
        this.active = active;
        foreach (GameObject component in interfaces)
        {
            component.SetActive(active);
        }
    }

    void Update()
    {
        // Timer
        timerPanelText.text = Time.unscaledTime.ToString("0.0");
        // FPS
        FPSCount += 1;
        FPSSum += 1.0f / Time.deltaTime;

        // Active
        if (robot == null)
            SetUIActive(false);
        // UI not activated
        if (!active)
            return;

        // Update UI
        UpdateRobotStatus();
        UpdateTaskStatus();

        // UI Keys
        if (input.isFocused)
            return;
        // help panel
        if (Input.GetKeyDown(KeyCode.H)) 
            ChangeHelpDisplay();
        // minimap
        if(Input.GetKeyDown(KeyCode.M))
            if (Input.GetKey(KeyCode.LeftShift))
                ChangeMinimapView();
            else
                ZoomMap();
        // map navigation
        if (displayMapInMain && autoNavigation != null &&
            Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mapCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
                SetNavigationGoal(hit.point);
        }
        // barcode
        if (Input.GetKeyDown(KeyCode.B))
            ChangeBarCodeScanDisplay();

        // Switch camera control
        if (Input.GetKeyDown(KeyCode.Keypad8))
            ChangeCameraView(!Input.GetKey(KeyCode.Keypad0), "Head");
        else if (Input.GetKeyDown(KeyCode.Keypad5))
            ChangeCameraView(!Input.GetKey(KeyCode.Keypad0), "Back");
        else if (Input.GetKeyDown(KeyCode.Keypad4))
            ChangeCameraView(!Input.GetKey(KeyCode.Keypad0), "Left");
        else if (Input.GetKeyDown(KeyCode.Keypad6))
            ChangeCameraView(!Input.GetKey(KeyCode.Keypad0), "Right");

        MonitorKeyPressed();
    }
    // Record key pressed during the use of interface
    // TODO though it works, 1) may not be the best script to put this code, and
    // 2) may not be the most reasonable to put all key pressed under task string record
    private void MonitorKeyPressed()
    {
        if (currentTask == null || robot == null)
            return;
        
        // Control modes
        if (Input.GetKeyDown(KeyCode.DownArrow)
            && gopherControl.Mode != GopherControl.ControlMode.Base)
            RecordKey("DownArrow");
        else if (Input.GetKeyDown(KeyCode.LeftArrow)
                 && gopherControl.Mode != GopherControl.ControlMode.LeftArm)
            RecordKey("LeftArrow");
        else if (Input.GetKeyDown(KeyCode.RightArrow)
                 && gopherControl.Mode != GopherControl.ControlMode.RightArm)
            RecordKey("RightArrow");
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            RecordKey("UpArrow");

        // Joint presets
        if (gopherControl.Mode != GopherControl.ControlMode.Base)
            if (Input.GetKeyDown(KeyCode.F1))
                RecordKey("F1");
            else if (Input.GetKeyDown(KeyCode.F2))
                RecordKey("F2");
            else if (Input.GetKeyDown(KeyCode.F3))
                RecordKey("F3");
            else if (Input.GetKeyDown(KeyCode.F4))
                RecordKey("F4");
            else if (Input.GetKeyDown(KeyCode.F5))
                RecordKey("F5");
            else if (Input.GetKeyDown(KeyCode.F8))
                RecordKey("F8");

        // Automation
        if (Input.GetKeyDown(KeyCode.T))
            RecordKey("T");
        if (Input.GetKeyDown(KeyCode.G))
            RecordKey("G");

        // Camera Switch
        if (Input.GetKeyDown(KeyCode.Keypad8))
            RecordKey((!Input.GetKey(KeyCode.Keypad0)).ToString() + ",Keypad8");
        else if (Input.GetKeyDown(KeyCode.Keypad5))
            RecordKey((!Input.GetKey(KeyCode.Keypad0)).ToString() + ",Keypad5");
        else if (Input.GetKeyDown(KeyCode.Keypad4))
            RecordKey((!Input.GetKey(KeyCode.Keypad0)).ToString() + ",Keypad4");
        else if (Input.GetKeyDown(KeyCode.Keypad6))
            RecordKey((!Input.GetKey(KeyCode.Keypad0)).ToString() + ",Keypad6");
    }
    private void RecordKey(string key)
    {
        // Record keys but not for checking result
        currentTask.CheckInput(key, true);
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
        UpdateControlMode(gopherControl.cameraControlEnabled, gopherControl.Mode);
        UpdateCameraViewing(cameraIndex);
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

    private void UpdateControlMode(bool cameraControlEnabled, 
                                   GopherControl.ControlMode controlMode)
    {
        // Camera
        controlModeCameraDisplay.SetActive(cameraControlEnabled);
        
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

    private void UpdateCameraViewing(int cameraIndex)
    {
        headCameraViewingDisplay.SetActive(false);
        leftCameraViewingDisplay.SetActive(false);
        rightCameraViewingDisplay.SetActive(false);
        backCameraViewingDisplay.SetActive(false);

        string secondaryCameraName = cameraSystem.GetName(secondaryCameraIndex).ToUpper();
        switch (secondaryCameraName)
        {
            case "HEAD":
                headCameraViewingDisplay.SetActive(true);
                headCameraViewingDisplay.GetComponent<RectTransform>().localScale = 
                    new Vector3(0.6f, 0.6f, 0f);
                break;
            case "LEFT":
                leftCameraViewingDisplay.SetActive(true);
                leftCameraViewingDisplay.GetComponent<RectTransform>().localScale = 
                    new Vector3(0.6f, 0.6f, 0f);
                break;
            case "RIGHT":
                rightCameraViewingDisplay.SetActive(true);
                rightCameraViewingDisplay.GetComponent<RectTransform>().localScale = 
                    new Vector3(0.6f, 0.6f, 0f);
                break;
            case "BACK":
                backCameraViewingDisplay.SetActive(true);
                backCameraViewingDisplay.GetComponent<RectTransform>().localScale = 
                    new Vector3(0.6f, 0.6f, 0f);
                break;
            default:
                break;
        }

        string cameraName = cameraSystem.GetName(cameraIndex).ToUpper();
        switch (cameraName)
        {
            case "HEAD":
                headCameraViewingDisplay.SetActive(true);
                headCameraViewingDisplay.GetComponent<RectTransform>().localScale = Vector3.one;
                break;
            case "LEFT":
                leftCameraViewingDisplay.SetActive(true);
                leftCameraViewingDisplay.GetComponent<RectTransform>().localScale = Vector3.one;
                break;
            case "RIGHT":
                rightCameraViewingDisplay.SetActive(true);
                rightCameraViewingDisplay.GetComponent<RectTransform>().localScale = Vector3.one;
                break;
            case "BACK":
                backCameraViewingDisplay.SetActive(true);
                backCameraViewingDisplay.GetComponent<RectTransform>().localScale = Vector3.one;
                break;
            default:
                break;
        }
    }

    private void UpdateHelpDisplay(GopherControl.ControlMode controlMode)
    {
        helpDisplayText.text = "Switch Control\n" + "  Key ← ↓ →\n"
                             + "Switch Camera\n" + "  Num (0+) 8 4 5 6\n"
                             + "Camera Control\n" + "  Enabling: ↑, Mouse\n"
                             + "Camera Centering\n" + "  Mouse middle button\n";
        switch (controlMode)
        {
            case GopherControl.ControlMode.LeftArm:
            case GopherControl.ControlMode.RightArm:
                helpDisplayText.text += "Joint Position Control\n" + "  WA/SD/QE\n"
                                      + "Joint Rotation Control\n" + "  IK/JL/UO\n"
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

    private void UpdateLocalization(Vector3 position, Vector3 rotation)
    {
        // location name
        string location = HospitalMapUtil.GetLocationName(position);
        locationText.text = "Current - " + location;
        // update map in minimap display
        map.transform.position = position - robot.transform.position + new Vector3(0f, -3f, 0f);
        map.transform.rotation = Quaternion.Euler(rotation - robot.transform.rotation.eulerAngles); 
    }

    private void UpdateBattery(float duration)
    {
        // Convert duration time to battery
        // (0, 100)min -> (1.0, 0.0)
        float value = (100 - duration / 60f) / 100f;
        batterySlider.value = Mathf.Clamp(value, 0f, 1f);
    }

    
    // Setup ROBOT AND TASK
    public void SetRobot(GameObject robot, bool isNewRobot)
    {
        // Robot
        this.robot = robot;
        cameraSystem = robot.GetComponentInChildren<CameraSystem>();
        laser = robot.GetComponentInChildren<Laser>();
        localization = robot.GetComponentInChildren<Localization>();
        stateReader = robot.GetComponentInChildren<StateReader>();
        gopherControl = robot.GetComponentInChildren<GopherControl>();
        autoNavigation = robot.GetComponentInChildren<AutoNavigation>();
        // Get IK and End effector reference transform for
        // switching IK reference frame based on camera view
        // TODO 1 left first and right next, no better way to tell each apart
        NewtonIK[] iKSolvers = robot.GetComponentsInChildren<NewtonIK>();
        leftIKSolver = iKSolvers[0];
        rightIKSolver = iKSolvers[1];
        Grasping[] graspings = robot.GetComponentsInChildren<Grasping>();
        leftEndEffectorRef = new GameObject("gopher/left_arm_ik_reference");
        rightEndEffectorRef = new GameObject("gopher/right_arm_ik_reference");
        leftEndEffectorRef.transform.parent = graspings[0].endEffector.transform;
        leftEndEffectorRef.transform.localPosition = Vector3.zero;
        rightEndEffectorRef.transform.parent = graspings[1].endEffector.transform;
        rightEndEffectorRef.transform.localPosition = Vector3.zero;
        // TODO 2 The proper reference is different from the tool frame...
        leftEndEffectorRef.transform.localRotation = Quaternion.Euler(new Vector3(0f, -90f, 180f));
        rightEndEffectorRef.transform.localRotation = Quaternion.Euler(new Vector3(0f, -90f, 180f));
        
        // Cameras
        cameraSystem.enabled = true;
        numCameras = cameraSystem.cameras.Length;
        cameraSystem.DisableAllCameras();
        // turn on main camera and secondary camera
        cameraIndex = cameraSystem.GetIndex("Head");
        secondaryCameraIndex = cameraSystem.GetIndex("Right");
        cameraSystem.SetTargetRenderTexture(cameraIndex, cameraRendertexture);
        cameraSystem.EnableCamera(cameraIndex);
        cameraSystem.SetTargetRenderTexture(secondaryCameraIndex, secondayCameraRendertexture);
        cameraSystem.EnableCamera(secondaryCameraIndex);
        // TODO may be all this part is not necessary for existing robot?
        // TEMP fix
        ChangeCameraView(true, cameraIndex);
        ChangeCameraView(false, secondaryCameraIndex);

        // Hide laser scan in the cameras
        foreach (Camera cam in cameraSystem.cameras)
            cam.cullingMask = cam.cullingMask & ~(1 << LayerMask.NameToLayer("Laser"));
        
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
        minimapCamera.cullingMask = LayerMask.GetMask("Robot", "Laser", "Map");
        // minimap framerate
        minimapCamera.targetTexture = minimapRendertexture;
        CameraFrameRate fr = minimapCameraObject.AddComponent<CameraFrameRate>();
        fr.cam = minimapCamera;
        fr.targetFrameRate = 5;

        // Map camera game object
        if (mapCameraObject != null)
            Destroy(mapCameraObject);
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
    public void ChangeCameraView(bool mainCamera, string cameraName)
    {
        ChangeCameraView(mainCamera, cameraSystem.GetIndex(cameraName));
    }
    public void ChangeCameraView(bool mainCamera, int index)
    {
        if (index < 0 || index >= cameraSystem.cameras.Length)
            return;
        
        // Change main camera view
        if (mainCamera)
        {
            if (index != secondaryCameraIndex)
            {
                cameraSystem.DisableCamera(cameraIndex);
                cameraIndex = index;
            }
            // Swap if same as secondary
            else
            {
                secondaryCameraIndex = cameraIndex;
                cameraIndex = index;
            }
        }
        // Change secondary camera view
        else
        {
            if (index != cameraIndex)
            {
                cameraSystem.DisableCamera(secondaryCameraIndex);
                secondaryCameraIndex = index;
            }
            // Swap if same as main
            else
            {
                cameraIndex = secondaryCameraIndex;
                secondaryCameraIndex = index;
            }
        }
        cameraSystem.SetTargetRenderTexture(cameraIndex, cameraRendertexture);
        cameraSystem.SetTargetRenderTexture(secondaryCameraIndex, secondayCameraRendertexture);
        cameraSystem.EnableCamera(cameraIndex);
        cameraSystem.EnableCamera(secondaryCameraIndex);

        // Adjust IK local transform based on viewing camera
        if (cameraIndex == cameraSystem.GetIndex("Head") ||
            cameraIndex == cameraSystem.GetIndex("Back"))
        {
            leftIKSolver.localToWorldTransform = robot.transform;
            rightIKSolver.localToWorldTransform = robot.transform;
        }
        else if (cameraIndex == cameraSystem.GetIndex("Left"))
        {
            leftIKSolver.localToWorldTransform = leftEndEffectorRef.transform;
            rightIKSolver.localToWorldTransform = robot.transform;
        }
        else if (cameraIndex == cameraSystem.GetIndex("Right"))
        {
            leftIKSolver.localToWorldTransform = robot.transform;
            rightIKSolver.localToWorldTransform = rightEndEffectorRef.transform;
        }
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
            Time.timeScale = 1f;
        }
        else
        {
            tempMapPosition = map.transform.position;
            tempMapRotation = map.transform.rotation;
            // set main view to map view
            cameraSystem.DisableCamera(cameraIndex);
            mapCamera.enabled = true;
            cameraDisplay.GetComponent<RawImage>().texture = mapRendertexture;
            // disable main camera control to have the mouse back
            gopherControl.cameraControlEnabled = false;
            Cursor.lockState = CursorLockMode.Confined;
            // Time.timeScale = 0f; // Can not stop due to auto path finding
            Time.timeScale = 0.5f;
        }
        displayMapInMain = !displayMapInMain;
    }

    public void SetNavigationGoal(Vector3 point)
    {
        // Cancel previous goal
        if ((point - prevClickPoint).magnitude < 0.5)
        {
            autoNavigation.DisableAutonomy();
            prevClickPoint = Vector3.zero;
        }
        // Set goal
        else
        {
            autoNavigation.SetGoal(point);
            prevClickPoint = point;
        }
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
        // Record input and let task handle it
        if (currentTask != null)
            currentTask.CheckInput(input.text, false);
        // Clear input
        input.text = "";
    }

    public Camera[] GetCurrentActiveCameras()
    {
        Camera[] cameras = new Camera[0];
        if (robot != null)
        {
            cameras = new Camera[2];
            cameras[0] = cameraSystem.cameras[cameraIndex];
            cameras[1] = cameraSystem.cameras[secondaryCameraIndex];
        }

        return cameras;
    }


    // FPS
    private void UpdateFPS()
    {
        FPS = (int)(FPSSum / FPSCount);
        FPSSum = 0;
        FPSCount = 0;
    }
}
