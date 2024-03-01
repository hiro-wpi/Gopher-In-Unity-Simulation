using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Task Specific class to ask questions about goal based navigation
///     to the participant.
/// </summary>
public class AskGoalBasedNavigationQuestions : MonoBehaviour
{
    [SerializeField] private AskQuestionGUI askQuestionGui;
    [SerializeField] private GoalBasedNavigataionAutonomy navigationTask;

    // Setup of the navigation task
    private enum Configuration{ Config1, Config2, Config3, Familiarization }
    private enum GUI { Regular, AR, RegularAR }
    [SerializeField] private Configuration config = Configuration.Config1;
    [SerializeField] private GUI gui = GUI.Regular;
    

    // Start is called before the first frame update
    void Start()
    {
        
        // Task Specific Questions
        askQuestionGui.AddQuestion("[0] Test Question", new List<string> { "Answer A", "Answer B", "Answer C", "Answer D"});

        askQuestionGui.AddQuestion("[1] What is the status of the medicines on the table the robot just checked?", new List<string>{"Patient has all their medicines", "Patient is missing a medicine"});
        askQuestionGui.AddQuestion("[2] Which table is the robot currently at?", new List<string>{"Table 1", "Table 2", "Table 3", "Table 4"});
        askQuestionGui.AddQuestion("[3] Where will the robot be going to next?", new List<string>{"Next Table", "Pharmacy", "Nowhere (Task is complete)"});

        askQuestionGui.AddQuestion("[1] Is this the first time the robot is at the pharmacy?", new List<string>{"Yes", "No"});
        askQuestionGui.AddQuestion("[2] Which medicine is the robot going to grab?", new List<string>{"Red", "Blue", "Green", "Yellow"});
        askQuestionGui.AddQuestion("[3] Which table will the robot be delivering the medicine to?", new List<string>{"Table 1", "Table 2", "Table 3", "Table 4"});

        askQuestionGui.AddQuestion("[1] Which bird can't fly?", new List<string>{"Penguin", "Eagle", "Hummingbird", "Pigeon"});
        askQuestionGui.AddQuestion("[2] What is the largest cat?", new List<string>{"Cheetah", "Lion", "Jaguar", "Tiger"});
        askQuestionGui.AddQuestion("[3] What color are giraffes?", new List<string>{"Blue", "Orange", "Green", "Purple"});

        initNavigationTaskConfig();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // when we are red
    //      patient + go to pharmacy
    // green
    //      patient + 1 + checking
    // when getting meds
    //      patient + get medicine

    private void initNavigationTaskConfig()
    {
        // Setup the the data recorder file
        SetTaskString();

        // Setup the goal based navigation task and questions
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
                List<GoalBasedNavigataionAutonomy.State> states1 = new List<GoalBasedNavigataionAutonomy.State>{
                    GoalBasedNavigataionAutonomy.State.CheckPatient,                                                                                         
                    GoalBasedNavigataionAutonomy.State.GetMedicine,
                    GoalBasedNavigataionAutonomy.State.GoToPharmacy};

                List<GoalBasedNavigataionAutonomy.patient> patientNum1 = new List<GoalBasedNavigataionAutonomy.patient>{
                    GoalBasedNavigataionAutonomy.patient.Patient2,                                                                                         
                    GoalBasedNavigataionAutonomy.patient.Patient2, 
                    GoalBasedNavigataionAutonomy.patient.Patient4};


                StartCoroutine(AskSetOfQuestions(set1, states1, patientNum1));

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

                List<GoalBasedNavigataionAutonomy.State> states2 = new List<GoalBasedNavigataionAutonomy.State>{
                    GoalBasedNavigataionAutonomy.State.GetMedicine,                                                                                         
                    GoalBasedNavigataionAutonomy.State.CheckPatient,
                    GoalBasedNavigataionAutonomy.State.GoToPharmacy};

                List<GoalBasedNavigataionAutonomy.patient> patientNum2 = new List<GoalBasedNavigataionAutonomy.patient>{
                    GoalBasedNavigataionAutonomy.patient.Patient1,                                                                                         
                    GoalBasedNavigataionAutonomy.patient.Patient3, 
                    GoalBasedNavigataionAutonomy.patient.Patient3};

                // List<float> startTimes2 = new List<float>{35.40f, 72.30f, 80.75f};

                StartCoroutine(AskSetOfQuestions(set2, states2, patientNum2));

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

                List<GoalBasedNavigataionAutonomy.State> states3 = new List<GoalBasedNavigataionAutonomy.State>{
                    GoalBasedNavigataionAutonomy.State.GoToPharmacy,                                                                                         
                    GoalBasedNavigataionAutonomy.State.GetMedicine,
                    GoalBasedNavigataionAutonomy.State.Done};

                List<GoalBasedNavigataionAutonomy.patient> patientNum3 = new List<GoalBasedNavigataionAutonomy.patient>{
                    GoalBasedNavigataionAutonomy.patient.Patient2,                                                                                         
                    GoalBasedNavigataionAutonomy.patient.Patient3, 
                    GoalBasedNavigataionAutonomy.patient.Patient4};

                // List<float> startTimes3 = new List<float>{17.15f, 100f, 139f};

                StartCoroutine(AskSetOfQuestions(set3, states3, patientNum3));
                break;

            case Configuration.Familiarization:

                navigationTask.reverseCheckingOrder = false;
                navigationTask.patientMissingMeds = new List<bool>{true, false, false, false};
                navigationTask.patientMissingMedsColors = new List<string>{"Green", "None", "None", "None"};

                // Questions Setup

                
                List<List<int>> setFam = new List<List<int>>{ new List<int>{1, 2, 3},
                                                                          new List<int>{4, 5, 6}, 
                                                                          new List<int>{1, 2, 3}};

                List<GoalBasedNavigataionAutonomy.State> statesFam = new List<GoalBasedNavigataionAutonomy.State>{
                    GoalBasedNavigataionAutonomy.State.GoToPharmacy,                                                                                         
                    GoalBasedNavigataionAutonomy.State.GetMedicine,
                    GoalBasedNavigataionAutonomy.State.CheckPatient};

                List<GoalBasedNavigataionAutonomy.patient> patientNumFam = new List<GoalBasedNavigataionAutonomy.patient>{
                    GoalBasedNavigataionAutonomy.patient.Patient1,                                                                                         
                    GoalBasedNavigataionAutonomy.patient.Patient1, 
                    GoalBasedNavigataionAutonomy.patient.Patient3};

                // List<float> startTimesFamiliarization = new List<float>{17.16f, 100f, 139f};

                StartCoroutine(AskSetOfQuestions(setFam, statesFam, patientNumFam));

                break;
        }
    }

