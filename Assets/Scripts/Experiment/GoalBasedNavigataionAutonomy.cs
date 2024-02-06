// using System;
using System.Collections;
using System.Collections.Generic;
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
///     - Change the color of the AR for the carts to describe state of task
///     
///    
/// 
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
    [SerializeField] private GenerateARGameObject arGenerator;

    // Autonomy
    [SerializeField] private ArticulationBaseController baseController;
    [SerializeField] private ArticulationArmController armController;
    [SerializeField] private AutoGrasping autoGrasping;
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


    // Tracked Trajectory of the Robot base

    private List<Vector3> waypointPositions = new List<Vector3>();
    private List<Vector3> waypointRotations = new List<Vector3>();

    private bool waypointReachedGoal = false; // This is for just reaching successive points on the list
    private bool reachedGoal = false;  // reached the whole goal


    private GameObject[] patientMedCarts;

    // public GameObject[] graspableMeds;
    private List<GameObject> graspableMeds = new List<GameObject>();

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
            if(robot == null)
            {
                Debug.Log("No robot found");
                return;
            }

            baseController = robot.GetComponentInChildren<ArticulationBaseController>();
            armController = robot.GetComponentInChildren<ArticulationArmController>();
            autoGrasping = robot.GetComponentInChildren<AutoGrasping>();
            // Constantly subscribe to the event to make our trajectory visible
            //      check if we arrive at the goal
            baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete += OnBaseReachedGoal;

            armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
            armController.OnAutonomyComplete += OnArmAutonomyComplete;

            // ScheuldeNextTask();
        }

        if(patientMedCarts == null)
        {
            patientMedCarts = GameObject.FindGameObjectsWithTag("GoalObject");
            

            if(patientMedCarts.Length == 0)
            {
                Debug.Log("No carts found");;
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

            if(graspableMeds.Count == 0)
            {
                Debug.Log("No meds found");;
            }
            else
            {
                Debug.Log("Meds found");
            }
        }

        
        
        // Keyboard press enter send med goal
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SetArmToMedTarget();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            armController.MoveToAutonomyTarget();
        }

        // ScheuldeNextTask();

    }


    void OnEnable()
    {
        // Subscribe to the event

        if (baseController != null)
        {
            baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete += OnBaseReachedGoal;
        }

        if (armController != null)
        {
            armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
            armController.OnAutonomyComplete += OnArmAutonomyComplete;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up

        if (baseController != null)
        {
            baseController.OnAutonomyTrajectory -= OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete -= OnBaseReachedGoal;
        }

        if (armController != null)
        {
            armController.OnAutonomyTrajectory -= OnArmTrajectoryGenerated;
            armController.OnAutonomyComplete -= OnArmAutonomyComplete;
        }
    }

    

    private void OnBaseReachedGoal()
    {
        // Event will be called when the robot reaches the goal
        waypointReachedGoal = true;
    }

    // State Machine   //////////////////////////////////////////////////////////////////////////////////////////
    private void ScheuldeNextTask()
    {
        List<Vector3> posTraj = new List<Vector3>();
        List<Vector3> rotTraj = new List<Vector3>();
        Vector3 patientPos;

        switch (state)
        {   
            case State.SetFirstPatcient:

                // Autonomy starts here
                
                // Create the waypoints to get to the first patcient
                posTraj = new List<Vector3>(transfereWaypointPositions);
                rotTraj = new List<Vector3>(transferWaypointRotations);
                posTraj.Add(patcientPosition[(int)currentPatcient]);
                rotTraj.Add(patcientRotations[(int)currentPatcient]);
                SetWaypoints(posTraj, rotTraj);

                // Create AR for the cart nearest to the patcient
                patientPos = patcientPosition[(int)currentPatcient];
                GenerateARForNearestCart(patientPos);

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
                        // Create the waypoints to get to the pharmacy
                        posTraj = new List<Vector3>{transfereWaypointPositions[1],transfereWaypointPositions[0]} ;
                        rotTraj = new List<Vector3>{transferReturnWaypointRotations[1], transferReturnWaypointRotations[0]};
                        posTraj.Add(pharmacyPosition);
                        rotTraj.Add(pharmacyRotation);
                        SetWaypoints(posTraj, rotTraj);

                        // Change AR to indicate an issue
                        patientPos = patcientPosition[(int)currentPatcient];
                        ChangeNearestCartARColor(patientPos, Color.red);

                        state = State.GoToPharmacy;
                    }
                    else
                    {
                        // Change AR to indicate no issue
                        patientPos = patcientPosition[(int)currentPatcient];
                        ChangeNearestCartARColor(patientPos, Color.green);

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
                    GameObject med = ChooseMedToPickUp();
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
                    posTraj = new List<Vector3>(transfereWaypointPositions);
                    rotTraj = new List<Vector3>(transferWaypointRotations);
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
                    patientPos = patcientPosition[(int)currentPatcient];
                    ChangeNearestCartARColor(patientPos, Color.green);
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
                        return;
                        break;
                    default:
                        break;
                }

                // Go to the next patient
                SetWaypoints(patcientPosition[(int)currentPatcient], patcientRotations[(int)currentPatcient]);

                // Generate AR for the nearest cart
                patientPos = patcientPosition[(int)currentPatcient];
                GenerateARForNearestCart(patientPos);

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

    // Base Navigation and Autonomy Functions //////////////////////////////////////////////////////////////////////////////////////////

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

    IEnumerator FollowWaypoints()
    {
        // Debug.Log("Start Motion");s

        for(int i = 0; i < waypointPositions.Count; i++)
        {
            // Debug.Log("Going to Waypoint");
            // Reset the reached goal flag
            reachedGoal = false;
            waypointReachedGoal = false;

            // Convert from euler angles to quaternion
            Quaternion worldRotation = Quaternion.Euler(waypointRotations[i]);
            baseController.SetAutonomyTarget(waypointPositions[i], worldRotation);

            yield return new WaitUntil(() => waypointReachedGoal);
        }
        reachedGoal = true;
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
                new Vector3(0, 0, 0),
                new Vector3(0.3f, 0.1f, 0.5f),
                Color.yellow,
                0.5f
            );
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

    private GameObject GetNearestCart()
    {
        return GetNearestCart(robot.transform.position);
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

    private GameObject ChooseMedToPickUp()
    {
        // Get a med
        GameObject med = graspableMeds[0];

        // Remvove the med from the list
        graspableMeds.Remove(med);

        // Generate AR for the nearest med
        GenerateARForMed(med);

        Debug.Log("Med Chosen");

        return med;
    }


    // Arm Navigation and Autonomy Functions //////////////////////////////////////////////////////////////////////////////////////////
    private void OnArmAutonomyComplete()
    {
        Debug.Log("Arm Autonomy Complete");
    }

    private void OnArmTrajectoryGenerated()
    {
        Debug.Log("Arm trajectory generated");
    }

    private void SetArmTarget(GameObject med)
    { 
        // Assume what we are picking up is on the left of our left ee

        // Get the hoverpoint and the grasp point
        var (hoverTransform, graspingTransform) = autoGrasping.GetHoverAndGraspTransforms(med);
        Debug.Log("hoverTransform: " + hoverTransform.position + " graspingTransform: " + graspingTransform.position);
        // Sending it to just hover in front of the med
        armController.SetAutonomyTarget(hoverTransform.position, hoverTransform.rotation);
    }

    private void SetArmToMedTarget()
    {
        GameObject med = ChooseMedToPickUp();
        SetArmTarget(med);
    }



}
