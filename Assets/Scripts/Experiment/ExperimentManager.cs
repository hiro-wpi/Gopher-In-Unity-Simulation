using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class ExperimentManager : MonoBehaviour
{   
    // Experiment
    public Experiment[] experiments;
    public FreePlayTask freePlayTask;
    private int totalTaskIndex;
    private int experimentIndex;
    private int taskIndex;
    // randomization
    public bool randomizeTasks;
    private int totalNumTask;
    private int[] cumulativeSumNumTask;
    private System.Random randomInt = new System.Random();
    private int[] randomizedIndices;
    // Task
    public bool experimentStarted;
    private bool taskStarted;
    private bool taskLoaded;
    private Task currentTask;
    // whether using the same scene and robot during the whole experiment
    public bool keepSameSceneAndRobot;
    private GameObject[] spawnedRobots;

    // UIs
    public ExperimentMenus experimentMenus;
    private GraphicalInterface gUI;
    private ControlInterface cUI;
    private CursorLockMode previousCursorState;

    // Data
    public DataRecorder dataRecorder;
    private string recordFolder;

    // Nav Mesh Agent for dynamic 
    public NavMeshSurface navMeshHuman;
    public NavMeshSurface navMeshRobot;  

    void Start()
    {
        // Start the main menus
        experimentStarted = false;
        LoadMainMenus();
        // experiments
        totalTaskIndex = 0;
        totalNumTask = 0;
        cumulativeSumNumTask = new int[experiments.Length];
    }

    void Update()
    {
        // Simulation not started
        if (!experimentStarted)
            return;

        // Hotkeys
        if (Input.GetKeyDown(KeyCode.Escape))
            LoadQuitMenus();

        // Check if user starts to move the robot
        if (currentTask != null && 
            taskLoaded && !taskStarted && 
            currentTask.CheckTaskStart())
        {
            // start
            taskStarted = true;
            StartRecording();
            // check current task status until completion every 1s
            InvokeRepeating("CheckTaskCompletion", 0f, 0.5f);
        }
    }
    private void CheckTaskCompletion() 
    {
        if (currentTask.CheckTaskCompletion())
        {
            // stop
            CancelInvoke("CheckTaskCompletion");
            StopRecording();
            // load new task
            bool success = LoadNewTask();
            // all tasks loaded
            if (!success)
                experimentMenus.LoadExperimentCompleted();
        }
    }


    // EXPERIMENT OR FREEPLAY //
    // Start the experiment
    public void StartExperiment()
    {
        // Randomize task orders
        // count total task number
        for (int i=0; i<experiments.Length; ++i)
        {
            Experiment experiment = experiments[i];
            // experiment.RandomizeTasks(); // handled in this script now
            totalNumTask += experiment.GetNumTasks();

            cumulativeSumNumTask[i] = experiment.GetNumTasks();
            if (i != 0)
                cumulativeSumNumTask[i] += cumulativeSumNumTask[i-1];
        }
        // randomize index
        randomizedIndices = Enumerable.Range(0, totalNumTask).ToArray();
        if (randomizeTasks)
            randomizedIndices = Utils.Shuffle(randomInt, randomizedIndices);

        // Load the first task to starts
        bool success = LoadNewTask();
        if (!success)
            experimentMenus.LoadExperimentCompleted();
        experimentStarted = true;

        // Initialize data recorder
        if (dataRecorder == null)
            dataRecorder = gameObject.AddComponent<DataRecorder>();
        recordFolder = Application.dataPath + "/Data" + "/" + 
                       System.DateTime.Now.ToString("MM-dd HH-mm-ss");
        if (!Directory.Exists(recordFolder))
            Directory.CreateDirectory(recordFolder);
    }

    // Free play mode
    public void FreePlay()
    {   
        // Load free play task
        currentTask = freePlayTask;
        experimentMenus.LoadLoading();
        StartCoroutine(LoadTaskCoroutine());
        // Flag
        experimentStarted = true;
        taskStarted = false;
    }

    
    // RECORDER //
    // Start or stop recording
    public void StartRecording()
    {
        if (!dataRecorder.isRecording)
        {
            string fileName = recordFolder + "/" + currentTask.taskName;
            dataRecorder.StartRecording(fileName, spawnedRobots, currentTask);
        }
        else
            return;
        
        // show icon
        gUI.SetRecordIconActive(true);
    }
    public void StopRecording()
    {
        if (!dataRecorder.isRecording)
            return;
        else
            dataRecorder.StopRecording();
        
        // show icon
        gUI.SetRecordIconActive(false);
    }


    // TASK //
    // Load new task
    public bool LoadNewTask()
    {
        if (totalTaskIndex == totalNumTask)
            return false;
        
        // Randomly get the next task index
        int randomizedIndex = randomizedIndices[totalTaskIndex];
        totalTaskIndex++;
        // Compute experiment index and task index
        for (int i = 0; i < cumulativeSumNumTask.Length; ++i)
        {
            if (randomizedIndex < cumulativeSumNumTask[i])
            {
                experimentIndex = i;
                taskIndex = randomizedIndex;
                if (i != 0)
                    taskIndex = randomizedIndex - cumulativeSumNumTask[i-1];
            }
        }

        // Load loading UI
        if (!keepSameSceneAndRobot || !experimentStarted )
            experimentMenus.LoadLoading();
        
        // Destroy previous task
        if (currentTask != null)
        {
            currentTask.DestroyObjects(true, false, false, true, 0f);
            currentTask.DestroyObjects(false, true, true, false, 7f);
            Destroy(currentTask);
        }
        // Get a new task
        taskLoaded = false;
        currentTask = experiments[experimentIndex].GetTask(taskIndex);
        
        // Load new scene and 
        // other game objects (objects, humans, robots, etc.)
        Debug.Log("Task " + currentTask.taskName + " is loading.");
        StartCoroutine(LoadTaskCoroutine());
        
        // Task flag
        taskStarted = false;
        return true;
    }
    private IEnumerator LoadTaskCoroutine()
    {
        Time.timeScale = 1f;
        // Reload scene, robot and objects for every task
        if (!keepSameSceneAndRobot)
        {
            DontDestroyOnLoad(this.gameObject);

            // MainScene
            SceneManager.LoadScene(currentTask.sceneName);
            yield return new WaitForSeconds(0.3f);

            // Generate robots
            currentTask.GenerateRobots();
            yield return new WaitForSeconds(0.3f); 

            // Generate static objects
            currentTask.GenerateStaticObjects();
            yield return new WaitForSeconds(0.1f);
            // Remake nav mesh
            navMeshRobot.BuildNavMesh();
            yield return new WaitForSeconds(0.4f);

            // Generate task and goal objects
            currentTask.GenerateTaskObjects();
            currentTask.GenerateGoalObjects();
            yield return new WaitForSeconds(0.1f);
            // Remake nav mesh
            navMeshHuman.BuildNavMesh();
            yield return new WaitForSeconds(0.4f);

            // Generate dynamic objects
            currentTask.GenerateDynamicObjects();
        }
        //Or Keep the same scene and robot for every task
        else
        {
            // First generation
            if (!experimentStarted)
            {
                DontDestroyOnLoad(this.gameObject);

                // MainScene
                SceneManager.LoadScene(currentTask.sceneName);
                yield return new WaitForSeconds(0.3f);
                // Generate robots
                spawnedRobots = currentTask.GenerateRobots();
                yield return new WaitForSeconds(0.3f);
            }
            // Other than the first time
            else
            {
                currentTask.DestroyObjects();
                // Set up the existing robot
                currentTask.SetRobots(spawnedRobots);
            }

            // Generate static objects
            currentTask.GenerateStaticObjects();
            yield return new WaitForSeconds(0.1f);
            // Remake nav mesh
            navMeshRobot.BuildNavMesh();
            yield return new WaitForSeconds(0.4f);

            // Generate task and goal objects
            currentTask.GenerateTaskObjects();
            currentTask.GenerateGoalObjects();
            yield return new WaitForSeconds(0.1f);
            // Remake nav mesh
            navMeshHuman.BuildNavMesh();
            yield return new WaitForSeconds(0.4f);

            // Generate dynamic objects
            currentTask.GenerateDynamicObjects();
        }

        // UI
        gUI = currentTask.gUI;
        gUI.SetUIActive(true);
        // TODO cUI = currentTask.CUI;

        taskLoaded = true;
    }

    // Reload current task
    public void ReloadTask()
    {
        // Stop recording
        if (dataRecorder.isRecording)
            StopRecording();
        
        // Reload currentTask
        // Ensure time is running and load loading UI
        ResumeFromQuitMenus();
        if (!keepSameSceneAndRobot)
            experimentMenus.LoadLoading();
        
        // Temp
        Debug.Log("Reload task " + currentTask.taskName);
        // Load scene and 
        // other game objects (objects, humans, robots, etc.)
        StartCoroutine(LoadTaskCoroutine());
        currentTask.ResetTaskStatus();
        taskStarted = false;
    }

    // Respawn robot
    public void RespawnRobot()
    {
        ResumeFromQuitMenus();
        experimentMenus.LoadLoading(2f);
        StartCoroutine(RespawnRobotCoroutine());
    }
    private IEnumerator RespawnRobotCoroutine()
    {
        // Destory existing robot
        foreach(GameObject robot in spawnedRobots)
            Destroy(robot);
        yield return new WaitForSeconds(0.5f);
        
        // Generate robots
        spawnedRobots = currentTask.GenerateRobots();
        // Set up the existing robot
        currentTask.SetRobots(spawnedRobots);
    }


    // MENUS //
    // Load quit menus
    public void LoadQuitMenus()
    {
        // Stop the time and save the mouse status
        Time.timeScale = 0f;
        previousCursorState = Cursor.lockState;
        Cursor.lockState = CursorLockMode.Confined;
        experimentMenus.LoadQuitMenus();
    }

    // Resume from quit menus
    public void ResumeFromQuitMenus()
    {
        // Resume the time and mouse
        Time.timeScale = 1f;
        Cursor.lockState = previousCursorState;
        experimentMenus.HideAll();
    }

    // Load main menus
    public void LoadMainMenus()
    {
        // Stop the time
        Time.timeScale = 0f;
        experimentMenus.LoadMainMenus();
        experimentStarted = false;
    }


    // SYSTEM //
    // Quit game
    public void Quit()
    {
        Application.Quit();
    }
}