// using System;
using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using UnityEngine;

/// <summary>
///    Goal Based Navigataion Autonomy
///    Main Script to handle full autonomy for the goal-based delivery task   
/// 
///    Sub Goals:
///    - Be able to pass in the target position and orientation
///    - Be able to visualize the trajectory
///    - Be able to cancel the trajectory
///    - Identiy when the robot has reached the goal
///    
///    Task Goals for the completion of the autonomy:
///    - Be able to navigate to a set of waypoints
/// </summary>
public class GoalBasedNavigataionAutonomy : MonoBehaviour
{

    // Test setup
    [SerializeField] private GraphicalInterface graphicalInterface;
    [SerializeField] private RectTransform displayRect;
    private Camera cam;

    // AR Featrues 
    [SerializeField] private FloorSelector floorSelector;
    [SerializeField] private DrawWaypoints drawWaypoints;
    // [SerializeField] private GenerateARGameObject generateARGameObject;

    // Autonomy
    [SerializeField] private ArticulationBaseController baseController;

    private GameObject robot;

    // State Machines
    private enum State { SetFirstPatcient, CheckPatcient, GoToPharmacy, GetMedicine, GoDeliverMedicine, ChangePatient, Done};
    private State state = State.SetFirstPatcient;

    private enum Patcient { Patcient1, Patcient2, Patcient3, Patcient4};

    private Patcient currentPatcient = Patcient.Patcient1;


    // Patcient Navigation Waypoints
    public Vector3[] patcientPosition = new Vector3[4];
    public Vector3[] patcientRotations = new Vector3[4];

    // Pharmacy Navigation Waypoint
    public Vector3 pharmacyPosition;
    public Vector3 pharmacyRotation;

    // Public Navigation Waypoints between pharmacy to patcient room
    public List<Vector3> transfereWaypointPositions = new List<Vector3>();
    public List<Vector3> transferWaypointRotations = new List<Vector3>();
    public List<Vector3> transferReturnWaypointRotations = new List<Vector3>();


    // Tracked Trajectory of the Robot

    private List<Vector3> waypointPositions = new List<Vector3>();
    private List<Vector3> waypointRotations = new List<Vector3>();

    private bool waypointReachedGoal = false; // This is for just reaching successive points on the list
    private bool reachedGoal = false;  // reached the whole goal

    void Start()
    {
    }


    void OnEnable()
    {
        // Subscribe to the event
        // floorSelector.OnFloorSelected += OnFloorSelected;
        // objectSelector.OnObjectSelected += OnObjectSelected;

        if (baseController != null)
        {
            baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete += OnBaseReachedGoal;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up
        // floorSelector.OnFloorSelected -= OnFloorSelected;
        // objectSelector.OnObjectSelected -= OnObjectSelected;

        if (baseController != null)
        {
            baseController.OnAutonomyTrajectory -= OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete -= OnBaseReachedGoal;
        }
    }

    private void SetAutonomyGoal(Vector3 worldPosition, Vector3 worldRotationEuler)
    {
        if (robot == null)
        {
            return;
        }

        // Reset the reached goal flag
        reachedGoal = false;
        waypointReachedGoal = false;

        // Convert from euler angles to quaternion
        Quaternion worldRotation = Quaternion.Euler(worldRotationEuler);
        baseController.SetAutonomyTarget(worldPosition, worldRotation);
    }


    private void OnBaseTrajectoryGenerated()
    {
        // Debug.Log("Base trajectory generated");

        var (globalWaypoints, LocalWaypoints) = 
            baseController.GetTrajectories();
        
        // Clear old waypoints
        drawWaypoints.RemoveLine("Global Path");
        // Add new waypoints
        drawWaypoints.DrawLine("Global Path", globalWaypoints);

        // Automatically Send the robot to the goal
        baseController.MoveToAutonomyTarget();
    }

    private void OnBaseReachedGoal()
    {
        // Event will be called when the robot reaches the goal
        // Debug.Log("Base reached goal");
        // reachedGoal = true;
        waypointReachedGoal = true;
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

                //////////////////////////////////////////
                floorSelector.SetCameraAndDisplay(cam, displayRect);
                // objectSelector.SetCameraAndDisplay(cam, displayRect);
                //////////////////////////////////////////
            }
        }

