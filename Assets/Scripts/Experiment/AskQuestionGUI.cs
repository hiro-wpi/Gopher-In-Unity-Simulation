using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.PlayerLoop;
using System.Security.Policy;

/// <summary>
/// Handles the GUI for asking questions to the user, and getting their responses recorded
/// </summary>
public class AskQuestionGUI : MonoBehaviour
{
    
    // [SerializeField] private GoalBasedNavigataionAutonomy navigationTask;
    private bool isSimPaused = false;
    [SerializeField] private GameObject guiBlocker;

    // private float waitTimer = 0.0f;
    // private float waitDuration = 5.0f;
    public AudioSource buzzSound;

    // private bool allowResume = false;

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
    private List<List<string>> questions = new List<List<string>>(); // List of questions and answers
    private List<List<float>> responses = new List<List<float>>(); // List of responses to the questions

    private int currentQuestionIndex = -1; // The current question being asked

    // Assume the first item in the list is the question, the rest are the answers

    private float simStartTime = 0.0f;   // Get the simulation time when the question was asked
    private float realStartTime = 0.0f;  // Get the the time when we start asking the question

    // Log GUI

    // public enum Configuration
    // {
    //     Config1,
    //     Config2,
    //     Config3,

    //     Familiarization
    // }

    // public enum GUI
    // {
    //     Regular,
    //     AR,
    //     RegularAR
    // }

    // [SerializeField] private List<float> startTimesConfig1 = new List<float>{17.29f, 43.35f, 88.75f};
    // [SerializeField] private List<float> startTimesConfig2 = new List<float>{35.40f, 72.30f, 80.75f};
    // [SerializeField] private List<float> startTimesConfig3 = new List<float>{17.15f, 100f, 140f};
    // [SerializeField] private List<float> startTimesFamiliarization = new List<float>{17.50f, 35.75f, 71.75f};

    // public Configuration config = Configuration.Config1;
    // public GUI gui = GUI.Regular;

    public List<List<float>> questionairOrder = new List<List<float>>(); // List of questions and answers
    // list<float> will have the simulation time to ask the question, and the question number
    // private float simStartDelay = 3.0f;  // Delay before the simulation starts

    [SerializeField] private EyeTrackingNavigation eyeTracking;
    private string fileName = "responses";
    private string fileExtension = ".csv"; // File Extension default, change if needed
    private string taskSetupString = ""; // Add the task setup string to the response log
    private TextWriter textWriter;

    [ReadOnly] public bool respondedToQuestion = false;
    public bool respondedToQuestionSet = false;

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

    // Resume, Pause /////////////////////////////////////////////////////////////////////////////////////////

    private void ResumeSim(float timeScale)
    {
        Time.timeScale = timeScale;
        isSimPaused = false;
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

    // GUI /////////////////////////////////////////////////////////////////////////////////////////////////////////

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
            catch
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
    
    public void OnClickUnsure()
    {
         // Debug.Log("Answer A");
        AddResponse(currentQuestionIndex, 0);
        currentQuestionIndex = -1;
        respondedToQuestion = true;
    }
    public void OnClickAnswerA()
    {
        // Debug.Log("Answer A");
        AddResponse(currentQuestionIndex, 1);
        currentQuestionIndex = -1;
        respondedToQuestion = true;
    }
    public void OnClickAnswerB()
    {
        // Debug.Log("Answer B");
        AddResponse(currentQuestionIndex, 2);
        currentQuestionIndex = -1;
        respondedToQuestion = true;
    }
    public void OnClickAnswerC()
    {
        // Debug.Log("Answer C");
        AddResponse(currentQuestionIndex, 3);
        currentQuestionIndex = -1;
        respondedToQuestion = true;
    }
    public void OnClickAnswerD()
    {
        // Debug.Log("Answer D");
        AddResponse(currentQuestionIndex, 4);
        currentQuestionIndex = -1;
        respondedToQuestion = true;
    }
    //////// Storing the Data of our responses and the state of the sim //////

    // Add Questions to the Pool that we can use
    public void AddQuestion(string question, List<string> answers)
    {
        questions.Add(new List<string> { question });
        questions[questions.Count - 1].AddRange(answers);
    }

    // Add the Response we get from the question
    public void AddResponse(int questionIndex, int responseIndex)
    {
        // simStartTime
        // duration
        float duration = Time.realtimeSinceStartup - realStartTime;
        responses.Add(new List<float> {realStartTime, duration, questionIndex, responseIndex});
    }

    // Handles a set of question
    //      -- Loading the questions in a gui
    //      -- Cycle between 3 at a time
    //      -- Records the results afterward
    public IEnumerator AskSetOfQuestionsTime(List<List<int>> setNums, List<float> delayTimesBeforePause)
    {
        
        // iterate through all the sets of questions
        for(int i = 0; i < setNums.Count; i++)
        {
            List<int> questionNums = setNums[i];
            float delayTime = delayTimesBeforePause[i];
            // Wait until the time matches
            yield return new WaitUntil(() => Time.time > delayTime * Time.timeScale + 4f);

            // Ask the next set of questions
            respondedToQuestionSet = false;
            StartCoroutine(AskSetOfQuestions(questionNums));
            yield return new WaitUntil(() => respondedToQuestionSet == true);

        }

        StartCoroutine(SaveResponsesToCSV());
    }

    public IEnumerator AskSetOfQuestions(List<int> questionNums)
    {
        
        // Pause
        Time.timeScale = 0f; 

        PlayBuzzSound();

        yield return new WaitForSecondsRealtime(4f);
        // allowResume = true;
        
        // Show all the questions
        ShowGuiBlocker();

        // respondedToQuestionSet = false;

        foreach(int questionNum in questionNums)
        {
            // Question Init
            respondedToQuestion = false;
            simStartTime = Time.time;
            realStartTime = Time.realtimeSinceStartup;
            currentQuestionIndex = questionNum;

            ChangeText(questions[questionNum]);
            // wait until we get a response;
            yield return new WaitUntil(() => respondedToQuestion == true);
        }

        // Reset in case
        respondedToQuestion = false;
        respondedToQuestionSet = true;
        HideGuiBlocker();
        ResumeSim(2.0f);

    }

    // Takes the responses and saves it to a CSV
    public IEnumerator SaveResponsesToCSV()
    {
        Debug.Log("Saving Responses");
        yield return new WaitUntil(() => isSimPaused == false);

        LogResponse();
        eyeTracking.LogResponse();
    }

    public void LogResponse()
    {
        string responseString = "";

        responseString += taskSetupString;
        
        responseString += "Start Time, Duration, Question, Response\n";

        foreach(List<float> response in responses)
        {
            // responseString += "Sim Start Time: " + (response[0]-3f) + ", Real Duration: " + response[1] + ", Question: " + response[2] + ", Response: " + response[3] + "\n";
            responseString += response[0] + ", " + response[1] + ", " + response[2] + ", " + response[3] + "\n";
        }

        textWriter = new StreamWriter(fileName + fileExtension, false);
        textWriter.WriteLine(responseString);
        textWriter.Close();

    }

    // Set the file name, extention and any additional task setup information
    public void setFileInformation(string name, string extension, string taskSetup)
    {
        fileName = name;
        fileExtension = extension;
        taskSetupString = taskSetup;
    }

    void OnDestroy()
    {
        LogResponse();
    }
}
