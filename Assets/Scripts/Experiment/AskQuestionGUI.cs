using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Handles the GUI for asking questions to the user, and getting their responses recorded
/// </summary>
public class AskQuestionGUI : MonoBehaviour
{
    
    [SerializeField] private GoalBasedNavigataionAutonomy navigationTask;
    private bool isSimPaused = false;
    [SerializeField] private GameObject guiBlocker;

    private float waitTimer = 0.0f;
    private float waitDuration = 5.0f;
    public AudioSource buzzSound;

    private bool allowResume = false;

    // Panels
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private GameObject answerAPanel;
    [SerializeField] private GameObject answerBPanel;
    [SerializeField] private GameObject answerCPanel;
    [SerializeField] private GameObject answerDPanel;

    // Text
    private TextMeshProUGUI questionPanelText;
    private TextMeshProUGUI answerAPanelText;
    private TextMeshProUGUI answerBPanelText;
    private TextMeshProUGUI answerCPanelText;
    private TextMeshProUGUI answerDPanelText;

    // Question and Answers
    private List<List<string>> questionair = new List<List<string>>(); // List of questions and answers
    private List<List<float>> responses = new List<List<float>>(); // List of responses to the questions

    private int currentQuestionIndex = -1; // The current question being asked

    // Assume the first item in the list is the question, the rest are the answers

    private float simStartTime = 0.0f;   // Get the simulation time when the question was asked
    private float realStartTime = 0.0f;  // Get the the time when we start asking the question

    // Log GUI

    public enum Configuration
    {
        Config1,
        Config2,
        Config3
    }

    public enum GUI
    {
        Regular,
        AR,
        RegularAR
    }

    public Configuration config = Configuration.Config1;
    public GUI gui = GUI.Regular;

    public List<List<float>> questionairOrder = new List<List<float>>(); // List of questions and answers
    // list<float> will have the simulation time to ask the question, and the question number
    private float simStartDelay = 3.0f;  // Delay before the simulation starts

    public string fileName;
    private TextWriter textWriter;

    // States

    private void Start()
    {
        // Question
        questionPanelText = questionPanel.GetComponentInChildren<TextMeshProUGUI>();

        // Answers
        answerAPanelText = answerAPanel.GetComponentInChildren<TextMeshProUGUI>();
        answerBPanelText = answerBPanel.GetComponentInChildren<TextMeshProUGUI>();
        answerCPanelText = answerCPanel.GetComponentInChildren<TextMeshProUGUI>();
        answerDPanelText = answerDPanel.GetComponentInChildren<TextMeshProUGUI>();
        
        // [0] Can be ran at any point
        AddQuestion("[0] Test Question", new List<string> { "Answer A", "Answer B", "Answer C", "Answer D"});
        
        // [1] Can do it max 2 times, it can be done in the beginning, or after a patient is checked
        AddQuestion("[1] Where is the robot going?", new List<string> { "To a patient on the left", "To a patient on the right", "To the pharmacy"});
        
        // [2] Can do it max 4 times, it can be done in the beginning before reaching the patient, or after a patient is checked
        AddQuestion("[2] What has the robot determined about the patient's medicines?", new List<string> {"Patient has all their medicines", "Patient is missing some medicines", "Nothing yet, checking it right now"});
        
        // [3] Can do it max 2 times, it can be done when we are delivering the first medicine, or after we have dropped off the medicine
        AddQuestion("[3] Does the robot have the medicine in hand?", new List<string> { "Yes", "No"});
        
        // [4] Can do it max 2 times, it can be only done when it picked up the medicine and is delivering it
        AddQuestion("[4] Does the robot think it has successfully picked up the medicine?", new List<string> { "Yes - The robot thinks it picked up the medicine", "No - the robot thinks it failed picking up the medicine"});
        
        // [5] Can do it max 1 times, it can be ran at any point in the simulation
        AddQuestion("[5] How many patients left does the robot need to check on?", new List<string> { "1", "2", "3", "4"});
       
        // [6] Can do it max 1 time, it can be ran at any point in the simulation
        AddQuestion("[6] Which patient is the robot going to check on?", new List<string> { "Patient 1", "Patient 2", "Patient 3", "Patient 4"});
        
        // [7] Can do it max 2 time, can only be ran when the robot it grasping the medicine at the pharmacy
        AddQuestion("[7] What row is the robot grabbing the medicine from?", new List<string> { "Top Row", "Bottom Row"});
        
        // [8] Can be done max 1 time, only after dropping off the medicine
        AddQuestion("[8] Did the robot succeed in delivering the medicine to the patient?", new List<string> { "Yes - the robot delivered the medicine", "No - the robot did not deliver the medicine"});
        
        // [9] (It's roughly the same as [8] but it's a different question, so we can ask it again if we want to), or swtich between 8 and 9
        AddQuestion("[9] Does the robot think it succeeded in delivering the medicine?", new List<string> { "Yes", "No"});
        
        // [10] Can do it max 1 time, only after dropping off the medicine
        AddQuestion("[10] What will the robot conclude when it checks on the next patient?", new List<string> { "Patient has all their medicines", "Patient is missing some medicines"});
        
        // [11] Can do it max 1 time, only only before delivering the medicine (can be interchanged with question 3 or 4)
        AddQuestion("[11] Will the robot succeed in delivering the medicine?", new List<string> { "Yes", "No"});

        // [12] Can do it max 1 time, only before delivering the medicine 
        AddQuestion("[12] After delivering the medicine, which patient will the robot check?", new List<string> { "Patient 1", "Patient 2", "Patient 3", "Patient 4"});

        initNavigationTaskConfig();
        
        StartCoroutine(HandleTimingQuestions());
    }

