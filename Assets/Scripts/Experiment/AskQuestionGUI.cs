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
    
    [SerializeField] private GoalBasedNavigataionAutonomy navigationTask;
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

    public enum Configuration
    {
        Config1,
        Config2,
        Config3,

        Familiarization
    }

    public enum GUI
    {
        Regular,
        AR,
        RegularAR
    }

    [SerializeField] private List<float> startTimesConfig1 = new List<float>{17.29f, 43.35f, 88.75f};
    [SerializeField] private List<float> startTimesConfig2 = new List<float>{35.40f, 72.30f, 80.75f};
    [SerializeField] private List<float> startTimesConfig3 = new List<float>{17.15f, 100f, 140f};
    [SerializeField] private List<float> startTimesFamiliarization = new List<float>{17.50f, 35.75f, 71.75f};

    public Configuration config = Configuration.Config1;
    public GUI gui = GUI.Regular;

    public List<List<float>> questionairOrder = new List<List<float>>(); // List of questions and answers
    // list<float> will have the simulation time to ask the question, and the question number
    // private float simStartDelay = 3.0f;  // Delay before the simulation starts

    [SerializeField] private EyeTrackingNavigation eyeTracking;
    public string fileName;
    private TextWriter textWriter;

    private bool respondedToQuestion = false;

    // States

    private void Start()
    {
        // Time.timeScale = 2.0f;
        // Question
        questionPanelText = questionPanel.GetComponentInChildren<TextMeshProUGUI>();

        // Answers
        answerAPanelText = answerAPanel.GetComponentInChildren<TextMeshProUGUI>();
        answerBPanelText = answerBPanel.GetComponentInChildren<TextMeshProUGUI>();
        answerCPanelText = answerCPanel.GetComponentInChildren<TextMeshProUGUI>();
        answerDPanelText = answerDPanel.GetComponentInChildren<TextMeshProUGUI>();
        
        // [0] Can be ran at any point
        AddQuestion("[0] Test Question", new List<string> { "Answer A", "Answer B", "Answer C", "Answer D"});
        
        // // [1] Can do it max 2 times, it can be done in the beginning, or after a patient is checked
        // AddQuestion("[1] Where is the robot going?", new List<string> { "To a patient on the left", "To a patient on the right", "To the pharmacy"});
        
        // // [2] Can do it max 4 times, it can be done in the beginning before reaching the patient, or after a patient is checked
        // AddQuestion("[2] What has the robot determined about the patient's medicines?", new List<string> {"Patient has all their medicines", "Patient is missing some medicines", "Nothing yet, checking it right now"});
        
        // // [3] Can do it max 2 times, it can be done when we are delivering the first medicine, or after we have dropped off the medicine
        // AddQuestion("[3] Does the robot have the medicine in hand?", new List<string> { "Yes", "No"});
        
        // // [4] Can do it max 2 times, it can be only done when it picked up the medicine and is delivering it
        // AddQuestion("[4] Does the robot think it has successfully picked up the medicine?", new List<string> { "Yes - The robot thinks it picked up the medicine", "No - the robot thinks it failed picking up the medicine"});
        
        // // [5] Can do it max 1 times, it can be ran at any point in the simulation
        // AddQuestion("[5] How many patients left does the robot need to check on?", new List<string> { "1", "2", "3", "4"});
       
        // // [6] Can do it max 1 time, it can be ran at any point in the simulation
        // AddQuestion("[6] Which patient is the robot going to check on?", new List<string> { "Patient 1", "Patient 2", "Patient 3", "Patient 4"});
        
        // // [7] Can do it max 2 time, can only be ran when the robot it grasping the medicine at the pharmacy
        // AddQuestion("[7] What row is the robot grabbing the medicine from?", new List<string> { "Top Row", "Bottom Row"});
        
        // // [8] Can be done max 1 time, only after dropping off the medicine
        // AddQuestion("[8] Did the robot succeed in delivering the medicine to the patient?", new List<string> { "Yes - the robot delivered the medicine", "No - the robot did not deliver the medicine"});
        
        // // [9] (It's roughly the same as [8] but it's a different question, so we can ask it again if we want to), or swtich between 8 and 9
        // AddQuestion("[9] Does the robot think it succeeded in delivering the medicine?", new List<string> { "Yes", "No"});
        
        // // [10] Can do it max 1 time, only after dropping off the medicine
        // AddQuestion("[10] What will the robot conclude when it checks on the next patient?", new List<string> { "Patient has all their medicines", "Patient is missing some medicines"});
        
        // // [11] Can do it max 1 time, only only before delivering the medicine (can be interchanged with question 3 or 4)
        // AddQuestion("[11] Will the robot succeed in delivering the medicine?", new List<string> { "Yes", "No"});

        // // [12] Can do it max 1 time, only before delivering the medicine 
        // AddQuestion("[12] After delivering the medicine, which patient will the robot check?", new List<string> { "Patient 1", "Patient 2", "Patient 3", "Patient 4"});

        // Present Question
        AddQuestion("[1] What is the status of the medicines on the table the robot just checked?", new List<string>{"Patient has all their medicines", "Patient is missing a medicine"});
        // Past Question
        AddQuestion("[2] Which table was the robot at before arriving here?", new List<string>{"Robot was not at any table before arriving here", "Table 1", "Table 2", "Table 3"});
        // Future Question
        AddQuestion("[3] Where will the robot be going to next?", new List<string>{"Next Table", "Pharmacy", "Nowhere (Task is complete)"});

        AddQuestion("[1] Is this the first time the robot is at the pharmacy?", new List<string>{"Yes", "No"});
        AddQuestion("[2] Which medicine is the robot going to grab?", new List<string>{"Red", "Blue", "Green", "Yellow"});
        AddQuestion("[3] Which table will the robot be delivering the medicine to?", new List<string>{"Table 1", "Table 2", "Table 3", "Table 4"});

        AddQuestion("[1] Which bird can't fly?", new List<string>{"Penguin", "Eagle", "Hummingbird", "Pigeon"});
        AddQuestion("[2] What is the largest cat?", new List<string>{"Cheetah", "Lion", "Jaguar", "Tiger"});
        AddQuestion("[3] What color are giraffes?", new List<string>{"Blue", "Orange", "Green", "Purple"});

        initNavigationTaskConfig();
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
        // allowResume = false;
        // HideGuiBlocker();
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

    public void initNavigationTaskConfig()
    {
        switch(config)
        {
            case Configuration.Config1:

                // Init Condition
                navigationTask.reverseCheckingOrder = false;
                navigationTask.patientMissingMeds = new List<bool>{false, true, false, true};
                navigationTask.patientMissingMedsColors = new List<string>{"None", "Blue", "None", "Yellow"};

                // Questions Setup
                List<List<int>> set1 = new List<List<int>>{ new List<int>{1, 2, 3},
                                                            new List<int>{4, 5, 6}, 
                                                            new List<int>{1, 2, 3}};

                // List<float> startTimes1 = new List<float>{17.29f, 43.35f, 88.75f};

                StartCoroutine(AskSetOfQuestions(set1, startTimesConfig1));

                break;
            case Configuration.Config2:

                // Init Condition
                navigationTask.reverseCheckingOrder = false;
                navigationTask.patientMissingMeds = new List<bool>{true, false, true, false};
                navigationTask.patientMissingMedsColors = new List<string>{"Red", "None", "Blue", "None"};

                // Questions Setup
                List<List<int>> set2 = new List<List<int>>{ new List<int>{4, 5, 6},
                                                            new List<int>{1, 2, 3}, 
                                                            new List<int>{1, 2, 3}};

                // List<float> startTimes2 = new List<float>{35.40f, 72.30f, 80.75f};

                StartCoroutine(AskSetOfQuestions(set2, startTimesConfig2));

                break;
            case Configuration.Config3:
                
                // Init Condition 
                navigationTask.reverseCheckingOrder = false;
                navigationTask.patientMissingMeds = new List<bool>{false, true, true, false};
                navigationTask.patientMissingMedsColors = new List<string>{"None", "Red", "Yellow", "None"};

                // Questions Setup
                List<List<int>> set3 = new List<List<int>>{ new List<int>{1, 2, 3},
                                                            new List<int>{4, 5, 6}, 
                                                            new List<int>{1, 2, 3}};

                // List<float> startTimes3 = new List<float>{17.15f, 100f, 139f};

                StartCoroutine(AskSetOfQuestions(set3, startTimesConfig3));
                break;

            case Configuration.Familiarization:

                navigationTask.reverseCheckingOrder = false;
                navigationTask.patientMissingMeds = new List<bool>{true, false, false, false};
                navigationTask.patientMissingMedsColors = new List<string>{"Green", "None", "None", "None"};

                // Questions Setup

                
                List<List<int>> setFamiliarization = new List<List<int>>{ new List<int>{1, 2, 3},
                                                                          new List<int>{4, 5, 6}, 
                                                                          new List<int>{1, 2, 3}};

                // List<float> startTimesFamiliarization = new List<float>{17.16f, 100f, 139f};

                StartCoroutine(AskSetOfQuestions(setFamiliarization, startTimesFamiliarization));

                break;
        }
    }

    // Handles a set of question
    //      -- Loading the questions in a gui
    //      -- Cycle between 3 at a time
    //      -- Records the results afterward
    IEnumerator AskSetOfQuestions(List<List<int>> setNums, List<float> delayTimesBeforePause)
    {
        
        // Pause The Simulation
        // Wait until the desired time
        // yield return new WaitForSeconds(delayTimeBeforePause);

        // iterate through all the sets of questions
        for(int i = 0; i < setNums.Count; i++)
        {
            List<int> questionNums = setNums[i];
            float delayTime = delayTimesBeforePause[i];

            // Wait until the time matches
            yield return new WaitUntil(() => Time.time > delayTime * Time.timeScale + 4f);

            // Pause
            Time.timeScale = 0f; 

            PlayBuzzSound();

            yield return new WaitForSecondsRealtime(4f);
            // allowResume = true;
            

            // Show all the questions
            ShowGuiBlocker();

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

            HideGuiBlocker();
            ResumeSim(2.0f);

        }

        Debug.Log("Saving Responses");

        if(config != Configuration.Familiarization)
        {
            SaveNavTaskResponsesToCSV();
            eyeTracking.LogResponse();
        }
        

    }

    // Takes the responses and saves it to a CSV
    IEnumerator SaveResponsesToCSV()
    {

        if(config != Configuration.Familiarization)
        {
            yield return new WaitUntil(() => isSimPaused == false);
            // Debug.Log("Responses: ");
            SaveNavTaskResponsesToCSV();
            eyeTracking.LogResponse();
        }
        
    }

    public void SaveNavTaskResponsesToCSV()
    {
        string responseString = "";
        // Add which configuration we are using
        responseString += "GUI: ," + gui + "\n";
        responseString += "Configuration: ," + config + "\n\n";
        
        responseString += "Start Time, Duration, Question, Response\n";

        foreach(List<float> response in responses)
        {
            // responseString += "Sim Start Time: " + (response[0]-3f) + ", Real Duration: " + response[1] + ", Question: " + response[2] + ", Response: " + response[3] + "\n";
            responseString += response[0] + ", " + response[1] + ", " + response[2] + ", " + response[3] + "\n";
        }

        textWriter = new StreamWriter(fileName + "_navigation_task.csv", false);
        textWriter.WriteLine(responseString);
        textWriter.Close();

        // Debug.Log(responseString);
    }

    void OnDestroy()
    {
        SaveNavTaskResponsesToCSV();
    }
}