        if(robot == null)
        {
            robot = GameObject.Find("Gopher(Clone)");
            
            // Set the articulation base controller
            baseController = robot.GetComponentInChildren<ArticulationBaseController>();

            // Constantly subscribe to the event to make our trajectory visible
            //      check if we arrive at the goal
            baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete += OnBaseReachedGoal;

            // ScheuldeNextTask();
        }
        

        // Keyboard press enter to start autonomy
        // if (Input.GetKeyDown(KeyCode.Return))
        // {
        //     Vector3 p1Position = new Vector3(-8.393f, 0.1f, 5.95f);
        //     Vector3 p1Rotation = new Vector3(0f, 90f, 0f);

        //     Vector3 p2Position = new Vector3(-7.5f, 0.1f, 8.1f);
        //     Vector3 p2Rotation = new Vector3(0f, -90f, 0f);

        //     List<Vector3> posTrajectory = new List<Vector3>{p1Position, p2Position};
        //     List<Vector3> rotTrajectory = new List<Vector3>{p1Rotation, p2Rotation};

        //     // SetWaypoints(posTrajectory, rotTrajectory);
        //     SetWaypoints(p1Position, p1Rotation);
        // }

        // Press Enter to send the robot to the pharmacy
        // if (Input.GetKeyDown(KeyCode.Return))
        // {
        //     Debug.Log("Parmacy");
        //     // SetAutonomyGoal(pharmacyPosition, pharmacyRotation);

        //     StartCoroutine(FollowWaypoints());
            
        // }
        
        

        ScheuldeNextTask();

