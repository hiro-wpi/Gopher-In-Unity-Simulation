using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{   
    // UI
    public UIManager uIManager;

    // Robot
    public GameObject robotPrefab;
    public GameObject robotPrefab2;
    public GameObject robot;
    // wheel
    private KeyboardWheelControl wheelController;
    // camera
    private Camera[] cameras;
    private MouseCameraControl[] cameraControllers;
    public int cameraIndex;
    private float[] cameraFOV;
    public int cameraFOVIndex;
    public RenderTexture[] cameraRenderTextures;
    public RenderTexture regularCameraRendertexture;
    public RenderTexture wideCameraRendertexture;
    public int cameraMobility;
    public int cameraDesiredFrameRate;

    // Scene
    public string mainScene;
    
    // Task
    public bool isExperimenting;
    public GameObject[] levelPrefabs;
    public Vector3[] spawnPositions;
    public Vector3[] spawnRotations;
    public Vector3[] goalPositions;
    private Vector3 currentGoalPosition;
    private float goalDectionRadius = 1.5f;
    public GameObject goalPrefab;
    public string[] tasks;

    public GameObject[] numberBoardPrefab;
    private float[,,] numberBoardXRanges;
    private float[,,] numberBoardZRanges;
    private float[,] numberBoardYRotation;
    private int wallNumberSum;

    public GameObject humanModelPrefab;
    public Vector3[,] humanSpawnPose;
    public float[,] humanActionDectionRange;
    public Vector3[,] humanTrajectory;

    public Vector3[,] humanSpawnPose2;
    public Vector3[,] humanTrajectory2;


    public int levelIndex;
    public int taskIndex;

    // Data
    public bool isRecording = false;
    public DataRecorder dataRecorder;
    
    void Start()
    {
        // Robot
        // camera
        cameraIndex = 0;
        cameraFOVIndex = 0;
        cameraMobility = 0;
        cameraFOV = new float[] {69.4f, 91.1f};
        cameraRenderTextures = new RenderTexture[] {regularCameraRendertexture, 
                                                    wideCameraRendertexture};

        // Scene
        numberBoardXRanges = new float[,,]
                                {{{0f, 0f},         {-6.0f, -4.3f},     {-3.6f, -2.95f}, {0f, 0f}, {0f, 0f}},
                                 {{-6.75f, -6.25f}, {-11.15f, -11.15f}, {-3.8f, -3.8f},  {-8.4f, -8.4f}, {0f, 0f}},
                                 {{-8.85f, -8.85f}, {-14.9f, -14.9f},   {-14.8f, -10f},  {-14.9f, -14.9f}, {-14.9f, -14.9f}},
                                 {{-8.85f, -8.85f}, {-14.9f, -14.9f},   {-14.8f, -10f},  {-14.9f, -14.9f}, {-14.9f, -14.9f}}};
        numberBoardZRanges = new float[,,]
                                {{{-18.5f, -16.2f}, {-13.1f, -13.1f}, {-19.9f, -19.9f}, {0f, 0f}, {0f, 0f}},
                                 {{-17.5f, -17.5f}, {-21.9f, -20.7f}, {-23.8f, -22.3f}, {-22.5f, -22.1f}, {0f, 0f}},
                                 {{-9.3f, -6.2f},   {-2.8f, -1.3f},   {1.4f, 1.4f}    , {-6.4f, -4.0f}, {-9.2f, -6.8f}},
                                 {{-9.3f, -6.2f},   {-2.8f, -1.3f},   {1.4f, 1.4f}    , {-6.4f, -4.0f}, {-9.2f, -6.8f}}};
        numberBoardYRotation = new float[,]
                                {{-90f, 180f, 0f, 0f, 0f},
                                 {180f, 90f, -90f, 90f, 0f},
                                 {-90f, 90f, 180f, 90f, 90f},
                                 {-90f, 90f, 180f, 90f, 90f}};
        levelIndex = 0;
        taskIndex = 0;

        // task
        humanSpawnPose = new Vector3[,]
                            {{new Vector3(5.6f, 0f, 3.8f), new Vector3(0f, 180f, 0f)},
                             {new Vector3(6.2f, 0f, -9.5f), new Vector3(0f, 0f, 0f)},
                             {new Vector3(-7f, 0f, -11.5f), new Vector3(0f, 90f, 0f)}, 
                             {new Vector3(-7.4f, 0f, -7f), new Vector3(0f, 180f, 0f)}, 
                             {new Vector3(-10f, 0f, -16.4f), new Vector3(0f, 90f, 0f)}};
        humanActionDectionRange = new float[,]
                            {{4f, 8f, 0f, 6f}, {4f, 8f, 0f, 6f},
                             {0f, 6.5f, -13f, -10f}, {-13f, 0f, -13f, -10f},
                             {-9f, -6f, -20f, -13.5f}};
        humanTrajectory = new Vector3[,]
                            {{new Vector3(5.6f, 0f, -10f), new Vector3(11.3f, 0f, -13f)},
                             {new Vector3(6.2f, 0f, 6.1f), new Vector3(14.7f, 0f, 6.1f)},
                             {new Vector3(-2.0f, 0f, -12.0f), new Vector3(-0.7f, 0f, -7.7f)},
                             {new Vector3(-7.4f, 0f, -7f), new Vector3(-7.4f, 0f, -20f)}, 
                             {new Vector3(-10f, 0f, -16.4f), new Vector3(-2.5f, 0f, -16.4f)}};

        // FPS
        cameraDesiredFrameRate = 30;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }

    void Update()
    {
        // Hotkeys
        if (robot != null)
        {
            // camera
            if (!isExperimenting)
            {
                if (Input.GetKeyDown(KeyCode.Tab)) 
                    ChangeCameraView();
                if (Input.GetKeyDown(KeyCode.V))
                    ChangeCameraFOV();
                if (Input.GetKeyDown(KeyCode.LeftControl))
                    ChangeCameraControl();
            }

            // wheel
            if (Input.GetKeyDown(KeyCode.LeftShift))
                ChangeRobotSpeed();

            // record
            if (Input.GetKeyDown(KeyCode.R) && !isExperimenting)
                Record();
        }
    }

    public void ReloadScene()
    {
        LoadSceneWithRobot(taskIndex, levelIndex, cameraIndex, cameraFOVIndex, cameraMobility);
    }

    public void LoadSceneWithRobot(int taskIndex, int levelIndex,
                                   int cameraIndex=0, int cameraFOVIndex=0, int cameraMobility=0)
    {   
        // Load task
        this.taskIndex = taskIndex;
        this.levelIndex = levelIndex;

        // Load camera configuration
        this.cameraIndex = cameraIndex;
        this.cameraFOVIndex = cameraFOVIndex;
        this.cameraMobility = cameraMobility;

        // Keep this manager
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(uIManager);
        // Load scene
        StartCoroutine(LoadSceneWithRobotCoroutine());
        // Loading sign
    }
    private IEnumerator LoadSceneWithRobotCoroutine()
    {
        // mainScene
        SceneManager.LoadScene(mainScene);
        yield return new WaitForSeconds(0.5f);

        // level
        Instantiate(levelPrefabs[levelIndex], new Vector3(), new Quaternion());
        // task
        GenerateHumanModel();
        if (taskIndex == 4)
            GenerateNumberBoard();
        yield return new WaitForSeconds(0.5f); 

        // Robot
        SpawnRobot();
    }

    private void GenerateHumanModel()
    {
        if (taskIndex == 0 && levelIndex != 3)
        {
            GameObject taskNurse = Instantiate(humanModelPrefab, 
                                                humanSpawnPose[0, 0], 
                                                Quaternion.Euler(humanSpawnPose[0, 1]));
            StartCoroutine(CharacterMoveOnAction(taskNurse, 0, humanTrajectory));
        }

        if (levelIndex == 2 && taskIndex != 4)
        {
            GameObject levelNurse = Instantiate(humanModelPrefab, 
                                                humanSpawnPose[taskIndex+1, 0], 
                                                Quaternion.Euler(humanSpawnPose[taskIndex+1, 1]));
            StartCoroutine(CharacterMoveOnAction(levelNurse, taskIndex+1, humanTrajectory));
        }

        if (levelIndex == 3 && taskIndex != 4)
        {
            if (taskIndex == 0)
            {
                Vector3[,] humanTrajectory40 = new Vector3[,]
                                {{new Vector3(5.6f, 0f, -0.3f), new Vector3(5f, 0f, -1.1f), 
                                  new Vector3(3.3f, 0f, -1.1f), new Vector3(0.5f, 0f, -1.9f),
                                  new Vector3(-1.8f, 0f, -1.1f), new Vector3(-6.4f, 0f, -1.4f),
                                  new Vector3(-10.4f, 0f, -1.4f)}};
            
                GameObject taskNurse = Instantiate(humanModelPrefab, 
                                                    humanSpawnPose[0, 0], 
                                                    Quaternion.Euler(humanSpawnPose[0, 1]));
                CharacterWalk characterWalk = taskNurse.GetComponent<CharacterWalk>();
                StartCoroutine(CharacterMoveOnAction(taskNurse, 0, humanTrajectory40));
            }

            if (taskIndex != 2 && taskIndex != 3)
            {
                GameObject levelNurse = Instantiate(humanModelPrefab,
                                                    humanSpawnPose[taskIndex + 1, 0],
                                                    Quaternion.Euler(humanSpawnPose[taskIndex + 1, 1]));
                StartCoroutine(CharacterMoveOnAction(levelNurse, taskIndex + 1, humanTrajectory));
            }

            // Temp
            int ii = 0;
            float r = Random.Range(0f, 1f);
            if (r > 0.5f)
                ii = 0;
            else
                ii = 2;

            if (taskIndex == 1 || taskIndex == 2)
            {
                Vector3[] spawnP = new Vector3[]
                            {new Vector3(-7.4f, 0f, -16f), new Vector3(0f, 0f, 0f), 
                             new Vector3(-7.4f, 0f, -6.8f), new Vector3(0f, 180f, 0f)};
                Vector3[] humanTraj = new Vector3[]
                            {new Vector3(-7.4f, 0f, -16f), new Vector3(-7.4f, 0f, -11.6f), 
                             new Vector3(-7.4f, 0f, -6.8f)};
                GameObject loopNurse = Instantiate(humanModelPrefab,
                                                   spawnP[ii],
                                                   Quaternion.Euler(spawnP[ii + 1]));
                StartCoroutine(CharacterMoveWait(loopNurse, humanTraj));
            }
            else if (taskIndex == 3)
            {
                Vector3[] spawnP = new Vector3[]
                            {new Vector3(-12.4f, 0f, -16.3f), new Vector3(0f, 90f, 0f), 
                             new Vector3(-2.4f, 0f, -16.3f), new Vector3(0f, -90f, 0f)};
                Vector3[] humanTraj = new Vector3[]
                            {new Vector3(-12.4f, 0f, -16.3f), new Vector3(-7.4f, 0f, -16.3f), 
                             new Vector3(-2.4f, 0f, -16.3f)};
                GameObject loopNurse = Instantiate(humanModelPrefab,
                                                   spawnP[ii],
                                                   Quaternion.Euler(spawnP[ii + 1]));
                StartCoroutine(CharacterMoveWait(loopNurse, humanTraj));
            }
        }

        if (levelIndex == 3 && taskIndex != 4)
        {
            GameObject levelNurse = Instantiate(humanModelPrefab,
                                                humanSpawnPose[taskIndex + 1, 0],
                                                Quaternion.Euler(humanSpawnPose[taskIndex + 1, 1]));
            StartCoroutine(CharacterMoveOnAction(levelNurse, taskIndex + 1, humanTrajectory));
        }
    }

    private IEnumerator CharacterMoveWait(GameObject character, Vector3[] traj)
    {
        if (character == null)
            yield break;

        CharacterWalk characterWalk = character.GetComponent<CharacterWalk>();
        characterWalk.loop = true;

        yield return new WaitForSeconds(Random.Range(1f, 3f));

        // In case previous human gets destroyed
        if (characterWalk != null)
            characterWalk.MoveTrajectory(traj);
    }

    private IEnumerator CharacterMoveOnAction(GameObject character, int index, Vector3[,] humanTrajectory)
    {
        if (character == null)
            yield break; 
            
        CharacterWalk characterWalk = character.GetComponent<CharacterWalk>();

        float xLower = humanActionDectionRange[index, 0];
        float xUpper = humanActionDectionRange[index, 1];
        float zLower = humanActionDectionRange[index, 2];
        float zUpper = humanActionDectionRange[index, 3];

        yield return new WaitUntil(() => RobotInArea(xLower, xUpper, zLower, zUpper) == true);

        Vector3[] trajectory = new Vector3[humanTrajectory.GetLength(1)];
        for (int r = 0; r < trajectory.Length; ++r)
            trajectory[r] = humanTrajectory[index, r];
        
        // In case previous human gets destroyed
        if (characterWalk != null)
            characterWalk.MoveTrajectory(trajectory);
    }


    private bool RobotInArea(float xLower, float xUpper, float zLower, float zUpper)
    {
        if (robot == null)
            return false;

        if ((xLower < robot.transform.position.x) && (robot.transform.position.x < xUpper) && 
            (zLower < robot.transform.position.z) && (robot.transform.position.z < zUpper))
            return true;
        else
            return false;
    }

    private void GenerateNumberBoard()
    {
        System.Random randomInt = new System.Random();

        float locationX;
        float locationZ;

        int[] wallIndices;
        if (levelIndex == 0)
            wallIndices = new int[] {0, 1, 2};
        else if (levelIndex == 1)
            wallIndices = new int[] {0, 1, 2, 3};
        else
            wallIndices = new int[] {0, 1, 2, 3, 4};

        wallIndices = wallIndices.OrderBy(x => randomInt.Next()).ToArray();
        
        wallNumberSum = 0;
        for (int i = 0; i < levelIndex+1; ++i)
        {
            int wallIndex = wallIndices[i];
            int number = randomInt.Next(numberBoardPrefab.Length);
            wallNumberSum += number;

            float xLower = numberBoardXRanges[levelIndex, wallIndex, 0];
            float xUpper = numberBoardXRanges[levelIndex, wallIndex, 1];
            float zLower = numberBoardZRanges[levelIndex, wallIndex, 0];
            float zUpper = numberBoardZRanges[levelIndex, wallIndex, 1];
            float yRotation = numberBoardYRotation[levelIndex, wallIndex];
            
            locationX = Random.Range(xLower, xUpper);
            locationZ = Random.Range(zLower, zUpper);
            GameObject boardPrefab = numberBoardPrefab[number];

            Instantiate(boardPrefab, new Vector3(locationX, 1.5f, locationZ), 
                                     Quaternion.Euler(new Vector3(0f, yRotation, 0f)));
        }
    }

    private void SpawnRobot()
    {
        // Spawn
        // robot
        int spawnIndex = taskIndex;
        if (taskIndex == 4)
            spawnIndex += levelIndex;
        if (levelIndex != 3)
            robot = Instantiate(robotPrefab, spawnPositions[spawnIndex], 
                                         Quaternion.Euler(spawnRotations[spawnIndex]));
        else
            robot = Instantiate(robotPrefab2, spawnPositions[spawnIndex],
                                         Quaternion.Euler(spawnRotations[spawnIndex]));
        // goal
        currentGoalPosition = goalPositions[taskIndex];
        if (taskIndex == 4)
            currentGoalPosition = new Vector3(0f, -10f, 0f);
        else if (taskIndex == 3)
            currentGoalPosition = goalPositions[taskIndex + levelIndex];
        else if (levelIndex == 3)
            currentGoalPosition = goalPositions[taskIndex + 7];
        StopCoroutine(CheckGoalCoroutine());
        StartCoroutine(CheckGoalCoroutine());

        // Get components
        wheelController = robot.GetComponentInChildren<KeyboardWheelControl>();
        cameras = robot.GetComponentsInChildren<Camera>();
        cameraControllers = robot.GetComponentsInChildren<MouseCameraControl>();
        // Initialization
        InitializeCameras();
        
        // Set up data recorder
        StartCoroutine(InitializeDataRecorder());
    }

    private IEnumerator CheckGoalCoroutine()
    {
        Instantiate(goalPrefab, currentGoalPosition, new Quaternion());
        yield return new WaitUntil(() => CheckGoalReached() == true);
    }
    private bool CheckGoalReached()
    {
        if (robot == null)
            return false;
        if ((robot.transform.position - currentGoalPosition).magnitude < goalDectionRadius)
        {
            if (!isExperimenting)
                uIManager.PopMessage("Goal Reached");
            else
            {
                if(isRecording)
                    Record();
                uIManager.LoadNextLevelUI();
            }
            return true;
        }
        else
            return false;
    }

    public void CheckNumberBoardAnswer(int answer)
    {
        if (answer == wallNumberSum)
        {
            if (!isExperimenting)
            {
                uIManager.PopMessage("Correct!");
            }
            else
            {
                if(isRecording)
                    Record();
                uIManager.LoadNextLevelUI();
            }
        }
        else
        {
            uIManager.PopMessage("Please try again");
        }
        uIManager.LoadQuitScene();
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

        cameras[cameraIndex].enabled = true;
        // InvokeRepeating("CameraRender", 0f, 1f/cameraDesiredFrameRate);

        // Camera control
        foreach (MouseCameraControl controller in cameraControllers)
            controller.enabled = false;

        cameraControllers[cameraIndex].enabled = (cameraMobility == 1);
        if ((cameraMobility == 1))
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.Confined;
    }
    private void CameraRender()
    {
        if (cameras[cameraIndex] != null)
            cameras[cameraIndex].Render();
    }

    private IEnumerator InitializeDataRecorder()
    {
        dataRecorder.setRobot(robot);
        yield return new WaitForSeconds(0.5f);
        dataRecorder.updateData = true;
    }

    // UI - camera
    public void ChangeCameraView()
    {
        cameras[cameraIndex].enabled = false;
        // TOFIX
        // cameraControllers[cameraIndex].HomeCameraJoints();
        bool mobility = cameraControllers[cameraIndex].enabled;
        cameraControllers[cameraIndex].enabled = false;

        cameraIndex = (cameraIndex+1) % cameras.Length;

        cameras[cameraIndex].enabled = true;
        cameraControllers[cameraIndex].enabled = mobility;
    }

    public void ChangeCameraFOV()
    {
        cameraFOVIndex = (cameraFOVIndex+1) % cameraFOV.Length;
        
        foreach (Camera camera in cameras)
        {
            camera.enabled = false;
            camera.fieldOfView = cameraFOV[cameraFOVIndex];
            camera.targetTexture = cameraRenderTextures[cameraFOVIndex];
        }
        cameras[cameraIndex].enabled = true;

        // reload UI
        uIManager.LoadRobotUI();
    }

    public void ChangeCameraControl()
    {
        bool mobility = !(cameraMobility == 1);
        cameraMobility = (cameraMobility == 1)? 0:1;

        foreach (MouseCameraControl controller in cameraControllers)
            controller.enabled = false;

        cameraControllers[cameraIndex].enabled = mobility;
        if (mobility)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.Confined;
    }

    // UI - wheel
    public void ChangeRobotSpeed()
    {
        if (wheelController.speed == 1.5f)
        {
            wheelController.speed = 1.0f;
            wheelController.angularSpeed = 1.0f;
        }
        else
        {
            wheelController.speed = 1.5f;
            wheelController.angularSpeed = 1.5f;
        }
    }

    // Data
    public void Record(string filePrefix = "")
    {
        if (!isRecording)
        {   
            string indexNumber = filePrefix + "" +
                                 cameraIndex + "." + cameraFOVIndex + "." +
                                 (cameraControllers[cameraIndex].enabled? 1:0) +
                                 "; " +
                                 taskIndex + "." + levelIndex;
            dataRecorder.StartRecording(indexNumber);
            uIManager.recordIconImage.SetActive(true);
        }
        else
        {
            dataRecorder.StopRecording();
            uIManager.recordIconImage.SetActive(false);
        }

        isRecording = !isRecording;
    }

    // Other
    public float GetRobotSpeed()
    {
        return wheelController.speed;
    }
    public void Quit()
    {
        Application.Quit();
    }
}