using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class ExperimentManager : MonoBehaviour
{   
    // UI
    public UIManager uIManager;

    // Robot
    private GopherManager[] gopherManagers; // Future work - Multi-robot
    public GopherManager gopherManager;
    public GameObject robot;
    // data
    private bool isRecording = false;

    // Scene
    public string mainScene;
    public string[] tasks;
    public GameObject[] levelPrefabs;
    public GameObject goalPrefab;
    private float goalDectionRadius = 1.5f;
    // robot
    private Vector3[] spawnPositions;
    private Vector3[] spawnRotations;
    private Vector3[] goalPositions;
    private Vector3 currentGoalPosition;
    //human
    public GameObject humanModelPrefab;
    private Vector3[,] humanSpawnPose;
    private float[,] humanActionDectionRange;
    private Vector3[,] humanTrajectory;
    // object
    public GameObject[] numberBoardPrefab;
    private float[,,] numberBoardXRanges;
    private float[,,] numberBoardZRanges;
    private float[,] numberBoardYRotation;
    private int wallNumberSum;

    // Experiment
    public bool isExperimenting;
    private bool trialStarted;
    public int levelIndex;
    public int taskIndex;
    // predefined varible
    private int[,] cameraConfigurations;
    private bool[] onSurvey;
    // generated experiment order
    public int experimentIndex;
    private int experimentLength;
    public int[] taskIndices;
    public int[] levelIndices;
    public int[] trialIndices;
    public int[] cameraIndices;
    // experiment condition
    private int[] runTask;
    private int[] runLevel;
    private int[] runTrial;
    private int[] runCamera;

    
    void Start()
    {
        uIManager.SetExperimentManager(this);

        // Task
        mainScene = "Hospital";
        tasks = new string[] {"Following", "Corridor", "Turning", "Door", "Searching"};
        
        // Experiment Scene Variable Initialization
        // Robot, spawn and goal pose
        spawnPositions = new Vector3[]
                            {new Vector3(5.0f, 0.0f, 6.5f), new Vector3(6.7f, 0.0f, -11.0f), new Vector3(-16.6f, 0.0f, -12.5f), 
                             new Vector3(-6.7f, 0.0f, -11.5f), new Vector3(-4.5f, 0.0f, -16.0f), new Vector3(-7.5f, 0.0f, -19.5f), 
                             new Vector3(-11.2f, 0f, -9.3f), new Vector3(-11.2f, 0f, -9.3f)};
        spawnRotations = new Vector3[]
                            {new Vector3(0f, 180f, 0f), new Vector3(0f, -90f, 0f), new Vector3(0f, 90f, 0f), 
                             new Vector3(0f, 180f, 0f), new Vector3(0f, 90f, 0f), new Vector3(0f, 180f, 0f), 
                             new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)};
        goalPositions = new Vector3[]
                            {new Vector3(6.0f, 0.0f, -10.0f), new Vector3(-16.0f, 0.0f, -12.0f), new Vector3(-7.0f, 0.0f, -6.0f), 
                             new Vector3(-8.0f, 0.0f, -20.0f), new Vector3(-12.0f, 0.0f, -17.0f), new Vector3(-12.0f, 0.0f, -17.0f), 
                             new Vector3(-3.0f, 0f, -17.0f), new Vector3(-8.5f, 0f, -1.5f), new Vector3(-16.0f, 0f, -12.0f), 
                             new Vector3(-8.0f, 0f, -3.0f)};
        // Human, spwan pose and trajectory
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
        // Object, spawn pose - random spawn limit
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

        // Experiment flag and index
        isExperimenting = false;
        levelIndex = 0;
        taskIndex = 0;
        experimentIndex = 0;
        cameraConfigurations = new int [,] {{1, 1, 1}, {0, 1, 1}, {1, 0, 1}, {1, 1, 0}}; 
        onSurvey = new bool[] {false, false};

        // Temp - Simulate camera stream rate
        // Should be replaced by GopherManager.CameraRender()
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }

    void Update()
    {
        if (!isExperimenting)
        {
            // Change camera configuration - only avaliable when not experimenting
            if (Input.GetKeyDown(KeyCode.Tab)) 
                ChangeCameraView();
            if (Input.GetKeyDown(KeyCode.V))
                ChangeCameraFOV();
            if (Input.GetKeyDown(KeyCode.LeftControl))
                ChangeCameraMobility();
            if  (Input.GetKeyDown(KeyCode.R))
                Record();
        }
        else
        {
            // Check if user start to move the robot
            if (!trialStarted &&
               (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) )
                    trialStarted = true;
        }

        // Wheel
        if (Input.GetKeyDown(KeyCode.LeftShift) || 
            Input.GetKeyDown(KeyCode.RightShift) )
            ChangeRobotSpeed();
    }


    /********** Robot **********/
    // Camera - view point
    public void ChangeCameraView()
    {
        if (robot == null)
            return;
        gopherManager.ChangeCameraView();
    }

    // Camera - FOV
    public void ChangeCameraFOV()
    {
        if (robot == null)
            return;
        gopherManager.ChangeCameraFOV();

        // Reload UI
        uIManager.LoadRobotUI();
    }

    // Camera - mobility
    public void ChangeCameraMobility()
    {
        if (robot == null)
            return;
        gopherManager.ChangeCameraMobility();
    }
 
    // Wheel
    public void ChangeRobotSpeed()
    {
        if (robot == null)
            return;
        gopherManager.ChangeRobotSpeed();
    }

    public float GetRobotSpeed()
    {
        return gopherManager.GetRobotSpeed();
    }

    // Data
    public void Record(string filePrefix = "")
    {
        // Start or stop recording 
        string indexNumber = filePrefix + 
                            " " +
                            gopherManager.cameraIndex + "." + 
                            gopherManager.cameraFOVIndex + "." +
                            (gopherManager.cameraMobility? 1:0) +
                            "; " +
                            taskIndex + "." + levelIndex + 
                            " " + 
                            System.DateTime.Now.ToString("MM-dd HH-mm-ss");
        gopherManager.Record(indexNumber);
        // Record Icon
        uIManager.recordIconImage.SetActive(gopherManager.isRecording);
    }


    /********** Scene **********/
    public void LoadSceneWithRobot(int taskIndex, int levelIndex,
                                   int cameraIndex=0, bool cameraMobility=false, int cameraFOVIndex=0)
    {   
        // Load scene with given level, task and camera configuration
        // Load task
        this.taskIndex = taskIndex;
        this.levelIndex = levelIndex;
        
        // Load camera configuration
        gopherManager.cameraIndex = cameraIndex;
        gopherManager.cameraMobility = cameraMobility;
        gopherManager.cameraFOVIndex = cameraFOVIndex;
        
        // Keep the experiment and UI manager
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(uIManager);
        // Load scene
        StartCoroutine(LoadSceneWithRobotCoroutine());
    }
    private IEnumerator LoadSceneWithRobotCoroutine()
    {
        // MainScene
        SceneManager.LoadScene(mainScene);
        yield return new WaitForSeconds(0.5f);

        // Mevel
        Instantiate(levelPrefabs[levelIndex], new Vector3(), new Quaternion());
        // Generate human or other object
        GenerateHumanModel();
        if (taskIndex == 4)
            GenerateNumberBoard();
        yield return new WaitForSeconds(0.5f); 

        // Robot
        SpawnRobot();
    }

    public void ReloadScene()
    {
        LoadSceneWithRobot(taskIndex, levelIndex, 
                           gopherManager.cameraIndex, 
                           gopherManager.cameraMobility,
                           gopherManager.cameraFOVIndex);
    }


    /********** Task **********/
    private void GenerateHumanModel()
    {
        // level 0-2
        // The followed human for Human following
        if (levelIndex != 3 && taskIndex == 0)
        {
            GameObject taskNurse = Instantiate(humanModelPrefab, 
                                                humanSpawnPose[0, 0], 
                                                Quaternion.Euler(humanSpawnPose[0, 1]));
            StartCoroutine(CharacterMoveOnAction(taskNurse, 0, GetRow<Vector3>(humanTrajectory, 0)));
        }
        // level 2
        // Extra human as dynamic obstacles
        if (levelIndex == 2 && taskIndex != 4)
        {
            GameObject levelNurse = Instantiate(humanModelPrefab, 
                                                humanSpawnPose[taskIndex+1, 0], 
                                                Quaternion.Euler(humanSpawnPose[taskIndex+1, 1]));
            StartCoroutine(CharacterMoveOnAction(levelNurse, taskIndex+1, GetRow<Vector3>(humanTrajectory, taskIndex+1)));
        }
        // level 3
        // Different trajectories compared to preivous level
        if (levelIndex == 3 && taskIndex != 4)
        {
            // followed human, different from previous level
            if (taskIndex == 0)
            {
                Vector3[] humanTrajectory30 = new Vector3[]
                                            {new Vector3(5.6f, 0f, -0.3f), new Vector3(5f, 0f, -1.1f), 
                                             new Vector3(3.3f, 0f, -1.1f), new Vector3(0.5f, 0f, -1.9f),
                                             new Vector3(-1.8f, 0f, -1.1f), new Vector3(-6.4f, 0f, -1.4f),
                                             new Vector3(-10.4f, 0f, -1.4f)};
                GameObject taskNurse = Instantiate(humanModelPrefab, 
                                                    humanSpawnPose[0, 0], 
                                                    Quaternion.Euler(humanSpawnPose[0, 1]));
                CharacterWalk characterWalk = taskNurse.GetComponent<CharacterWalk>();
                StartCoroutine(CharacterMoveOnAction(taskNurse, 0, humanTrajectory30));
            }
            // extra human as dynamic obstacles, same as previous level
            if (taskIndex == 0 && taskIndex == 1)
            {
                GameObject levelNurse = Instantiate(humanModelPrefab,
                                                    humanSpawnPose[taskIndex + 1, 0],
                                                    Quaternion.Euler(humanSpawnPose[taskIndex + 1, 1]));
                StartCoroutine(CharacterMoveOnAction(levelNurse, taskIndex+1, GetRow<Vector3>(humanTrajectory, taskIndex+1)));
            }
            // extra human as dynamic obstacles, different from  previous level
            int ii = (Random.Range(0f, 1f) > 0.5f)? 0:2; // randomize trajectories
            if (taskIndex == 1 || taskIndex == 2)
            {
                Vector3[] humanTrajectory31 = new Vector3[]
                            {new Vector3(-7.4f, 0f, -16f), new Vector3(-7.4f, 0f, -11.6f), 
                             new Vector3(-7.4f, 0f, -6.8f)};
                GameObject loopNurse = Instantiate(humanModelPrefab,
                                                   humanTrajectory31[ii],
                                                   new Quaternion());
                StartCoroutine(CharacterMoveWait(loopNurse, humanTrajectory31));
            }
            else if (taskIndex == 3)
            {
                Vector3[] humanTrajectory33 = new Vector3[]
                            {new Vector3(-12.4f, 0f, -16.3f), new Vector3(-7.4f, 0f, -16.3f), 
                             new Vector3(-2.4f, 0f, -16.3f)};
                GameObject loopNurse = Instantiate(humanModelPrefab,
                                                   humanTrajectory33[ii],
                                                   new Quaternion());
                StartCoroutine(CharacterMoveWait(loopNurse, humanTrajectory33));
            }
        }
    }
    private IEnumerator CharacterMoveWait(GameObject character, Vector3[] trajectory)
    {
        if (character == null)
            yield break;

        // Wait for a random time to start
        yield return new WaitForSeconds(Random.Range(1f, 3f));

        // Set trajectory
        CharacterWalk characterWalk = character.GetComponent<CharacterWalk>();
        if (characterWalk != null)
        {
            characterWalk.loop = true;
            characterWalk.MoveTrajectory(trajectory);
        }
    }
    private IEnumerator CharacterMoveOnAction(GameObject character, int index, Vector3[] trajectory)
    {
        if (character == null)
            yield break; 
            
        // Wait for the robot to start
        float xLower = humanActionDectionRange[index, 0];
        float xUpper = humanActionDectionRange[index, 1];
        float zLower = humanActionDectionRange[index, 2];
        float zUpper = humanActionDectionRange[index, 3];
        yield return new WaitUntil(() => RobotInArea(xLower, xUpper, zLower, zUpper) == true);

        // Set trajectory
        CharacterWalk characterWalk = character.GetComponent<CharacterWalk>();
        if (characterWalk != null)
        {
            characterWalk.loop = true;
            characterWalk.MoveTrajectory(trajectory);
        }
    }
    private bool RobotInArea(float xLower, float xUpper, float zLower, float zUpper)
    {
        // Check if robot reaches a rectange area
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
        // Randomly select which walls to put number boards
        int[] wallIndices;
        if (levelIndex == 0)
            wallIndices = new int[] {0, 1, 2};
        else if (levelIndex == 1)
            wallIndices = new int[] {0, 1, 2, 3};
        else
            wallIndices = new int[] {0, 1, 2, 3, 4};

        System.Random randomInt = new System.Random();
        wallIndices = wallIndices.OrderBy(x => randomInt.Next()).ToArray();
        
        // Generate number board and keep the number sum
        wallNumberSum = 0;
        float locationX;
        float locationZ;
        // # of board = level
        for (int i = 0; i < levelIndex+1; ++i)
        {
            // randomly select a number
            int number = randomInt.Next(numberBoardPrefab.Length);
            wallNumberSum += number;

            // randomly generate the board on the selected wall
            int wallIndex = wallIndices[i];
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
    private void CheckNumberBoardAnswer()
    {
        uIManager.LoadQuitScene();

        // Get input answer
        int answer = uIManager.GetNumberBoardAnswer();
        if (answer == wallNumberSum)
        {
            if (!isExperimenting)
            {
                uIManager.PopMessage("Correct!");
            }
            else
            {
                // Level up
                if(isRecording)
                    Record();
                uIManager.LoadNextLevelUI();
            }
        }
        else
        {
            uIManager.PopMessage("Please try again");
        }
    }

    private void SpawnRobot()
    {
        // Spawn robot
        int spawnIndex = taskIndex;
        if (taskIndex == 4)
            spawnIndex += levelIndex;
        if (levelIndex != 3)
            gopherManager.SpawnRobot(0, spawnPositions[spawnIndex], spawnRotations[spawnIndex]);
        else
            gopherManager.SpawnRobot(1, spawnPositions[spawnIndex], spawnRotations[spawnIndex]);

        // Set goal
        currentGoalPosition = goalPositions[taskIndex];
        if (taskIndex == 4) // No goal for searching task
            currentGoalPosition = new Vector3(0f, -10f, 0f);
        else if (taskIndex == 3)
            currentGoalPosition = goalPositions[taskIndex + levelIndex];
        else if (levelIndex == 3)
            currentGoalPosition = goalPositions[taskIndex + 7];
        StopCoroutine(CheckGoalCoroutine());
        StartCoroutine(CheckGoalCoroutine());
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
                // Level up
                if(isRecording)
                    Record();
                uIManager.LoadNextLevelUI();
            }
            return true;
        }
        else
            return false;
    }


    /********** Experiment **********/
    public void StartExperiment()
    {
        // Start experiment
        uIManager.StartExperiment();

        // Load experiment conditions
        (runTask, runLevel, runTrial, runCamera) = uIManager.GetExperimentCondition();
        // Randomly generate experiment order
        CreateIndicesArray();

        // If start with training phase
        if (levelIndices[0] == 0)
            experimentIndex = runCamera.Length * runTask.Length;
        else
            experimentIndex = 0;

        isExperimenting = true;
        LoadLevel();
    }
    
    private void LoadLevel(int index=-1)
    {
        if (index == -1)
            index = experimentIndex;
        
        // UI
        uIManager.loadLevel();

        // Load scene
        int cameraConfigIndex = cameraIndices[index];
        LoadSceneWithRobot(taskIndices[index], 
                           levelIndices[index],
                           cameraConfigurations[cameraConfigIndex, 0],
                           (cameraConfigurations[cameraConfigIndex, 2] == 1)? true:false,
                           cameraConfigurations[cameraConfigIndex, 1]);
        
        // Stop previous recording
        StopCoroutine(StartRecordOnAction());
        if (isRecording)
            Record();
        // Record
        trialStarted = false;
        StartCoroutine(StartRecordOnAction());
    }
    private IEnumerator StartRecordOnAction()
    {
        yield return new WaitUntil(() => trialStarted == true);
        // if start with training phase
        if (levelIndices[0] == 0)
            Record((experimentIndex-(runCamera.Length * runTask.Length)).ToString() + 
                                "- " + trialIndices[experimentIndex].ToString() + ";");
        else
            Record((experimentIndex).ToString() + 
                                "- " + trialIndices[experimentIndex].ToString() + ";");
    }

    public void ReloadLevel()
    {
        if (isExperimenting)
            LoadLevel();
        else
            ReloadScene();
    }

    public void LoadNextLevel()
    {
        uIManager.LoadNextLevel();
        NextLevel();
    }

    private void NextLevel()
    {
        experimentIndex += 1;

        // Load next level
        if (experimentIndex != experimentLength)
        {
            // Survey
            int levelLength = runCamera.Length * runTask.Length * runTrial.Length;
            if ((trialIndices[experimentIndex-1] == 1) && (levelIndices[experimentIndex-1] != 0) &&
                ((taskIndices[experimentIndex-1] != taskIndices[experimentIndex]) ||
                 (levelIndices[experimentIndex-1] != levelIndices[experimentIndex])) )
            {
                StartCoroutine(SurveyCoroutine(1));
            }
            if ((trialIndices[experimentIndex-1] == 1) && (levelIndices[experimentIndex-1] != 0) &&
                (experimentIndex % levelLength == 0) )
            {
                StartCoroutine(SurveyCoroutine(0));
            }

            // Next configuration
            LoadLevel();
        }
        else
        {
            StartCoroutine(SurveyCoroutine(1));
            StartCoroutine(SurveyCoroutine(0));
        }
    }
    private IEnumerator SurveyCoroutine(int type)
    {
        uIManager.LoadSurvey(type);
        onSurvey[type] = true;

        yield return new WaitUntil(()=> onSurvey[type] == false);
    }
    public void FinishSurvey(int type)
    {
        uIManager.LoadSurvey(type);
        onSurvey[type] = false;
    }

    private void CreateIndicesArray()
    {
        // Randomly generate the experiment order
        // based on the experiment condition
        System.Random random = new System.Random();
    
        // Intialize indices array
        experimentLength = runCamera.Length * runTask.Length * 
                           runLevel.Length * runTrial.Length;
        cameraIndices = new int[experimentLength];
        taskIndices = new int[experimentLength];
        levelIndices = new int[experimentLength];
        trialIndices = new int[experimentLength];

        // Extend
        int count;
        // level
        count = 0;
        for (int l = 0; l < runLevel.Length; ++l)
            for (int s = 0; s < runTrial.Length * runCamera.Length * runTask.Length; ++s)
            {
                levelIndices[count] = runLevel[l];
                count += 1;
            }
        // trial
        count = 0;
        for (int l = 0; l < runLevel.Length; ++l)
            for (int tr = 0; tr < runTrial.Length; ++tr)
                for (int s = 0; s < runCamera.Length * runTask.Length; ++s)
                {
                    trialIndices[count] = runTrial[tr];
                    count += 1;
                }
        // task
        count = 0;
        for (int l = 0; l < runLevel.Length; ++l)
            for (int tr = 0; tr < runTrial.Length; ++tr)
            {
                int[] randomRunTask = runTask.OrderBy(x => random.Next()).ToArray();
                for (int ta = 0; ta < runTask.Length; ++ta)
                    for (int s = 0; s < runCamera.Length; ++s)
                    {
                        taskIndices[count] = randomRunTask[ta];
                        count += 1;
                    }
            }
        // camera
        count = 0;
        for (int l = 0; l < runLevel.Length; ++l)
            for (int tr = 0; tr < runTrial.Length; ++tr)
                for (int ta = 0; ta < runTask.Length; ++ta)
                {
                    int[] randomRunCamera = runCamera.OrderBy(x => random.Next()).ToArray();
                    for (int s = 0; s < runCamera.Length; ++s)
                    {
                        cameraIndices[count] = randomRunCamera[s];
                        count += 1;
                    }
                }

        experimentIndex = 0;
    }


    /********** Others **********/
    // System
    public void Quit()
    {
        Application.Quit();
    }

    // Helper functions
    private T[] GetRow<T>(T[,] Array2D, int index)
    {
        T[] row = new T[Array2D.GetLength(1)];
        for (int i = 0; i < row.Length; ++i)
            row[i] = Array2D[index, i];
        return row;
    }
}