using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the GUI for asking questions to the user, and getting their responses recorded
/// </summary>
public class AskQuestionGUI : MonoBehaviour
{
    

    private bool isSimPaused = false;
    [SerializeField] private GameObject guiBlocker;

    private float waitTimer = 0.0f;
    private float waitDuration = 2.0f;
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

    private float simStartTime = 0.0f;
    private float realStartTime = 0.0f;
    // private float duration = 0.0f;

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

        AddQuestion("What is the robot going to do?", new List<string> { "Check on another patient", "Go to the pharmacy"});
        AddQuestion("What has the robot determined about the patient's medicines?", new List<string> { "Nothing yet, checking it right now", "Patient has all their medicines", "Patient is missing some medicines"});
        AddQuestion("Does the robot have the medicine in hand?", new List<string> { "Yes", "No"});
        AddQuestion("Does the robot think that it successfully picked up the medicine?", new List<string> { "Yes - The robot thinks it picked up the medicine", "No - the robot thinks it failed picking up the medicine"});
        AddQuestion("How many patients left does the robot need to check on?", new List<string> { "1", "2", "3", "4"});
        AddQuestion("Which patient is the robot going to check on?", new List<string> { "Patient 1", "Patient 2", "Patient 3", "Patient 4"});
        AddQuestion("What row is the robot grabbing the medicine from?", new List<string> { "Top Row", "Bottom Row"});
        AddQuestion("Did the robot succeed in delivering the medicine to the patient?", new List<string> { "Yes - the robot delivered the medicine", "No - the robot did not deliver the medicine"});
        AddQuestion("Does the robot think it succeeded in delivering the medicine?", new List<string> { "Yes", "No"});
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

        HandleStopResume();
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
        if(isSimPaused && allowResume && Input.GetKeyDown("space"))
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
        responses.Add(new List<float> {simStartTime, duration, questionIndex+1, responseIndex+1 });
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
        string responseString = "";
        foreach(List<float> response in responses)
        {
            responseString += "Sim Start Time: " + response[0] + ", Real Duration: " + response[1] + ", Question: " + response[2] + ", Response: " + response[3] + "\n";
        }

        Debug.Log(responseString);
    }
    
    

}
