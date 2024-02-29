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
    
    // Start Times for the Questions
    [SerializeField] private List<float> startTimesConfig1 = new List<float>{17.29f, 43.35f, 88.75f};
    [SerializeField] private List<float> startTimesConfig2 = new List<float>{35.40f, 72.30f, 80.75f};
    [SerializeField] private List<float> startTimesConfig3 = new List<float>{17.15f, 100f, 140f};
    [SerializeField] private List<float> startTimesFamiliarization = new List<float>{17.50f, 35.75f, 71.75f};

    

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

                StartCoroutine(askQuestionGui.AskSetOfQuestions(set1, startTimesConfig1));

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

                StartCoroutine(askQuestionGui.AskSetOfQuestions(set2, startTimesConfig2));

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

                StartCoroutine(askQuestionGui.AskSetOfQuestions(set3, startTimesConfig3));
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

                StartCoroutine(askQuestionGui.AskSetOfQuestions(setFamiliarization, startTimesFamiliarization));

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


}
