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
    // [SerializeField] private FloorSelector floorSelector;
    // [SerializeField] private DrawWaypoints drawLocalWaypoints;
    // [SerializeField] private DrawWaypoints drawGlobalWaypoints;
    [SerializeField] private GenerateARGameObject arGenerator;
    [SerializeField] private HighlightObjectOnCanvas highlightObject;

    private bool hideLocalPath = false;
    [SerializeField] private Sprite icon1;
    [SerializeField] private Sprite icon2;
    [SerializeField] private Sprite icon3;

    // [SerializeField] private Material globalPathMaterial;
    // [SerializeField] private Material localPathMaterial;

    // Autonomy
    // [SerializeField] private ArticulationBaseController baseController;
    // [SerializeField] private ArticulationArmController armController;
    // [SerializeField] private AutoGrasping autoGrasping;
    private GameObject robot;
    // private GameObject leftRobotEE;

    // State Machines
    private enum State { SetFirstPatcient, CheckPatcient, GoToPharmacy, GetMedicine, GoDeliverMedicine, DropOffMedicine, ChangePatient, Done};
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
    // private List<Vector3> waypointPositions = new List<Vector3>();
    // private List<Vector3> waypointRotations = new List<Vector3>();
    // private List<int> waypointGripperActions = new List<int>(); // 0 for open, 1 for close

    // private bool waypointReachedGoal = false; // This is for just reaching successive points on the list
    // private bool reachedGoal = false;  // reached the whole goal

    // private bool passingInGlobalGoal = false;

     // Tracked Trajectory of the Robot Arm

    // public List<Vector3> waypointArmPositions = new List<Vector3>();
    // public List<Quaternion> waypointArmRotations = new List<Quaternion>();

    // private bool waypointArmReachedGoal = false; // This is for just reaching successive points on the list
    // private bool reachedArmGoal = false;  // reached the whole goal


    private GameObject[] patientMedCarts;

    // public GameObject[] graspableMeds;
    private List<GameObject> graspableMeds = new List<GameObject>();

    public List<bool> patientMissingMeds = new List<bool>{false, true, false, false}; 

    [SerializeField] private GameObject[] patientMedGameObjects;  // 0 load missing meds, 1 load all meds

    private GameObject pluckedMed;

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
                // floorSelector.SetCameraAndDisplay(cam, displayRect);
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
            // Get Child of the robot
            // leftRobotEE = robot.transform.Find("Plugins/Hardware/Left Arm").gameObject;

            // baseController = robot.GetComponentInChildren<ArticulationBaseController>();
            // armController = robot.GetComponentInChildren<ArticulationArmController>();
            // autoGrasping = robot.GetComponentInChildren<AutoGrasping>();
            // Constantly subscribe to the event to make our trajectory visible
            //      check if we arrive at the goal
            // baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
            // baseController.OnAutonomyComplete += OnBaseReachedGoal;

            // armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
            // armController.OnAutonomyComplete += OnArmAutonomyComplete;

            // ScheuldeNextTask();
        }

        if(patientMedCarts == null)
        {
            patientMedCarts = GameObject.FindGameObjectsWithTag("GoalObject");
            
            if(patientMedCarts.Length == 0)
            {
                Debug.Log("No carts found");;
            }
            else
            {
                Debug.Log("Carts found");
                // load the patient meds into the scene
                for(int i = 0; i < patcientPosition.Length; i++)
                {
                    LoadPatientMedPrefab(patcientPosition[i], patientMissingMeds[i]);
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

            if(graspableMeds.Count == 0)
            {
                Debug.Log("No meds found");;
            }
            else
            {
                Debug.Log("Meds found");
            }
        }

        ScheuldeNextTask();

    }


    void OnEnable()
    {
        // Subscribe to the event

        // if (baseController != null)
        // {
        //     baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
        //     baseController.OnAutonomyComplete += OnBaseReachedGoal;
        // }

        // if (armController != null)
        // {
        //     armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
        //     armController.OnAutonomyComplete += OnArmAutonomyComplete;
        // }
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up

        // if (baseController != null)
        // {
        //     baseController.OnAutonomyTrajectory -= OnBaseTrajectoryGenerated;
        //     baseController.OnAutonomyComplete -= OnBaseReachedGoal;
        // }

        // if (armController != null)
        // {
        //     armController.OnAutonomyTrajectory -= OnArmTrajectoryGenerated;
        //     armController.OnAutonomyComplete -= OnArmAutonomyComplete;
        // }
    }

    

    // private void OnBaseReachedGoal()
    // {
    //     // Event will be called when the robot reaches the goal
    //     waypointReachedGoal = true;
    // }

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
                arNavAuto.SetWaypoints(posTraj, rotTraj);

                // Create AR for the cart nearest to the patcient
                patientPos = patcientPosition[(int)currentPatcient];
                GenerateARForNearestCart(patientPos);

                state = State.CheckPatcient;

                break;

            case State.CheckPatcient:

                // Check if the patient has all their medications
                if (arNavAuto.reachedGoal)
                {
                    Debug.Log("Checking the patient");
                    // TODO: Do this actually properly
                    if (patientMissingMeds[(int)currentPatcient])
                    {   
                        // Create the waypoints to get to the pharmacy


                        posTraj = new List<Vector3>{transfereWaypointPositions[1],transfereWaypointPositions[0]} ;
                        rotTraj = new List<Vector3>{transferReturnWaypointRotations[1], transferReturnWaypointRotations[0]};
                        posTraj.Add(pharmacyPosition);
                        rotTraj.Add(pharmacyRotation);
                        arNavAuto.SetWaypoints(posTraj, rotTraj);

                        // Change AR to indicate an issue
                        patientPos = patcientPosition[(int)currentPatcient];
                        ChangeNearestCartARColor(patientPos, Color.red);
                        ChangeNearestCartCanvasHighlightIcon(patientPos, icon3);

                        state = State.GoToPharmacy;
                    }
                    else
                    {
                        // Change AR to indicate no issue
                        patientPos = patcientPosition[(int)currentPatcient];
                        ChangeNearestCartARColor(patientPos, Color.green);
                        ChangeNearestCartCanvasHighlightIcon(patientPos, icon2);

                        state = State.ChangePatient;
                    }
                    // state = State.ChangePatient;
                }
                break;

            case State.GoToPharmacy:

                // Go to the pharmacy

                // if we arrive at the pharmacy, get the medicine
                if (arNavAuto.reachedGoal)
                {
                    // GameObject med = ChooseMedToPickUp();
                    SetArmToMedTarget();
                    state = State.GetMedicine;
                }
                
                break;

            case State.GetMedicine:

                // Get the medicine
                // TODO: Implement the medicine getting
                // bool gotMedicine = true;

                // if we have the medicine, go deliver the medicine
                if (arManipAuto.reachedArmGoal)
                {
                    Debug.Log("Got Medicine");
                    arManipAuto.HomeJoints();
                    posTraj = new List<Vector3>(transfereWaypointPositions);
                    rotTraj = new List<Vector3>(transferWaypointRotations);
                    posTraj.Add(patcientPosition[(int)currentPatcient]);
                    rotTraj.Add(patcientRotations[(int)currentPatcient]);
                    arNavAuto.SetWaypoints(posTraj, rotTraj);

                    state = State.GoDeliverMedicine;
                }
                
                break;

            case State.GoDeliverMedicine:
                // Deliver the medicine
                
                // if we came back to the patcient, drop off the medicine
                if (arNavAuto.reachedGoal)
                {
                    patientPos = patcientPosition[(int)currentPatcient];
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
                        
                        arManipAuto.SetArmWaypoints(positions, rotations, gripperActions);
                    }

                    // ChangeNearestCartARColor(patientPos, Color.green);
                    state = State.DropOffMedicine;
                }
                
                break;

            case State.DropOffMedicine:
                // Drop off the medicine
                if(arManipAuto.reachedArmGoal)
                {
                    patientPos = patcientPosition[(int)currentPatcient];
                    ChangeNearestCartARColor(patientPos, Color.green);
                    ChangeNearestCartCanvasHighlightIcon(patientPos, icon2);

                    arManipAuto.HomeJoints();
                    
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

                if(state == State.Done)
                {
                    // Remove the AR for the last patient
                    StartCoroutine(RemoveARForPatient(patcientPosition[(int)currentPatcient]));
                    return;
                }

                // Go to the next patient
                arNavAuto.SetWaypoints(patcientPosition[(int)currentPatcient], patcientRotations[(int)currentPatcient]);

                // Destroy the AR of the previous patient
                StartCoroutine(RemoveARForPatient(patcientPosition[(int)currentPatcient - 1]));

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
    // private void SetWaypoints(Vector3 pos, Vector3 rot)
    // {
    //     waypointPositions = new List<Vector3>{pos};
    //     waypointRotations = new List<Vector3>{rot};

    //     StartCoroutine(FollowWaypoints());
    // }

    // Automatically Sets the waypoint and goes to it
    // private void SetWaypoints(List<Vector3> pos, List<Vector3> rot)
    // {
    //     if(pos.Count != rot.Count)
    //     {
    //         Debug.LogWarning("The list used are not the same size, they are not being added");
    //         return;
    //     }

    //     waypointPositions = pos;
    //     waypointRotations = rot;

    //     StartCoroutine(FollowWaypoints());
    // }

    // IEnumerator FollowWaypoints()
    // {
    //     // Debug.Log("Start Motion");

    //     // get the last position and rotation from waypointPositions and waypointRotations
    //     reachedGoal = false;

    //     int lastIndex = waypointPositions.Count - 1;

    //     // if there is only one waypoint, no need to show both the global and the local path, lets just show the global path
        
    //     // if(lastIndex == 0)
    //     // {
    //     //     // Hide the local path
    //     //     hideLocalPath = true;
    //     //     // drawGlobalWaypoints.RemoveLine("Global Path");
    //     // }
    //     // else
    //     // {
    //     //     hideLocalPath = false;
    //     // }

    //     // Display the global path
    //     passingInGlobalGoal = true;
    //     Debug.Log("FollowingWaypoints >> SendingGlobalPath");
    //     baseController.SetAutonomyTarget(waypointPositions[lastIndex], Quaternion.Euler(waypointRotations[lastIndex]));
    //     yield return new WaitUntil(() => passingInGlobalGoal == false);
        
    //     for(int i = 0; i < waypointPositions.Count; i++)
    //     {
    //         // Debug.Log("Going to Waypoint");
    //         // Reset the reached goal flag
    //         Debug.Log("FollowingWaypoints >> SendingLocalPaths");
    //         waypointReachedGoal = false;

    //         // Convert from euler angles to quaternion
    //         Quaternion worldRotation = Quaternion.Euler(waypointRotations[i]);
    //         baseController.SetAutonomyTarget(waypointPositions[i], worldRotation);

    //         yield return new WaitUntil(() => waypointReachedGoal);
    //     }
    //     reachedGoal = true;
    // }


    // private void OnBaseTrajectoryGenerated()
    // {
    //     // Debug.Log("Base trajectory generated");
    //     var (globalWaypoints, LocalWaypoints) = 
    //         baseController.GetTrajectories();

    //     // Generate the global path
    //     if(passingInGlobalGoal == true)
    //     {   
    //         // Clear old waypoints
    //         drawGlobalWaypoints.RemoveLine("Global Path");
    //         // Add new waypoints
    //         drawGlobalWaypoints.DrawLine("Global Path", globalWaypoints);
    //         // Debug.Log("Global Path");
    //     }
    //     else
    //     {
    //         // Debug.Log("Local Path");
        
    //         // Clear old waypoints
    //         // drawLocalWaypoints.RemoveLine("Local Path");

    //         // if(!hideLocalPath)
    //         // {
    //         //     // Add new waypoints
    //         //     drawLocalWaypoints.DrawLine("Local Path", globalWaypoints);
    //         // }

    //         // Automatically Send the robot to the goal
    //         baseController.MoveToAutonomyTarget();
    //     }
    //     passingInGlobalGoal = false;

        
    // }

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
                Color.yellow,
                0.15f
            );

        // Get the cart ar feature
        List<GameObject> arFeatures = arGenerator.GetARGameObject(nearestCart);

        if(arFeatures.Count == 1)
        {
            // Add a highlight to the AR Feature
            Debug.Log("Create AR highlight object on canvas");
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

        // Debug.Log("Med Chosen");

        return med;
    }


    // Arm Navigation and Autonomy Functions //////////////////////////////////////////////////////////////////////////////////////////
    // private void OnArmAutonomyComplete()
    // {
    //     Debug.Log("Arm Autonomy Complete");
    //     waypointArmReachedGoal = true;
    // }

    // private void OnArmTrajectoryGenerated()
    // {
    //     Debug.Log("Arm trajectory generated");
    //     armController.MoveToAutonomyTarget();
    // }

    private void SetArmToMedTarget()
    {
        pluckedMed = ChooseMedToPickUp();

        var (hoverTransform, graspingTransform) = arManipAuto.GetHoverAndGraspTransforms(pluckedMed);
        List<Vector3> positions = new List<Vector3>{hoverTransform.position, graspingTransform.position, hoverTransform.position};
        List<Quaternion> rotations = new List<Quaternion>{hoverTransform.rotation, graspingTransform.rotation, hoverTransform.rotation};
        List<int> gripperActions = new List<int>{-1, 1, -1};
        // SetArmTarget(med);
        arManipAuto.SetArmWaypoints(positions, rotations, gripperActions);
    }

    // private void SetArmWaypoints(Vector3 position, Quaternion rotation, int gripperAction = 0)
    // {
    //     SetArmWaypoints(new List<Vector3>{position}, new List<Quaternion>{rotation}, new List<int>{gripperAction});
    // }


    // private void SetArmWaypoints(List<Vector3> positions, List<Quaternion> rotations, List<int> gripperActions)
    // {
    //     if(positions.Count != rotations.Count || positions.Count != gripperActions.Count)
    //     {
    //         Debug.LogWarning("The list used are not the same size, they are not being added");
    //         return;
    //     }

    //     waypointArmPositions = positions;
    //     waypointArmRotations = rotations;
    //     waypointGripperActions = gripperActions;

    //     StartCoroutine(FollowArmWaypoints());
    // }

    // IEnumerator FollowArmWaypoints()
    // {
    //     for(int i = 0; i < waypointArmPositions.Count; i++)
    //     {
    //         // Reset the reached goal flag
    //         reachedArmGoal = false;
    //         waypointArmReachedGoal = false;

    //         // Move to the position
    //         armController.SetAutonomyTarget(waypointArmPositions[i], waypointArmRotations[i]);

    //         // Wait for when the arm reaches the position
    //         yield return new WaitUntil(() => waypointArmReachedGoal);

    //         // Set the gripper (open or closed)
    //         if(waypointGripperActions[i] != -1)
    //         {
    //             armController.SetGripperPosition(waypointGripperActions[i]);
    //         }
            
    //     }
    //     reachedArmGoal = true;

    //     Debug.Log("Finished Trajectory");
    // }

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
                Color.green,
                adjustUIScale: false,
                position: location
            );
    }

    private void ReplaceHighlightObjectOnCanvas(GameObject selectedObject, Sprite icon, Vector3 positionOffset = new Vector3())
    {
        // Find the cube
        var cube = selectedObject.transform.Find("Highlight2DObjectLocation").gameObject;

        // if(cube == null)
        // {
        //     CreateHighlightObjectOnCanvas(selectedObject, positionOffset);
        //     return;
        // }

        // Destroy the previous highlight
        highlightObject.RemoveHighlight(cube);

        // Create a new highlight
        var location = HighlightObjectOnCanvas.ElementPosition.Center;
            highlightObject.Highlight(
                cube,
                cam,
                displayRect,
                icon,
                Color.green,
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