    private void Update()
    {
        if (buzzSound == null)
        {
            buzzSound = GetComponent<AudioSource>();
            if (buzzSound == null)
            {
                return;
                // Debug.LogError("AudioSource is not assigned to the BuzzSoundController.");
            }

            Debug.Log("AudioSource is assigned to the BuzzSoundController.");
        }
    }

    // Pause the simulation to ask a question to the user
    // TODO later, remove AskQuestion(delyaTimeBeforePause), we should alway have a question number
    public void AskQuestion(float delayTimeBeforePause)
    {
        AskQuestion(0, delayTimeBeforePause);
    }

    public void AskQuestion(int questionNum, float delayTimeBeforePause)
    {
        currentQuestionIndex = questionNum;
        ChangeText(questionair[questionNum]);
        PauseSim(delayTimeBeforePause);
    }
    private void PauseSim(float delayTimeBeforePause)
    {
        if(isSimPaused)
        {
            return;
        }
        isSimPaused = true;
        StartCoroutine(PauseSimForTime(delayTimeBeforePause, 4f));
    }

    IEnumerator PauseSimForTime(float delayTimeBeforePause, float inspectionTime)
    {
        yield return new WaitForSeconds(delayTimeBeforePause);
        Time.timeScale = 0f;
        // isSimPaused = true;
        // Buzz user
        PlayBuzzSound();
        yield return new WaitForSecondsRealtime(inspectionTime);
        allowResume = true;
        simStartTime = Time.time;
        realStartTime = Time.realtimeSinceStartup;
        ShowGuiBlocker();
    }

    private void ResumeSim()
    {
        Time.timeScale = 1f;
        isSimPaused = false;
        allowResume = false;
        HideGuiBlocker();
    }

    private void HandleStopResume()
    {
        if(isSimPaused && allowResume)
        {
            ResumeSim();
        }
        
    }

