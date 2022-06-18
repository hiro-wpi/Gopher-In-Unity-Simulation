using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperimentManager : MonoBehaviour
{   
    // Experiment
    public Experiment experiment;
    private Task task;
    private int taskIndex;
    private int numTasks;
    private bool taskStarted;

    // UIs
    public ExperimentMenus experimentMenus;
    private UserInterface uI;
    private GraphicalInterface gUI;
    private ControlInterface cUI;
    private CursorLockMode previousCursorState;

    // Data
    private DataRecorder dataRecorder;
    private string recordFolder;


    void Start()
    {
        // Start the menus
        LoadMainMenus();

        // Temp - Simulate camera stream rate
        // Should be replaced by cam.CameraRender() in the future
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }

    void Update() 
    {
        // Simulation not started
        if (uI == null)
            return;

        // Hotkeys
        if (Input.GetKeyDown(KeyCode.Escape))
            LoadQuitMenus();
    }

    void FixedUpdate()
    {
        // Simulation not started
        if (uI == null)
            return;
        
        // Check if user starts to move the robot
        if (!taskStarted &&
            (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
             Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ||
             Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
             Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) 
           )
           {
               taskStarted = true;
               Record();
           }
        // Check the status of the current task
        else
        {
            if(task.CheckTaskCompletion() == 1)
            {
                taskIndex += 1;
                LoadTask(taskIndex);
                Record();
            }
        }
    }
    

    // Start experiment
    public void StartExperiment()
    {
        numTasks = experiment.GetNumTasks();
        experiment.RandomizeTasks();

        // Load the first task to starts
        taskIndex = 0;
        LoadTask(taskIndex);

        // Interface
        uI = task.UI;
        gUI = uI.GUI;
        cUI = uI.CUI;

        // Initialize recorder
        dataRecorder = gameObject.AddComponent<DataRecorder>();
        recordFolder = Application.dataPath + "/Data" + "/" + 
                       System.DateTime.Now.ToString("MM-dd HH-mm-ss");
        if (!Directory.Exists(recordFolder))
            Directory.CreateDirectory(recordFolder);
    }


    // Start or stop recording
    public void Record()
    {
        if (!dataRecorder.IsRecording)
        {
            // start
            string fileName = recordFolder + "/" + task.TaskName;
            dataRecorder.StartRecording(fileName, task);
        }
        else
        {
            // stop
            dataRecorder.StopRecording();
        }
        // show icon
        gUI.SetRecordIconActive(dataRecorder.IsRecording);
    }


    // Load the given task in the scene
    public void LoadTask(int taskIndex)
    {
        // Ensure time is running
        Time.timeScale = 1f;
        experimentMenus.LoadLoading();

        // Load current task
        task = experiment.GetTask(taskIndex);
        Debug.Log("Task " + task.TaskName + " is loading.");
        // Load scene and other game objects (objects, humans, robots, etc.)
        LoadScene(task.SceneName);
        
        taskStarted = false;
    }

    // Functions to load scene
    private void LoadScene(string sceneName)
    {
        // Keep the experiment manager and all children
        DontDestroyOnLoad(this.gameObject);
        // Load scene
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // MainScene
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(0.5f);

        // Generate objects
        task.GenerateObjects();
        yield return new WaitForSeconds(0.5f); 

        // Generate robots
        task.GenerateRobots();
    }


    // Load quit menus
    public void LoadQuitMenus()
    {
        // Stop the time and save the mouse status
        Time.timeScale = 0f;
        previousCursorState = Cursor.lockState;
        Cursor.lockState = CursorLockMode.Confined;
        experimentMenus.LoadMainMenus();
    }

    // Resume from quit menus
    public void ResumeFromQuitMenus()
    {
        // Resume the time and mouse
        Time.timeScale = 1f;
        Cursor.lockState = previousCursorState;
        experimentMenus.HideAll();
    }

    // Reload the current task
    public void ReloadTask()
    {
        // Stop recording
        if (dataRecorder.IsRecording)
            Record();
        // Reload task
        LoadTask(taskIndex);
    }

    // Go back to main menus
    public void LoadMainMenus()
    {
        // Stop the time
        Time.timeScale = 0f;
        // TODO Destroy all other game objects
        experimentMenus.LoadMainMenus();
    }

    // Quit game
    public void Quit()
    {
        Application.Quit();
    }
}