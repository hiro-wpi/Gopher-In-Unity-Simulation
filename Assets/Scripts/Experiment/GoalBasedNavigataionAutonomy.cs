// using System;
using System;
using System.Collections;
using System.Collections.Generic;
// using System.Numerics;


// using System.Numerics;
using UnityEngine;

/// <summary>
///    Goal Based Navigataion Autonomy
///    Main Script to handle full autonomy for the goal-based delivery task 
///    in the hospital environment
///    
///     This script is able to:
///     - Set the gross waypoints for the robot to navigate
///     - Follow the gross waypoints
///     - Generate AR for carts and meds
///     - Change the color and icon of the AR for the carts to describe state of task
///     
///    Todo
///    - Seperate the autonomy from the state machine
///         - ARNavigationAutonomy
///         - ARManipulationAutonomy

/// </summary>
public class GoalBasedNavigataionAutonomy : MonoBehaviour
{

    // Test setup
    [SerializeField] private GraphicalInterface graphicalInterface;
    [SerializeField] private RectTransform displayRect;
    private Camera cam;

    // AR Automation
    [SerializeField] private ARNavigationAutomation arNavAuto;
    [SerializeField] private ARManipulationAutomation arManipAuto;

    // AR Featrues 
    [SerializeField] private GenerateARGameObject arGenerator;
    [SerializeField] private HighlightObjectOnCanvas highlightObject;

    [SerializeField] private Sprite icon1;
    [SerializeField] private Sprite icon2;
    [SerializeField] private Sprite icon3;

    private GameObject robot;
    [SerializeField] private string robotName = "Gopher";

    // State Machines
    [SerializeField] public enum State { SetFirstPatient, CheckPatient, GoToPharmacy, GetMedicine, GoDeliverMedicine, DropOffMedicine, ChangePatient, Done};
    [SerializeField, ReadOnly] public State currentState = State.SetFirstPatient;

    [SerializeField] public enum patient { Patient1, Patient2, Patient3, Patient4};

    [SerializeField, ReadOnly] public patient currentPatient = patient.Patient1;


    // Patient Navigation Waypoints
    public Vector3[] patientPosition = new Vector3[4];
    public Vector3[] patientRotations = new Vector3[4];

    // Pharmacy Navigation Waypoint
    public Vector3 pharmacyPosition;
    public Vector3 pharmacyRotation;

    // Public Navigation Waypoints between pharmacy to patient room
    public List<Vector3> transfereWaypointPositions = new List<Vector3>();
    public List<Vector3> transferWaypointRotations = new List<Vector3>();
    public List<Vector3> transferReturnWaypointRotations = new List<Vector3>();

    
    private GameObject[] patientMedCarts;

    // public GameObject[] graspableMeds;
    public List<GameObject> graspableMeds = new List<GameObject>();

    public List<bool> patientMissingMeds = new List<bool>{false, true, false, false}; // which patients are missing meds. true = missing meds
    public List<string> patientMissingMedsColors = new List<string>{"None", "None", "None", "None"}; 

    [SerializeField] private GameObject[] patientMedGameObjects;  // 0 load missing meds, 1 load all meds

    private GameObject pluckedMed;
    public bool reverseCheckingOrder = false;

    public bool isTaskInit = false;

    private float showCartARForRealSeconds = 1.5f; // The amount of time to show the real AR
    private float showArmTrajectoryARForRealSeconds = 0.25f; // The amount of time before transitioning to the next state

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Get the main camera if it is not already set
        if (cam == null)
        {
            // Get all the active cameras referanced in the graphical interface
            Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();
            if (cameras.Length > 0)
            {
                cam = cameras[0];

            }
        }

        if(robot == null)
        {
            robot = GameObject.Find(robotName + "(Clone)");
            
            if(robot == null)
            {
                // Debug.Log("No robot found");
                return;
            }
        }

        if(!(arNavAuto.autoReady && arManipAuto.autoReady))
        {
            return;
        }

        if(patientMedCarts == null)
        {
            patientMedCarts = GameObject.FindGameObjectsWithTag("GoalObject");
            
            if(patientMedCarts.Length == 0)
            {
                // Debug.Log("No carts found");;
            }
            else
            {
                // Debug.Log("Carts found");
                // load the patient meds into the scene
                for(int i = 0; i < patientPosition.Length; i++)
                {
                    LoadPatientMedPrefab(patientPosition[i], patientMissingMeds[i]);
                }
            }
        }