    private void DelayDebugMessage()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer < waitDuration)
        {
            return;
        }
        waitTimer = 0.0f;
    }
    public void PlayBuzzSound()
    {
        if (buzzSound != null)
        {
            buzzSound.time = 1f; // Where in the wav file to start playing audio
            buzzSound.Play();
        }
    }

    private void ShowGuiBlocker()
    {
        guiBlocker.SetActive(true);
    }

    private void HideGuiBlocker()
    {
        guiBlocker.SetActive(false);
    }

    public void ChangeText(List<string> questionAnswers)
    {
        ChangeText(questionAnswers[0], questionAnswers.GetRange(1, questionAnswers.Count - 1));
    }

    public void ChangeText(string question, List<string> answers)
    {
        // Change the question text
        questionPanelText.text = question;

        // Check if there are enough answers 2 - 4 answers
        if (answers.Count < 2)
        {
            Debug.LogError("Not enough answers to display");
            return;
        }
        else if(answers.Count > 4)
        {
            Debug.LogError("Too many answers to display");
            return;
        }

        // Change the answer text
        answerAPanelText.text = answers[0];
        answerBPanelText.text = answers[1];
        
        for(int i = 0; i < 4; i++)
        {
            try
            {
                // Change the answer text
                if(i == 0)
                {
                    answerAPanel.SetActive(true);
                    answerAPanelText.text = answers[i];
                }
                else if(i == 1)
                {
                    answerBPanel.SetActive(true);
                    answerBPanelText.text = answers[i];
                }
                else if(i == 2)
                {
                    answerCPanel.SetActive(true);
                    answerCPanelText.text = answers[i];
                }
                else if(i == 3)
                {
                    answerDPanel.SetActive(true);
                    answerDPanelText.text = answers[i];
                }

            }
            catch(System.Exception e)
            {
                // If we can add the text, hide the answer panel
                if(i == 0)
                {
                    answerAPanel.SetActive(false);
                }
                else if(i == 1)
                {
                    answerBPanel.SetActive(false);
                }
                else if(i == 2)
                {
                    answerCPanel.SetActive(false);
                }
                else if(i == 3)
                {
                    answerDPanel.SetActive(false);
                }

            }
        }

    }

    public void OnClickAnswerA()
    {
        // Debug.Log("Answer A");
        AddResponse(currentQuestionIndex, 0);
        currentQuestionIndex = -1;
        ResumeSim();
    }
    public void OnClickAnswerB()
    {
        // Debug.Log("Answer B");
        AddResponse(currentQuestionIndex, 1);
        currentQuestionIndex = -1;
        ResumeSim();
    }
    public void OnClickAnswerC()
    {
        // Debug.Log("Answer C");
        AddResponse(currentQuestionIndex, 2);
        currentQuestionIndex = -1;
        ResumeSim();
    }
    public void OnClickAnswerD()
    {
        // Debug.Log("Answer D");
        AddResponse(currentQuestionIndex, 3);
        currentQuestionIndex = -1;
        ResumeSim();
    }

    //////// Storing the Data of our responses and the state of the sim //////

    public void AddQuestion(string question, List<string> answers)
    {
        questionair.Add(new List<string> { question });
        questionair[questionair.Count - 1].AddRange(answers);
    }

    public void AddResponse(int questionIndex, int responseIndex)
    {
        // simStartTime
        // duration
        float duration = Time.realtimeSinceStartup - realStartTime;
        responses.Add(new List<float> {realStartTime, duration, questionIndex, responseIndex});
    }

    public void SaveResponses()
    {
        // Save the responses to a file
    }

    public void PrintResponses()
    {
        StartCoroutine(DebugLogResponses());
    }

    IEnumerator DebugLogResponses()
    {

        yield return new WaitUntil(() => isSimPaused == false);
        // Debug.Log("Responses: ");
        LogResponse();
    }

    public void LogResponse()
    {
        string responseString = "";
        // Add which configuration we are using
        responseString += "GUI: ," + gui + "\n";
        responseString += "Configuration: ," + config + "\n\n";
        
        responseString += "Start Time, Duration, Question, Response\n";

        foreach(List<float> response in responses)
        {
            // responseString += "Sim Start Time: " + (response[0]-3f) + ", Real Duration: " + response[1] + ", Question: " + response[2] + ", Response: " + response[3] + "\n";
            responseString += response[0] + ", " + response[1] + ", " + response[2] + ", " + (response[3]+1) + "\n";
        }

        textWriter = new StreamWriter(fileName + "_navigation_task.csv", false);
        textWriter.WriteLine(responseString);
        textWriter.Close();

        // Debug.Log(responseString);
    }

    void OnDestroy()
    {
        LogResponse();
    }

    public void initNavigationTaskConfig()
    {
        switch(config)
        {
            case Configuration.Config1:
                //SetupConfig1();
                // Init Condition
                // Reversed = false; -> Normal
                navigationTask.reverseCheckingOrder = false;
                // MissingPatientMeds = 1001
                navigationTask.patientMissingMeds = new List<bool>{true, false, false, true};
                break;
            case Configuration.Config2:
                SetupConfig2();
                // Init Condition
                // Reversed = false; -> Normal
                navigationTask.reverseCheckingOrder = false;
                // MissingPatientMeds = 0110
                navigationTask.patientMissingMeds = new List<bool>{false, true, true, false};
                break;
            case Configuration.Config3:
                SetupCongig3();
                // Init Condition
                // Reversed = true;
                navigationTask.reverseCheckingOrder = true;
                // MissingPatientMeds = 0101
                navigationTask.patientMissingMeds = new List<bool>{false, true, false, true};
                break;
        }
    }


    public IEnumerator HandleTimingQuestions()
    {
        switch(config)
        {
            case Configuration.Config1:
                SetupConfig1();
                break;
            case Configuration.Config2:
                SetupConfig2();
                break;
            case Configuration.Config3:
                SetupCongig3();
                break;
        }

        foreach(List<float> question in questionairOrder)
        {
            // Wait until the time to ask the question
            yield return new WaitUntil(() => Time.time >= question[0]);

            // Ask the question
            AskQuestion((int)question[1], 0);
        }

        PrintResponses();
    }

    public void AddQuestionOrder(int questionNum, float simTime)
    {
        questionairOrder.Add(new List<float> { simTime + simStartDelay, questionNum });
    }

    // Questions Setup for Goal Based Navigation Task
    // TODO: Put this in a seperate file
    
    public void SetupConfig1()
    {
        AddQuestionOrder(2, 34.56f);
        AddQuestionOrder(4, 83.06f);
        AddQuestionOrder(5, 138.91f);
        AddQuestionOrder(10, 158.80f);
        AddQuestionOrder(1, 173.60f);
        AddQuestionOrder(11, 243.86f);
    }

    public void SetupConfig2()
    {
        AddQuestionOrder(2, 25.09f);
        AddQuestionOrder(10, 43.75f);
        AddQuestionOrder(7, 88.5f);
        AddQuestionOrder(8, 138.7f);
        AddQuestionOrder(1, 155.78f);
        AddQuestionOrder(9, 245.37f);
    }

    public void SetupCongig3()
    {
        AddQuestionOrder(1, 11.83f);
        AddQuestionOrder(2, 43.09f);
        AddQuestionOrder(7, 86.90f);
        AddQuestionOrder(3, 118.56f);
        AddQuestionOrder(10, 146.55f);
        AddQuestionOrder(11, 228.5f);
    }
}