    private void SetTaskString()
    {
        // file name
        string fileName = "";
        switch(gui)
        {
            case GUI.Regular:
                fileName += "GUI";
                break;
            case GUI.AR:
                fileName += "AR";
                break;
            case GUI.RegularAR:
                fileName += "GUI_AR";
                break;
        }

        // file extension
        string fileExtension = "_navigation_task.csv";

        // task setup string
        string taskSetupString = "";
        taskSetupString += "GUI: ," + gui + "\n";
        taskSetupString += "Configuration: ," + config + "\n\n";

        // Update the file
        askQuestionGui.setFileInformation(fileName, fileExtension, taskSetupString);

    }

    // List<GoalBasedNavigataionAutonomy.State> states, List<GoalBasedNavigataionAutonomy.patient> patientNum
    public IEnumerator AskSetOfQuestions(List<List<int>> setNums, 
                                         List<GoalBasedNavigataionAutonomy.State> states, 
                                         List<GoalBasedNavigataionAutonomy.patient> patientNum)
    {
        
        // iterate through all the sets of questions
        for(int i = 0; i < setNums.Count; i++)
        {
            List<int> questionNums = setNums[i];
            // float delayTime = delayTimesBeforePause[i];
            GoalBasedNavigataionAutonomy.State state = states[i];
            GoalBasedNavigataionAutonomy.patient patient = patientNum[i];

            // Wait until the state and patient are the same
            yield return new WaitUntil(() => navigationTask.currentState == state 
                                            && navigationTask.currentPatient == patient);

            // Ask the next set of questions
            askQuestionGui.respondedToQuestionSet = false;
            StartCoroutine(askQuestionGui.AskSetOfQuestions(questionNums));
            yield return new WaitUntil(() => askQuestionGui.respondedToQuestionSet == true);

        }

        StartCoroutine(askQuestionGui.SaveResponsesToCSV());
    }


}