        if(graspableMeds.Count == 0)
        {
            GameObject[] graspable = GameObject.FindGameObjectsWithTag("GraspableObject");

            // Filter the graspable object for those with the script "AutoGraspable"
            List<GameObject> filteredGraspable = new List<GameObject>();
            foreach(GameObject obj in graspable)
            {
                if(obj.GetComponent<AutoGraspable>() != null)
                {
                    filteredGraspable.Add(obj);
                }
            }
            graspableMeds = filteredGraspable;

        }

        if(!isTaskInit)
        {
            isTaskInit = true;
            Time.timeScale = 2.0f;
            StartCoroutine(SchedulerCoroutine());
        }

        // ScheduleNextTask();

    }

    // State Machine   //////////////////////////////////////////////////////////////////////////////////////////
    private void ScheduleNextTask()
    {
        List<Vector3> posTraj = new List<Vector3>();
        List<Vector3> rotTraj = new List<Vector3>();
        Vector3 patientPos;

        switch (currentState)
        {   
            case State.SetFirstPatient:

                // Time.timeScale = 2.0f;

                // Autonomy starts here
                graphicalInterface.AddLogInfo("Starting nurse request: check patient medicines");
                graphicalInterface.AddLogInfo("Going to first patient");
                // Create the waypoints to get to the first patient
                posTraj = new List<Vector3>(transfereWaypointPositions);
                rotTraj = new List<Vector3>(transferWaypointRotations);
                posTraj.Add(patientPosition[(int)currentPatient]);
                rotTraj.Add(patientRotations[(int)currentPatient]);
                arNavAuto.SetWaypoints(posTraj, rotTraj);

                // Create AR for the cart nearest to the patient
                patientPos = patientPosition[(int)currentPatient];
                GenerateARForNearestCart(patientPos);

                currentState = State.CheckPatient;

                break;

            case State.CheckPatient:

                // Check if the patient has all their medications
                if (arNavAuto.reachedGoal)
                {
                    graphicalInterface.AddLogInfo("Arrived at destination");
                    graphicalInterface.AddLogInfo("Checking patient medicines");

                    
                    
                    // Debug.Log("Checking the patient");
                    // TODO: Do this actually properly
                    if (patientMissingMeds[(int)currentPatient])
                    {   
                        // askQuestionGUI.AskQuestion(2, 0.2f);

                        // Change AR to indicate an issue
                        graphicalInterface.AddLogInfo("Patient is missing a " + patientMissingMedsColors[(int)currentPatient] + " medicine");
                        patientPos = patientPosition[(int)currentPatient];
                        ChangeNearestCartARColor(patientPos, Color.red);
                        ChangeNearestCartCanvasHighlightIcon(patientPos, icon3);

                        // Create the waypoints to get to the pharmacy
                        graphicalInterface.AddLogInfo("Going to pharmacy");
                        posTraj = new List<Vector3>{transfereWaypointPositions[1],transfereWaypointPositions[0]} ;
                        rotTraj = new List<Vector3>{transferReturnWaypointRotations[1], transferReturnWaypointRotations[0]};
                        posTraj.Add(pharmacyPosition);
                        rotTraj.Add(pharmacyRotation);
                        arNavAuto.SetWaypoints(posTraj, rotTraj);

                        currentState = State.GoToPharmacy;
                    }
                    else
                    {
                        // askQuestionGUI.AskQuestion(2, 0.2f);
                        // Change AR to indicate no issue
                        graphicalInterface.AddLogInfo("Patient has expected medicines");
                        patientPos = patientPosition[(int)currentPatient];
                        ChangeNearestCartARColor(patientPos, Color.green);
                        ChangeNearestCartCanvasHighlightIcon(patientPos, icon2);

                        graphicalInterface.AddLogInfo("Going to next patient");
                        currentState = State.ChangePatient;
                    }
                    // state = State.ChangePatient;
                }
                break;

            case State.GoToPharmacy:

                // Go to the pharmacy

                // if we arrive at the pharmacy, get the medicine
                if (arNavAuto.reachedGoal)
                {
                    graphicalInterface.AddLogInfo("Arrived at destination");
                    graphicalInterface.AddLogInfo("Retrieving " + patientMissingMedsColors[(int)currentPatient] + " medicine");    
                    
                    // SetArmToMedTarget();
                    SetArmToMedTarget(patientMissingMedsColors[(int)currentPatient]);
                    // askQuestionGUI.AskQuestion(7, 0.5f);
                    currentState = State.GetMedicine;
                }
                
                break;

            case State.GetMedicine:

                // if we have the medicine, go deliver the medicine
                if (arManipAuto.reachedArmGoal)
                {
                    // Debug.Log("Got Medicine");
                    graphicalInterface.AddLogInfo("Medicine in posession");
                    graphicalInterface.AddLogInfo("Delivering medicine to patient");
                    // arManipAuto.HomeJoints();
                    posTraj = new List<Vector3>(transfereWaypointPositions);
                    rotTraj = new List<Vector3>(transferWaypointRotations);
                    posTraj.Add(patientPosition[(int)currentPatient]);
                    rotTraj.Add(patientRotations[(int)currentPatient]);
                    arNavAuto.SetWaypoints(posTraj, rotTraj);

                    currentState = State.GoDeliverMedicine;
                    
                    // askQuestionGUI.AskQuestion(4, 10f);
                }
                
                break;

            case State.GoDeliverMedicine:
                // Deliver the medicine
                
                // if we came back to the patient, drop off the medicine
                if (arNavAuto.reachedGoal)
                {

                    graphicalInterface.AddLogInfo("Arrived at destination");

                    patientPos = patientPosition[(int)currentPatient];
                    GameObject nearestCart = GetNearestCart(patientPos);
                    // GenerateARForNearestCart(nearestCart.transform.position);
                    List<GameObject> arFeatures = arGenerator.GetARGameObject(nearestCart);
                    if(arFeatures.Count == 1)
                    {
                        // Set the drop off position and rotation for the med
                        Vector3 dropOffPos = arFeatures[0].transform.position + Vector3.right * 0.1f + Vector3.back * 0.1f;
                        Vector3 hoverDropOffPos2 = dropOffPos + Vector3.up * 0.1f;
                        Vector3 hoverDropOffPos1 = hoverDropOffPos2 + Vector3.right * 0.2f;


                        Quaternion dropOffRot = arFeatures[0].transform.rotation;
                        Quaternion hoverDropOffRot2 = dropOffRot;
                        Quaternion hoverDropOffRot1 = dropOffRot;
                        
                        // Set the waypoints for the arm to drop off the med
                        List<Vector3> positions = new List<Vector3>{hoverDropOffPos1, hoverDropOffPos2, hoverDropOffPos1};
                        List<Quaternion> rotations = new List<Quaternion>{hoverDropOffRot1, hoverDropOffRot2, hoverDropOffRot1};
                        List<int> gripperActions = new List<int>{-1, 0, -1, };
                        
                        graphicalInterface.AddLogInfo("Delivering medicine");
                        arManipAuto.SetArmWaypoints(positions, rotations, gripperActions);
                    }

                    // askQuestionGUI.AskQuestion(0.5f);
                    currentState = State.DropOffMedicine;
                }
                
                break;

            case State.DropOffMedicine:
                // Drop off the medicine
                if(arManipAuto.reachedArmGoal)
                {
                    // askQuestionGUI.AskQuestion(8, 0.25f);
                    graphicalInterface.AddLogInfo("Delivered medicine");
                    graphicalInterface.AddLogInfo("Issue resolved");
                    patientPos = patientPosition[(int)currentPatient];
                    ChangeNearestCartARColor(patientPos, Color.green);
                    ChangeNearestCartCanvasHighlightIcon(patientPos, icon2);

                    arManipAuto.HomeJoints();
                    
                    graphicalInterface.AddLogInfo("Checking next patient medicines");
                    currentState = State.ChangePatient;
                }
                break;

            case State.ChangePatient:
                // Change the patient
                // Debug.Log("Changing Patient");

                // if all the patients have been checked, we are done
                // if we have changed the patient, go back to check the patient
                
                // Do the normal checking order
                switch (currentPatient)
                {
                    case patient.Patient1:
                        // Set the next
                        currentPatient = patient.Patient2;
                        break;
                    case patient.Patient2:
                        // Set the next patient
                        currentPatient = patient.Patient3;
                        break;
                    case patient.Patient3:
                        // Set the next patient
                        currentPatient = patient.Patient4;
                        break;
                    case patient.Patient4:
                        // Set the next patient
                        graphicalInterface.AddLogInfo("All patient medicines have been checked");
                        // Debug.Log("All patients have been checked");
                        currentState = State.Done;
                        break;
                    default:
                        break;
                }

                // Destroy the AR of the previous patient
                StartCoroutine(RemoveARForPatient(patientPosition[(int)currentPatient - 1]));
                
                if(currentState == State.Done)
                {
                    // Remove the AR for the last patient
                    StartCoroutine(RemoveARForPatient(patientPosition[(int)currentPatient]));
                    // askQuestionGUI.DebugLogResponses();
                    // askQuestionGUI.PrintResponses();
                    return;
                }

                // Go to the next patient
                arNavAuto.SetWaypoints(patientPosition[(int)currentPatient], patientRotations[(int)currentPatient]);

                // // Destroy the AR of the previous patient
                // StartCoroutine(RemoveARForPatient(patientPosition[(int)currentpatient - 1]));

                // Generate AR for the nearest cart
                patientPos = patientPosition[(int)currentPatient];
                GenerateARForNearestCart(patientPos);

                currentState = State.CheckPatient;

                break;

            case State.Done:
                // Done
                // The autonomy is done
                // Debug.Log("Autonomy is done");
                break;

            default:
                break;
        }
    }


    // Scheduler in the form of a coroutine
    IEnumerator SchedulerCoroutine()
    {
        while(currentState != State.Done)
        {
            List<Vector3> posTraj = new List<Vector3>();
            List<Vector3> rotTraj = new List<Vector3>();
            Vector3 patientPos;

            switch (currentState)
            {   
                case State.SetFirstPatient:

                    // Time.timeScale = 2.0f;

                    // Autonomy starts here
                    graphicalInterface.AddLogInfo("Starting nurse request: check patient medicines");
                    // graphicalInterface.AddLogInfo("Going to first patient");

                    // Create the waypoints to get to the first patient
                    posTraj = new List<Vector3>(transfereWaypointPositions);
                    rotTraj = new List<Vector3>(transferWaypointRotations);
                    posTraj.Add(patientPosition[(int)currentPatient]);
                    rotTraj.Add(patientRotations[(int)currentPatient]);
                    arNavAuto.SetWaypoints(posTraj, rotTraj);

                    // Create AR for the cart nearest to the patient
                    patientPos = patientPosition[(int)currentPatient];
                    GenerateARForNearestCart(patientPos);

                    currentState = State.CheckPatient;

                    break;

                case State.CheckPatient:

                    yield return null;

                    // Check if the patient has all their medications
                    if (arNavAuto.reachedGoal)
                    {
                        // graphicalInterface.AddLogInfo("Arrived at destination");
                        graphicalInterface.AddLogInfo("Checking patient medicines");

                        // Debug.Log("Checking the patient");
                        // TODO: Do this actually properly
                        if (patientMissingMeds[(int)currentPatient])
                        {   
                            // askQuestionGUI.AskQuestion(2, 0.2f);

                            // Change AR to indicate an issue
                            graphicalInterface.AddLogInfo("Patient is missing a " + patientMissingMedsColors[(int)currentPatient] + " medicine");
                            patientPos = patientPosition[(int)currentPatient];
                            ChangeNearestCartARColor(patientPos, Color.red);
                            ChangeNearestCartCanvasHighlightIcon(patientPos, icon3);

                            // wait for AR to load using real seconds
                            yield return new WaitForSecondsRealtime(showCartARForRealSeconds);

                            // Create the waypoints to get to the pharmacy
                            graphicalInterface.AddLogInfo("Going to pharmacy");
                            posTraj = new List<Vector3>{transfereWaypointPositions[1],transfereWaypointPositions[0]} ;
                            rotTraj = new List<Vector3>{transferReturnWaypointRotations[1], transferReturnWaypointRotations[0]};
                            posTraj.Add(pharmacyPosition);
                            rotTraj.Add(pharmacyRotation);
                            arNavAuto.SetWaypoints(posTraj, rotTraj);

                            currentState = State.GoToPharmacy;
                        }
                        else
                        {
                            // askQuestionGUI.AskQuestion(2, 0.2f);
                            // Change AR to indicate no issue
                            graphicalInterface.AddLogInfo("Patient has expected medicines");
                            patientPos = patientPosition[(int)currentPatient];
                            ChangeNearestCartARColor(patientPos, Color.green);
                            ChangeNearestCartCanvasHighlightIcon(patientPos, icon2);

                            // wait for AR to load using real seconds
                            yield return new WaitForSecondsRealtime(showCartARForRealSeconds);

                            graphicalInterface.AddLogInfo("Going to next patient");
                            currentState = State.ChangePatient;
                        }
                        // state = State.ChangePatient;
                    }
                    break;

                case State.GoToPharmacy:

                    // Go to the pharmacy
                    yield return null;

                    // if we arrive at the pharmacy, get the medicine
                    if (arNavAuto.reachedGoal)
                    {
                        graphicalInterface.AddLogInfo("Arrived at destination");
                        graphicalInterface.AddLogInfo("Retrieving " + patientMissingMedsColors[(int)currentPatient] + " medicine");    
                        
                        // SetArmToMedTarget();
                        SetArmToMedTarget(patientMissingMedsColors[(int)currentPatient]);
                        // askQuestionGUI.AskQuestion(7, 0.5f);

                        yield return new WaitForSecondsRealtime(showArmTrajectoryARForRealSeconds);

                        currentState = State.GetMedicine;
                    }
                    
                    break;

                case State.GetMedicine:

                    yield return null;

                    // if we have the medicine, go deliver the medicine
                    if (arManipAuto.reachedArmGoal)
                    {
                        // Debug.Log("Got Medicine");
                        graphicalInterface.AddLogInfo("Medicine in posession");
                        graphicalInterface.AddLogInfo("Delivering medicine to patient");
                        // arManipAuto.HomeJoints();
                        posTraj = new List<Vector3>(transfereWaypointPositions);
                        rotTraj = new List<Vector3>(transferWaypointRotations);
                        posTraj.Add(patientPosition[(int)currentPatient]);
                        rotTraj.Add(patientRotations[(int)currentPatient]);
                        arNavAuto.SetWaypoints(posTraj, rotTraj);

                        currentState = State.GoDeliverMedicine;
                        
                        // askQuestionGUI.AskQuestion(4, 10f);
                    }
                    
                    break;

                case State.GoDeliverMedicine:
                    // Deliver the medicine
                    yield return null;
                    // if we came back to the patient, drop off the medicine
                    if (arNavAuto.reachedGoal)
                    {

                        graphicalInterface.AddLogInfo("Arrived at destination");

                        patientPos = patientPosition[(int)currentPatient];
                        GameObject nearestCart = GetNearestCart(patientPos);
                        // GenerateARForNearestCart(nearestCart.transform.position);
                        List<GameObject> arFeatures = arGenerator.GetARGameObject(nearestCart);
                        if(arFeatures.Count == 1)
                        {
                            // Set the drop off position and rotation for the med
                            Vector3 dropOffPos = arFeatures[0].transform.position + Vector3.right * 0.1f + Vector3.back * 0.1f;
                            Vector3 hoverDropOffPos2 = dropOffPos + Vector3.up * 0.1f;
                            Vector3 hoverDropOffPos1 = hoverDropOffPos2 + Vector3.right * 0.2f;


                            Quaternion dropOffRot = arFeatures[0].transform.rotation;
                            Quaternion hoverDropOffRot2 = dropOffRot;
                            Quaternion hoverDropOffRot1 = dropOffRot;
                            
                            // Set the waypoints for the arm to drop off the med
                            List<Vector3> positions = new List<Vector3>{hoverDropOffPos1, hoverDropOffPos2, hoverDropOffPos1};
                            List<Quaternion> rotations = new List<Quaternion>{hoverDropOffRot1, hoverDropOffRot2, hoverDropOffRot1};
                            List<int> gripperActions = new List<int>{-1, 0, -1, };
                            
                            graphicalInterface.AddLogInfo("Delivering medicine");
                            arManipAuto.SetArmWaypoints(positions, rotations, gripperActions);
                        }

                        // askQuestionGUI.AskQuestion(0.5f);
                        currentState = State.DropOffMedicine;
                    }
                    
                    break;

                case State.DropOffMedicine:
                    // Drop off the medicine
                    yield return null;

                    if(arManipAuto.reachedArmGoal)
                    {
                        // askQuestionGUI.AskQuestion(8, 0.25f);
                        graphicalInterface.AddLogInfo("Delivered medicine");
                        graphicalInterface.AddLogInfo("Issue resolved");
                        patientPos = patientPosition[(int)currentPatient];
                        ChangeNearestCartARColor(patientPos, Color.green);
                        ChangeNearestCartCanvasHighlightIcon(patientPos, icon2);

                        // wait for AR to load using real seconds
                        yield return new WaitForSecondsRealtime(showCartARForRealSeconds);

                        arManipAuto.HomeJoints();
                        
                        graphicalInterface.AddLogInfo("Checking next patient medicines");
                        currentState = State.ChangePatient;
                    }
                    break;

                case State.ChangePatient:
                    // Change the patient
                    // Debug.Log("Changing Patient");

                    // if all the patients have been checked, we are done
                    // if we have changed the patient, go back to check the patient
                    
                    // Do the normal checking order
                    switch (currentPatient)
                    {
                        case patient.Patient1:
                            // Set the next
                            currentPatient = patient.Patient2;
                            break;
                        case patient.Patient2:
                            // Set the next patient
                            currentPatient = patient.Patient3;
                            break;
                        case patient.Patient3:
                            // Set the next patient
                            currentPatient = patient.Patient4;
                            break;
                        case patient.Patient4:
                            // Set the next patient
                            graphicalInterface.AddLogInfo("All patient medicines have been checked");
                            // Debug.Log("All patients have been checked");
                            currentState = State.Done;
                            break;
                        default:
                            break;
                    }

                    // Destroy the AR of the previous patient
                    StartCoroutine(RemoveARForPatient(patientPosition[(int)currentPatient - 1]));
                    
                    if(currentState == State.Done)
                    {
                        // Remove the AR for the last patient
                        StartCoroutine(RemoveARForPatient(patientPosition[(int)currentPatient]));
                        // askQuestionGUI.DebugLogResponses();
                        // askQuestionGUI.PrintResponses();
                        
                    }
                    else
                    {
                        // Go to the next patient
                        arNavAuto.SetWaypoints(patientPosition[(int)currentPatient], patientRotations[(int)currentPatient]);

                        // // Destroy the AR of the previous patient
                        // StartCoroutine(RemoveARForPatient(patientPosition[(int)currentpatient - 1]));

                        // Generate AR for the nearest cart
                        patientPos = patientPosition[(int)currentPatient];
                        GenerateARForNearestCart(patientPos);

                        currentState = State.CheckPatient;
                    }

                    break;

                case State.Done:
                    
                    break;

                default:
                    break;
            }
            
        }
        
    }
    


    // AR Functions //////////////////////////////////////////////////////////////////////////////////////////

    // Generate AR for the nearest cart
    private void GenerateARForNearestCart(Vector3 position)
    {
        // Get the nearest cart relative to the robot
        GameObject nearestCart = GetNearestCart(position);

        // Generate AR for the nearest cart
        var type = GenerateARGameObject.ARObjectType.Cube;
            arGenerator.Instantiate(
                nearestCart,
                type,
                new Vector3(0, 0.93f, -0.02f),
                new Vector3(180, 0, 90),
                new Vector3(0.025f, 0.35f, 0.5f),
                Color.cyan,
                0.35f
            );

        // Get the cart ar feature
        List<GameObject> arFeatures = arGenerator.GetARGameObject(nearestCart);

        if(arFeatures.Count == 1)
        {
            // Add a highlight to the AR Feature
            CreateHighlightObjectOnCanvas(nearestCart, new Vector3(0f, 1.1f, 0));
        }
       
    }

    // Get the nearest cart
    private GameObject GetNearestCart(Vector3 position)
    {
        // Get the nearest cart relative to the robot
        GameObject nearestCart = patientMedCarts[0];
        float nearestDistance = Vector3.Distance(nearestCart.transform.position, position);
        foreach(GameObject cart in patientMedCarts)
        {
            float distance = Vector3.Distance(cart.transform.position, position);
            if(distance < nearestDistance)
            {
                nearestCart = cart;
                nearestDistance = distance;
            }
        }

        return nearestCart;
    }
    
    
    private void ChangeCartARColor(GameObject cart, Color color)
    {
        List<GameObject> arFeatures = arGenerator.GetARGameObject(cart);

        if(arFeatures.Count == 1)
        {
            // if there is only one AR feature, change the color
            arFeatures[0].GetComponent<Renderer>().material.color = color;
        }
    }

    private void ChangeNearestCartARColor(Vector3 position, Color color)
    {
        GameObject nearestCart = GetNearestCart(position);
        ChangeCartARColor(nearestCart, color);
    }

    private void GenerateARForMed(GameObject med)
    {
        var type = GenerateARGameObject.ARObjectType.Cube;
        arGenerator.Instantiate(
            med,
            type,
            new Vector3(0, 0.045f, 0),
            new Vector3(0, 0, 0),
            new Vector3(0.06f, 0.06f, 0.1f),
            Color.green,
            0.25f
        );
    }

    private GameObject PickGraspableMed()
    {
        // Get a med
        GameObject med = graspableMeds[0];

        // Remvove the med from the list
        graspableMeds.Remove(med);

        // Generate AR for the nearest med
        GenerateARForMed(med);

        // Debug.Log("Med Chosen");

        return med;
    }

    private GameObject PickGraspableMed(string color)
    {
        // Get a med
        foreach(GameObject med in graspableMeds)
        {
            if(med.name.Contains(color))
            {
                // Remvove the med from the list
                graspableMeds.Remove(med);

                // Generate AR for the nearest med
                GenerateARForMed(med);
                return med;
            }
        }

        Debug.LogWarning("Medicine with color not found, will return another medicine");
        
        GameObject med_alt = graspableMeds[0];

        // Remvove the med from the list
        graspableMeds.Remove(med_alt);

        // Generate AR for the nearest med
        GenerateARForMed(med_alt);

        return med_alt;
    }

    // private void Update2DARVisablity()
    // {
    //     RaycastHit hit;
    //     Vector3 patientPos = patientPosition[(int)currentpatient];
    //     GameObject nearestCart = GetNearestCart(patientPos);

    //     // Perform a raycast from objectA towards objectB
    //     if (Physics.Raycast(robot.transform.position + Vector3.up * 1.5f, nearestCart.transform.position - (robot.transform.position + Vector3.up * 1.5f) , out hit))
    //     {
    //         // Check if the raycast hit an object with the specified tag
    //         if (hit.collider.name == "monitor_cart")
    //         {
    //             // If the raycast hits an object with the specified tag, show objectA
    //             hide2DARUI.SetActive(true);
    //             // Debug.Log("Set Active");
    //         }
    //         else
    //         {
    //             // If the raycast doesn't hit an object with the specified tag, hide objectA
    //             hide2DARUI.SetActive(false);
    //             // Debug.Log("Deactive");
                
    //         }
    //     }
    //     else
    //     {
    //         // If the raycast doesn't hit anything, hide objectA
    //         hide2DARUI.SetActive(false);
    //         // Debug.Log("Deactive");
    //     }

    //     // Debug.DrawRay(robot.transform.position + Vector3.up * 1.5f, nearestCart.transform.position - (robot.transform.position + Vector3.up * 1.5f), Color.red);

    // }

    


    private void SetArmToMedTarget()
    {
        // pluckedMed = PickGraspableMed();
        pluckedMed = PickGraspableMed("Red");

        var (hoverTransform, graspingTransform) = arManipAuto.GetHoverAndGraspTransforms(pluckedMed);
        List<Vector3> positions = new List<Vector3>{hoverTransform.position, graspingTransform.position, hoverTransform.position};
        List<Quaternion> rotations = new List<Quaternion>{hoverTransform.rotation, graspingTransform.rotation, hoverTransform.rotation};
        List<int> gripperActions = new List<int>{-1, 1, -1};
        // SetArmTarget(med);
        arManipAuto.SetArmWaypoints(positions, rotations, gripperActions);
    }

    private void SetArmToMedTarget(string color)
    {
        // pluckedMed = PickGraspableMed();
        pluckedMed = PickGraspableMed(color);

        

        var (hoverTransform, graspingTransform) = arManipAuto.GetHoverAndGraspTransforms(pluckedMed);

        // Create a location that is close to the home position
        GameObject homeEE = new GameObject("EEHomePosition");
        homeEE.transform.parent = robot.transform;
        homeEE.transform.localPosition = new Vector3(-0.085f, 0.75f, 0.45f);
        homeEE.transform.rotation = hoverTransform.rotation;

        List<Vector3> positions = new List<Vector3>{hoverTransform.position, graspingTransform.position, hoverTransform.position, homeEE.transform.position};
        List<Quaternion> rotations = new List<Quaternion>{hoverTransform.rotation, graspingTransform.rotation, hoverTransform.rotation, homeEE.transform.rotation};
        List<int> gripperActions = new List<int>{-1, 1, -1, -1};
        // SetArmTarget(med);
        arManipAuto.SetArmWaypoints(positions, rotations, gripperActions);
    }

    // Canvas Highlight Functions //////////////////////////////////////////////////////////////////////////////////////////
    private void CreateHighlightObjectOnCanvas(GameObject selectedObject, Vector3 positionOffset = new Vector3())
    {
        // Create a cube gameobject as a child of the selected object
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(selectedObject.transform);
        cube.transform.localPosition = positionOffset;
        cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);   // Scale will affect the size of the highlight
        cube.name = "Highlight2DObjectLocation";

        // Disable the collider and render of the cube
        cube.GetComponent<Collider>().enabled = false;
        cube.GetComponent<Renderer>().enabled = false;

        var location = HighlightObjectOnCanvas.ElementPosition.Center;
            highlightObject.Highlight(
                cube,
                cam,
                displayRect,
                icon1,
                Color.white,
                adjustUIScale: false,
                position: location
            );
    }

    private void ReplaceHighlightObjectOnCanvas(GameObject selectedObject, Sprite icon, Vector3 positionOffset = new Vector3())
    {
        // Find the cube
        var cube = selectedObject.transform.Find("Highlight2DObjectLocation").gameObject;

        // Destroy the previous highlight
        highlightObject.RemoveHighlight(cube);

        // Create a new highlight
        var location = HighlightObjectOnCanvas.ElementPosition.Center;
            highlightObject.Highlight(
                cube,
                cam,
                displayRect,
                icon,
                Color.white,
                adjustUIScale: false,
                position: location
            );
    }

    private void RemoveHighlightObjectOnCanvas(GameObject selectedObject)
    {
        // Find the cube
        var cube = selectedObject.transform.Find("Highlight2DObjectLocation").gameObject;

        // Destroy the previous highlight
        highlightObject.RemoveHighlight(cube);
    }

    private void ChangeNearestCartCanvasHighlightIcon(Vector3 position, Sprite icon)
    {
        GameObject nearestCart = GetNearestCart(position);
        ReplaceHighlightObjectOnCanvas(nearestCart, icon);
    }

    // Load Patient Meds //////////////////////////////////////////////////////////////////////////////////////////

    IEnumerator RemoveARForPatient(Vector3 position)
    {
        // wait for 2 seconds
        yield return new WaitForSeconds(2);

        // destroy the 3DAR and CanvasAR for the nearest cart
        GameObject nearestCart = GetNearestCart(position);
        RemoveHighlightObjectOnCanvas(nearestCart);
        arGenerator.Destroy(nearestCart);
        if(pluckedMed != null)
        {
            arGenerator.Destroy(pluckedMed);
            pluckedMed = null;
        }
    }
    

    private void LoadPatientMedPrefab(Vector3 position, bool medState)
    {
        GameObject nearestCart = GetNearestCart(position);
        //instatiate the prefab and place it on the cart
        int medStateInt = medState ? 0 : 1; // shorthand: if true, return 1, false return 1
        GameObject medPrefab = Instantiate(patientMedGameObjects[medStateInt], nearestCart.transform);
        medPrefab.transform.localPosition = new Vector3(-0.002f, 0.87f, -0.1f);
        // medPrefab.transform.localRotation = Quaternion.Euler(180, 0, 90);
    }    

}
