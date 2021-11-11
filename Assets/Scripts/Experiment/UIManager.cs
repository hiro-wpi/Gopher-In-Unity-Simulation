using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Game manager
    public ExperimentManager experimentManager;

    // Screen
    public bool isWideScreen;
    private Vector2[] resolution;
    private Vector2[] wideResolution;

    // UI
    private GameObject[] UIs;
    private int UIIndex;
    // menus
    public GameObject mainMenus;
    public GameObject loadingUI;
    public GameObject quitMenus;
    private CursorLockMode previousCursorState;
    // camera
    public GameObject cameraDisplay;
    private RectTransform cameraDisplayRect;
    public GameObject regularViewUI;
    public GameObject wideViewUI;
    public GameObject experimentUI;
    // task & state
    public GameObject allStateDisplay;
    public GameObject taskStatePanel;
    public GameObject robotStatePanel;
    public GameObject experimentTaskPanel;
    public GameObject experimentStatePanel;
    public GameObject messagePanel;
    private TextMeshProUGUI allStatePanelText;
    private TextMeshProUGUI taskStatePanelText;
    private TextMeshProUGUI robotStatePanelText;
    private TextMeshProUGUI experimentTaskPanelText;
    private TextMeshProUGUI experimentStatePanelText;
    private TextMeshProUGUI messagePanelText;
    
    private string[] taskMessage;

    // Scene
    public GameObject taskDropDown;
    public GameObject levelDropDown;

    // Data
    private GopherDataRecorder dataRecorder;
    public GameObject recordIconImage;
    
    // Others
    public GameObject helpDisplay;
    public GameObject miniMap;
    private int FPS;
    private float FPSSum;
    private int FPSCount;

    // Experiment
    public GameObject levelCompleteUI;
    public GameObject surveyTaskUI;
    public GameObject surveyCameraUI;
    private GameObject[] surveyUI;
    public GameObject experimentConditionUI;
    public GameObject NumberBoardAnswerUI;
    public GameObject NumberBoardAnswerField;
    public GameObject FinishUI;

    public void SetExperimentManager(ExperimentManager experimentManager)
    {
        this.experimentManager = experimentManager;
        dataRecorder = experimentManager.gopherManager.dataRecorder;
    }


    void Start()
    {
        // Screen
        resolution = new Vector2[] {new Vector2 (1463, 823), new Vector2 (1920, 823)};
        wideResolution = new Vector2[] {new Vector2 (1920, 1080), new Vector2 (2560, 1080)};

        // UI
        UIs = new GameObject[] {mainMenus, loadingUI, experimentConditionUI, 
                                experimentUI, wideViewUI, regularViewUI,
                                quitMenus,
                                cameraDisplay, allStateDisplay,
                                levelCompleteUI};

        cameraDisplayRect = cameraDisplay.GetComponent<RectTransform>();
        surveyUI = new GameObject[] {surveyCameraUI, surveyTaskUI};

        // Text to update
        allStatePanelText = allStateDisplay.GetComponentInChildren<TextMeshProUGUI>();
        taskStatePanelText = taskStatePanel.GetComponentInChildren<TextMeshProUGUI>();
        robotStatePanelText = robotStatePanel.GetComponentInChildren<TextMeshProUGUI>();
        experimentTaskPanelText = experimentTaskPanel.GetComponentInChildren<TextMeshProUGUI>();
        experimentStatePanelText = experimentStatePanel.GetComponentInChildren<TextMeshProUGUI>();
        messagePanelText = messagePanel.GetComponentInChildren<TextMeshProUGUI>();

        // Experiment help
        taskMessage = new string[] {
                        "Please follow the nurse until you reach the shining circle.", 
                        "Please go straight and reach the shining circle.",
                        "Please go straight and take a left turn and reach the shining circle.",
                        "Please pass the door, enter the room and reach the shining circle.",
                        "Please find the number boards inside the room and sum the numbers."};
        // Load menus
        LoadMainMenus();

        // Update panel
        InvokeRepeating("UpdateAllStatePenal", 1.0f, 0.1f);
        InvokeRepeating("UpdateRobotUIStatePenal", 1.0f, 0.1f);
        InvokeRepeating("UpdateExperimentPanel", 1.0f, 0.1f);

        // FPS
        FPSCount = 0;
        FPSSum = 0;
        InvokeRepeating("UpdateFPS", 1.0f, 0.25f);
    }

    void Update()
    {
        // Hotkeys
        // info
        if (Input.GetKeyDown(KeyCode.U))
            if (UIIndex != 0 && UIIndex != 1 && UIIndex != 2)
                ChangeAllStateDisplay();
        // miniMap
        if (Input.GetKeyDown(KeyCode.M))
            ChangeMinimapDisplay();

        // system
        if (Input.GetKeyDown(KeyCode.Escape)) 
            if (UIIndex != 0 && UIIndex != 1 && UIIndex != 2)
                LoadQuitScene();
            
        if (Input.GetKeyDown(KeyCode.H)) 
            if (UIIndex != 0 && UIIndex != 1 && UIIndex != 2)
                ChangeHelpDisplay();

        // FPS
        FPSCount += 1;
        FPSSum += 1.0f/Time.deltaTime;
    }
    
    // UIs
    public void LoadMainMenus()
    {
        Time.timeScale = 1f;

        UIIndex = 0;
        foreach (GameObject UI in UIs)
            UI.SetActive(false);

        UIs[UIIndex].SetActive(true);
    }

    private IEnumerator LoadLoading()
    {
        UIIndex = 1;
        foreach (GameObject UI in UIs)
            UI.SetActive(false);

        UIs[UIIndex].SetActive(true);

        yield return new WaitForSeconds(4.0f);
        //yield return new WaitUntil(() => CheckArmPose() == true); //TODO

        LoadRobotUI();
    }
    private bool CheckArmPose() // TODO
    {
        if (experimentManager == null)
        {
            Debug.Log("Game manager doesn't exist!");
            return false;
        }
        
        if (dataRecorder != null && experimentManager.robot != null &&
            Mathf.Abs(dataRecorder.states[14] + 1.57f) < 0.02)
            return true;
        return false;
    }

    public void LoadExperiment()
    {
        UIIndex = 2;
        foreach (GameObject UI in UIs)
            UI.SetActive(false);

        UIs[UIIndex].SetActive(true);
    }

    public void LoadNextLevelUI()
    {
        Time.timeScale = 0.0f;
        Cursor.lockState = CursorLockMode.Confined;

        foreach (GameObject UI in UIs)
            UI.SetActive(false);
        UIs[9].SetActive(true);
    }

    public void LoadNextLevel()
    { 
        Time.timeScale = 1.0f;
        StartCoroutine(LoadLoading());
    }

    public void LoadQuitScene()
    {
        if (UIs[6].activeSelf)
        {
            Cursor.lockState = previousCursorState;
            Time.timeScale = 1f;
        }
        else
        {
            
            Cursor.lockState = Cursor.lockState;
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 0f;

            if (experimentManager != null)
                NumberBoardAnswerUI.SetActive(experimentManager.taskIndex == 4);
        }

        UIs[6].SetActive(!UIs[6].activeSelf);
    }

    public void LoadRobotUI()
    {
        foreach (GameObject UI in UIs)
            UI.SetActive(false);

        cameraDisplay.GetComponent<RawImage>().texture = 
            experimentManager.gopherManager.cameraRenderTextures[experimentManager.gopherManager.cameraFOVIndex];
        cameraDisplay.SetActive(true);
        
        if(experimentManager.isExperimenting)
            UIs[3].SetActive(true);
        else
            UIs[4].SetActive(true);

        Vector2[] displaySize;
        if (isWideScreen)
            displaySize = wideResolution;
        else
            displaySize = resolution;

        if (experimentManager.gopherManager.cameraFOVIndex == 0)
        {
            UIIndex = 5;
            UIs[UIIndex].SetActive(true);
            cameraDisplayRect.sizeDelta = displaySize[0];
        }
        else if (experimentManager.gopherManager.cameraFOVIndex == 1)
        {
            UIIndex = 4;
            cameraDisplayRect.sizeDelta = displaySize[1];
        }
    }

    // Scene
    public void ChangeScene()
    {
        StartCoroutine(LoadLoading());
        
        int taskIndex = taskDropDown.GetComponent<TMP_Dropdown>().value;
        int levelIndex = levelDropDown.GetComponent<TMP_Dropdown>().value;
        experimentManager.LoadSceneWithRobot(taskIndex, levelIndex);
    }

    public void loadLevel()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadLoading());
    }

    // Task
    public int GetNumberBoardAnswer()
    {
        // Get answer from input field
        TMP_InputField answerField = NumberBoardAnswerField.GetComponent<TMP_InputField>();
        int answer;
        int.TryParse(answerField.text, out answer);
        
        answerField.text = "";
        return answer;
    }

    // Experiment
    public void StartExperiment()
    {
        // Load loading scene
        StartCoroutine(LoadLoading());
    }

    public (int[], int[], int[], int[]) GetExperimentCondition()
    {
        // Get current condition blocks
        Toggle[] toggles = experimentConditionUI.GetComponentsInChildren<Toggle>();
        bool[] conditions = new bool[toggles.Length];
        for (int i = 0; i < toggles.Length; ++i)
            conditions[i] = toggles[i].isOn;

        // Task array
        int[] runTask = ConditionToIndexArray(conditions.Skip(0).Take(5).ToArray());

        // Camera configuration array
        bool[] cameraConditionsTemp = new bool[4];
        cameraConditionsTemp[0] = true;
        conditions.Skip(5).Take(3).ToArray().CopyTo(cameraConditionsTemp, 1);

        int[] runCamera = ConditionToIndexArray(cameraConditionsTemp);

        // Level array
        int[] runLevel = ConditionToIndexArray(conditions.Skip(8).Take(4).ToArray());

        // Trial array
        int[] runTrial = ConditionToIndexArray(conditions.Skip(12).Take(2).ToArray());

        return (runTask, runLevel, runTrial, runCamera);
    }

    private int[] ConditionToIndexArray(bool[] conditions)
    {
        int[] indices;

        // Initialization
        int count = 0;
        foreach (bool condition in conditions)
            if (condition)
                count += 1;
        indices = new int[count];

        // Fill
        int i = 0;
        for (int c = 0; c < conditions.Length; ++c)
            if (conditions[c])
            {
                indices[i] = c;
                i += 1;
            }
        return indices;
    }

    public void LoadSurvey(int index)
    {
        if (surveyUI[index].activeSelf)
        {
            surveyUI[index].SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            surveyUI[index].SetActive(true);
            Time.timeScale = 0;
        }
    }

    // Additional
    public void ChangeAllStateDisplay()
    {
        allStateDisplay.SetActive(!allStateDisplay.activeSelf);
    }

    public void ChangeMinimapDisplay()
    {
        if (miniMap != null)
            miniMap.SetActive(!miniMap.activeSelf);
    }

    public void ChangeHelpDisplay()
    {
        helpDisplay.SetActive(!helpDisplay.activeSelf);
    }

    public void PopMessage(string message)
    {
        StartCoroutine(PopMessageCoroutine(message));
    }
    private IEnumerator PopMessageCoroutine(string message)
    {
        messagePanel.SetActive(true);
        messagePanelText.text = message;
        yield return new WaitForSeconds(1.5f);
        messagePanel.SetActive(false);
    }

    
    // Update panels
    private void UpdateExperimentPanel()
    {   
        if (experimentUI.activeSelf)
        {
            int trialIndex = 1;
            if (experimentManager.experimentIndex < experimentManager.trialIndices.Length)
                trialIndex = experimentManager.trialIndices[experimentManager.experimentIndex] + 1;
            experimentTaskPanelText.text =
                "Task: \n" + 
                "\t" + experimentManager.tasks[experimentManager.taskIndex] + "\n" + 
                "Level: " + "\tLevel " + string.Format("{0:0}", experimentManager.levelIndex) + "\n" +
                "Trial: " + "\tTrial " + string.Format("{0:0}", trialIndex) + "\n" +
                "Speed: \t" + string.Format("{0:0.0}", experimentManager.GetRobotSpeed());

            if (experimentStatePanelText != null)
                experimentStatePanelText.text =
                    taskMessage[experimentManager.taskIndex] +  "\n" + 
                    "Try not to hit any obstacles." + "\n\n" + 
                    "FPS: " + string.Format("{0:0}", FPS);
        }
    }

    private void UpdateRobotUIStatePenal()
    {
        if (wideViewUI.activeSelf)
        {
            taskStatePanelText.text =
                "Task: \n\t" + experimentManager.tasks[experimentManager.taskIndex] + "\n" + 
                "\n" +
                "Level: \t" + "level " + string.Format("{0:0}", experimentManager.levelIndex+1) + "\n" +
                "FPS: " + string.Format("{0:0}", FPS);
            robotStatePanelText.text = 
                "x: \t\t" + string.Format("{0:0.00}", dataRecorder.states[1]) + "\n" +
                "y: \t\t" + string.Format("{0:0.00}", dataRecorder.states[2]) + "\n" + 
                "yaw: \t" + string.Format("{0:0.00}", dataRecorder.states[3]) + "\n" + 
                "Vx: \t\t" + string.Format("{0:0.00}", dataRecorder.states[4]) +  "\n" +  
                "Wz: \t\t" + string.Format("{0:0.00}", dataRecorder.states[5]) + "\n";
        }
    }

    private void UpdateAllStatePenal()
    {
        if (allStateDisplay.activeSelf)
        {
            allStatePanelText.text = 
                "time: \t" + string.Format("{0:0.00}", dataRecorder.states[0]) + "\n" + 
                "\n" +
                "x: \t\t" + string.Format("{0:0.00}", dataRecorder.states[1]) + "\n" +
                "y: \t\t" + string.Format("{0:0.00}", dataRecorder.states[2]) + "\n" + 
                "yaw: \t" + string.Format("{0:0.00}", dataRecorder.states[3]) + "\n" + 
                "Vx: \t\t" + string.Format("{0:0.00}", dataRecorder.states[4]) +  "\n" +  
                "Wz: \t\t" + string.Format("{0:0.00}", dataRecorder.states[5]) + "\n" + 
                "\n" +
                "min_dis_obs: \t\t" + string.Format("{0:0.00}", dataRecorder.states[6]) + "\n" + 
                "min_dis_obs_dir: \t" + string.Format("{0:0.00}", dataRecorder.states[7]) + "\n" + 
                "min_dis_h: \t\t" + string.Format("{0:0.00}", dataRecorder.states[8]) + "\n" + 
                "min_dis_h_dir: \t" + string.Format("{0:0.00}", dataRecorder.states[9]) + "\n" + 
                "\n" +
                "main_cam_yaw: \t" + string.Format("{0:0.00}", dataRecorder.states[10]) + "\n" + 
                "main_cam_pitch: \t" + string.Format("{0:0.00}", dataRecorder.states[11]) + "\n" + 
                "main_cam_yaw_vel: \t" + string.Format("{0:0.00}", dataRecorder.states[12]) + "\n" + 
                "main_cam_yaw_vel: \t" + string.Format("{0:0.00}", dataRecorder.states[13]) + "\n" +
                "arm_cam_yaw: \t\t" + string.Format("{0:0.00}", dataRecorder.states[14]) + "\n" + 
                "arm_cam_pitch: \t" + string.Format("{0:0.00}", dataRecorder.states[15]) + "\n" + 
                "arm_cam_yaw_vel: \t" + string.Format("{0:0.00}", dataRecorder.states[16]) + "\n" + 
                "arm_cam_yaw_vel: \t" + string.Format("{0:0.00}", dataRecorder.states[17]) + "\n" +
                "\n" +
                "collision_self_name: \n" + dataRecorder.collisions[0] + "\n" + 
                "collision_other_name: \n" + dataRecorder.collisions[1];
        }
    }

    private void UpdateFPS()
    {
        FPS = (int)(FPSSum / FPSCount);
        FPSCount = 0;
        FPSSum = 0;
    }
}