        // // Keyboard press space to emergency stop
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     Debug.Log("Emergency Stop");
        //     baseController.CancelAutonomyTarget();
        // }

        

    }

    private void ScheulderTest()
    {
        switch (state)
        {
            case State.SetFirstPatcient:
                Debug.Log("Set the first Patcient");
                SetAutonomyGoal(patcientPosition[(int)currentPatcient], patcientRotations[(int)currentPatcient]);
                state = State.CheckPatcient;
                break;

            case State.CheckPatcient:

                if(reachedGoal)
                {
                    state = State.ChangePatient;
                }
                break;

            case State.ChangePatient:
                
                // Change the patient
                // Debug.Log("Changing Patient");

                // if all the patients have been checked, we are done
                // if we have changed the patient, go back to check the patient
                switch (currentPatcient)
                {
                    case Patcient.Patcient1:
                        // Set the next 
                        Debug.Log("Changing to Patcient 2");
                        currentPatcient = Patcient.Patcient2;
                        SetAutonomyGoal(patcientPosition[(int)currentPatcient], patcientRotations[(int)currentPatcient]);
                        state = State.CheckPatcient;
                        break;
                    case Patcient.Patcient2:
                        // Set the next patient
                        Debug.Log("Changing to Patcient 3");
                        currentPatcient = Patcient.Patcient3;
                        SetAutonomyGoal(patcientPosition[(int)currentPatcient], patcientRotations[(int)currentPatcient]);
                        state = State.CheckPatcient;
                        break;
                    case Patcient.Patcient3:
                        // Set the next patient
                        Debug.Log("Changing to Patcient 4");
                        currentPatcient = Patcient.Patcient4;
                        SetAutonomyGoal(patcientPosition[(int)currentPatcient], patcientRotations[(int)currentPatcient]);
                        state = State.CheckPatcient;
                        break;
                    case Patcient.Patcient4:
                        // Set the next patient
                        Debug.Log("All patients have been checked");
                        state = State.Done;
                        break;
                    default:
                        break;
                }

                break;
            
            case State.Done:
                break;
                
        }
        
    }


    // State Machine   //////////////////////////////////////////////////////////////////////////////////////////
    private void ScheuldeNextTask()
    {
        switch (state)
        {   
            case State.SetFirstPatcient:

                // Autonomy starts here
                // Go to the first patient
                Debug.Log("Setting the first patient");
                SetWaypoints(patcientPosition[(int)currentPatcient], patcientRotations[(int)currentPatcient]);

                state = State.CheckPatcient;

                break;

            case State.CheckPatcient:

                // Check if the patient has all their medications
                if (reachedGoal)
                {
                    Debug.Log("Checking the patient");
                    // TODO: Do this actually properly
                    if (currentPatcient == Patcient.Patcient2)
                    {
                        Debug.Log("Paticeint 2 is missing their meds, going to the pharmacy");
                        
                        List<Vector3> posTraj = new List<Vector3>{transfereWaypointPositions[1],transfereWaypointPositions[0]} ;
                        List<Vector3> rotTraj = new List<Vector3>{transferReturnWaypointRotations[1], transferReturnWaypointRotations[0]};
                        // List<Vector3> posTraj = transfereWaypointPositions;
                        // List<Vector3> rotTraj = transferReturnWaypointRotations;

                        posTraj.Add(pharmacyPosition);
                        rotTraj.Add(pharmacyRotation);

                        SetWaypoints(posTraj, rotTraj);

                        state = State.GoToPharmacy;
                    }
                    else
                    {
                        Debug.Log("Patient has all their meds, going to the next patient");
                        state = State.ChangePatient;
                    }
                    // state = State.ChangePatient;
                }
                break;

            case State.GoToPharmacy:

                // Go to the pharmacy

                // if we arrive at the pharmacy, get the medicine
                if (reachedGoal)
                {
                    Debug.Log("Going to the pharmacy");
                    state = State.GetMedicine;
                }
                
                break;

            case State.GetMedicine:

                // Get the medicine
                // TODO: Implement the medicine getting
                bool gotMedicine = true;

                // if we have the medicine, go deliver the medicine
                if (gotMedicine)
                {
                    Debug.Log("Got Medicine");
                    List<Vector3> posTraj = transfereWaypointPositions;
                    List<Vector3> rotTraj = transferWaypointRotations;
                    // List<Vector3> posTraj = transfereWaypointPositions;
                    // List<Vector3> rotTraj = transferReturnWaypointRotations;

                    posTraj.Add(patcientPosition[(int)currentPatcient]);
                    rotTraj.Add(patcientRotations[(int)currentPatcient]);

                    SetWaypoints(posTraj, rotTraj);
                    state = State.GoDeliverMedicine;
                }
                
                break;

            case State.GoDeliverMedicine:
                // Deliver the medicine
                
                // if we have delivered the medicine, change the patient
                if (reachedGoal)
                {
                    Debug.Log("Delivering Medicine");
                    state = State.ChangePatient;
                }
                
                break;

            case State.ChangePatient:
                // Change the patient
                // Debug.Log("Changing Patient");

                // if all the patients have been checked, we are done
                // if we have changed the patient, go back to check the patient
                switch (currentPatcient)
                {
                    case Patcient.Patcient1:
                        // Set the next 
                        Debug.Log("Changing to Patcient 2");
                        currentPatcient = Patcient.Patcient2;
                        break;
                    case Patcient.Patcient2:
                        // Set the next patient
                        Debug.Log("Changing to Patcient 3");
                        currentPatcient = Patcient.Patcient3;
                        break;
                    case Patcient.Patcient3:
                        // Set the next patient
                        Debug.Log("Changing to Patcient 4");
                        currentPatcient = Patcient.Patcient4;
                        break;
                    case Patcient.Patcient4:
                        // Set the next patient
                        Debug.Log("All patients have been checked");
                        state = State.Done;
                        break;
                    default:
                        break;
                }

                SetWaypoints(patcientPosition[(int)currentPatcient], patcientRotations[(int)currentPatcient]);
                state = State.CheckPatcient;

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

    // New Method for Scheuling Task

    // lets actually do a couroutine where we can actually have it handle the waypoints being played int he abck
    IEnumerator FollowWaypoints()
    {
        Debug.Log("Start Motion");

        for(int i = 0; i < waypointPositions.Count; i++)
        {
            Debug.Log("Going to Waypoint");
            SetAutonomyGoal(waypointPositions[i], waypointRotations[i]);
            yield return new WaitUntil(() => waypointReachedGoal);
        }
        reachedGoal = true;
    }

    // Automatically Sets the waypoint and goes to it
    private void SetWaypoints(Vector3 pos, Vector3 rot)
    {
        waypointPositions = new List<Vector3>{pos};
        waypointRotations = new List<Vector3>{rot};

        StartCoroutine(FollowWaypoints());
    }

    // Automatically Sets the waypoint and goes to it
    private void SetWaypoints(List<Vector3> pos, List<Vector3> rot)
    {
        if(pos.Count != rot.Count)
        {
            Debug.LogWarning("The list used are not the same size, they are not being added");
            return;
        }

        waypointPositions = pos;
        waypointRotations = rot;

        StartCoroutine(FollowWaypoints());
    }

    // private void StartFollowingWaypoints()
    // {
    //     StartCoroutine(FollowWaypoints());
    // }
}
