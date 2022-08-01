using System.IO;
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
    private int[] taskIndices;
    private bool experimentStarted;
    private bool taskStarted;
    private System.Random randomInt = new System.Random();
    private Task currentTask;
    // whether using the same scene and robot during the whole experiment
    public bool keepSameSceneAndRobot;
    private GameObject[] spawnedRobots;
    private GameObject[] spawnedStaticObjects;
    private GameObject[] spawnedDynamicObjects;

    // UIs
    public ExperimentMenus experimentMenus;
    private GraphicalInterface gUI;
    private ControlInterface cUI;
    private CursorLockMode previousCursorState;

    // Data
    public DataRecorder dataRecorder;
    private string recordFolder;

    // Nav Mesh Agent for dynamic 
    public NavMeshSurface navMeshSurface;

    void Start()
    {
        // Start the main menus
        LoadMainMenus();
        // experiments
        taskIndices = new int[experiments.Length];
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
        if (currentTask != null && !taskStarted && currentTask.CheckTaskStart())
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


    // Start the experiment
    public void StartExperiment()
    {
        for (int i=0; i<experiments.Length; ++i)
        {
            Experiment experiment = experiments[i];
            experiment.RandomizeTasks();
            taskIndices[i] = experiment.GetNumTasks() - 1;
        }

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
        currentTask = freePlayTask;
        StartCoroutine(LoadTaskCoroutine(true));
        experimentStarted = true;
        taskStarted = false;
    }

    
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


    // Task
    // Load new task
    public bool LoadNewTask()
    {
        // Randomly get the next task from unfinished experiment
        int experimentIndex = GetNextExperimentIndex();
        if (experimentIndex == -1)
            return false; // all experiments are done

        // Update the task index and
        // get update task index of selected experiment
        int taskInedex = taskIndices[experimentIndex];
        taskIndices[experimentIndex] -= 1;

        // Ensure time is running and
        // load loading UI
        Time.timeScale = 1f;
        if (!keepSameSceneAndRobot || !experimentStarted )
            experimentMenus.LoadLoading();
        
        // Destroy previous task and get a new task
        Destroy(currentTask);
        currentTask = experiments[experimentIndex].GetTask(taskInedex);
        if (currentTask == null)
            return false;
        
        // Temp
        Debug.Log("Task " + currentTask.taskName + " is loading.");
        // Load scene and 
        // other game objects (objects, humans, robots, etc.)
        StartCoroutine(LoadTaskCoroutine(experimentStarted==false));
        
        // Task
        taskStarted = false;
        return true;
    }
    private int GetNextExperimentIndex()
    {
        // Check if all experiments are done
        int sum = 0;
        foreach (int i in taskIndices)
            sum += i;
        if (sum == -1 * taskIndices.Length)
            return -1;

        // If not, randomly select the next experiment
        int taskIndex = -1;
        int experimentIndex = 0;
        while (taskIndex == -1)
        {
            // Select
            experimentIndex = randomInt.Next(0, experiments.Length);
            // As long as the task in this experiment is not done
            taskIndex = taskIndices[experimentIndex];
        }
        return experimentIndex;
    }
    private IEnumerator LoadTaskCoroutine(bool isFirstTask = false)
    {
        // Reload scene, robot and objects for every task
        if (!keepSameSceneAndRobot)
        {
            DontDestroyOnLoad(this.gameObject);

            // MainScene
            SceneManager.LoadScene(currentTask.sceneName);
            yield return new WaitForSeconds(0.5f);

            // Generate robots
            currentTask.GenerateRobots();
            yield return new WaitForSeconds(0.5f); 

            // Generate static objects
            spawnedStaticObjects = currentTask.GenerateStaticObjects();
            yield return new WaitForSeconds(0.2f);
            // Remake nav mesh for dynamic objects;
            navMeshSurface.BuildNavMesh();
            yield return new WaitForSeconds(0.8f);

            // Generate dynamic objects
            spawnedDynamicObjects = currentTask.GenerateDynamicObjects();
        }
        //Or Keep the same scene and robot for every task
        else
        {
            // First generation
            if (isFirstTask)
            {
                DontDestroyOnLoad(this.gameObject);

                // MainScene
                SceneManager.LoadScene(currentTask.sceneName);
                yield return new WaitForSeconds(0.5f);
                
                // Generate robots
                spawnedRobots = currentTask.GenerateRobots();
                yield return new WaitForSeconds(0.5f);
            }
            // Other than the first time
            else
            {
                currentTask.DestroyObjects();
                // Set up the existing robot
                currentTask.SetRobots(spawnedRobots);
            }

            // Generate static objects
            spawnedStaticObjects = currentTask.GenerateStaticObjects();
            yield return new WaitForSeconds(0.2f);
            // Rebake nav mesh for dynamic objects;
            navMeshSurface.BuildNavMesh();
            yield return new WaitForSeconds(0.8f);

            // Generate dynamic objects
            spawnedDynamicObjects = currentTask.GenerateDynamicObjects();
        }

        // UI
        gUI = currentTask.gUI;
        gUI.SetUIActive(true);
        // TODO cUI = currentTask.CUI;
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


    // Menus
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


    // System
    // Quit game
    public void Quit()
    {
        Application.Quit();
    }
}