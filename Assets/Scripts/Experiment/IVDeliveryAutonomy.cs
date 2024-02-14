using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IVDeliveryAutonomy : MonoBehaviour
{
    // Test Setup (General)
    // [SerializeField] private GraphicalInterface graphicalInterface;
    // [SerializeField] private RectTransform displayRect;
    // private Camera cam;
    private GameObject robot;

    // Autonomy

    // State to Determine Robot Location (In or out of Room)
    public bool robotInRoom = false;

    // State: Auto or Manual Control
    enum ControlState {Manual, ManualToAuto, Auto, AutoToManual};
    ControlState controlState = ControlState.Manual;

    enum ObsticleState {None, Detected, Handled};
    ObsticleState obsticleState = ObsticleState.None;

    public ARNavigationAutomation arNavAuto;

    // Task Setup (IV Delivery)

    public Vector3 goalPosition = new Vector3(0, 0, 0);
    public Vector3 goalRotation = new Vector3(0, 0, 0);  

    // Task Objects  //////
    // Door Entry
    private GameObject doorFront;
    private GameObject doorBack;
    private GameObject door;
    private float doorOffset = 0.75f;

    // Obsticle
    private GameObject obsticle;  // For now, we are using the cart as an obsticle
    private Vector3 obsticleInitPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get the main camera if it is not already set
        // if (cam == null)
        // {
        //     // Get all the active cameras referanced in the graphical interface
        //     Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();
        //     if (cameras.Length > 0)
        //     {
        //         cam = cameras[0];
        //     }
        // }

        if(robot == null)
        {
            robot = GameObject.Find("Gopher(Clone)");

            
            if(robot == null)
            {
                // Debug.Log("No robot found");
                return;
            }
        }

        InitTaskSetup();

        if(!arNavAuto.autoReady)
        {
            return;
        }

        UpdateRobotLocation();
        HandleObsticleState();
        HandleChangeAutoState();

        
        // Start Any Task Related Stuff Here
        // if we hit the space button, send the door position to the robot
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     List<Vector3> posTraj = new List<Vector3>();
        //     List<Vector3> rotTraj = new List<Vector3>();

        //     // Create the waypoints to get to the first patient
        //     // MAYBE TODO Instead of pass in the door position, pass 2 positions, slighlty infront and behind the door
            
        //     posTraj = new List<Vector3>{doorPosition};
        //     rotTraj = new List<Vector3>{doorRotation};
        //     posTraj.Add(goalPosition);
        //     rotTraj.Add(goalRotation);
        //     arNavAuto.SetWaypoints(posTraj, rotTraj);
        // }

    }

    public void InitTaskSetup()
    {
        // Make Sure We have all the necessary task gameobject to be able to start the task

        // Create the door entry gameobjects
        if(door == null)
        {
            door = GameObject.Find("DoorEntry(Clone)");
            
            if(door == null)
            {
                return;
            }
            else
            {
                CreateDoorEntryGameObjects();
            }
        }

        // Find the cart
        if(obsticle == null)
        {
            obsticle = GameObject.Find("Service Cart Movable(Clone)");
            // Save the initial position of the obsticle for later use
            // obsticleInitPosition = obsticle.transform.position;

            if(obsticle == null)
            {
                return;
            }
            else
            {
                obsticleInitPosition = obsticle.transform.position;
            }
        }

        // TODO Later Find the goal position

    }

    public void HandleChangeAutoState()
    {
        switch(controlState)
        {
            case ControlState.Manual:

                // Start Switching to Auto
                // If the space button is pressed and there is no obsticle detected
                if(Input.GetKeyDown(KeyCode.Space) && obsticleState != ObsticleState.Detected)
                {
                    controlState = ControlState.ManualToAuto;
                    SetTaskWaypoints();
                }
                
                break;
            case ControlState.ManualToAuto:
                if(Input.GetKeyUp(KeyCode.Space))
                {
                    controlState = ControlState.Auto;
                }
                break;
            case ControlState.Auto:

                // Automatically Swtich to Manual if the obsticle is detected
                if(obsticleState == ObsticleState.Detected)
                {
                    controlState = ControlState.Manual;  // No need to transition to ManualToAuto
                    CancelAutonomy();
                }
                else
                {
                    // Start Switching to Manual
                    if(Input.GetKeyDown(KeyCode.Space))
                    {
                        controlState = ControlState.AutoToManual;
                        CancelAutonomy();
                    }
                }

                break;
            case ControlState.AutoToManual:
                if(Input.GetKeyUp(KeyCode.Space))
                {
                    controlState = ControlState.Manual;
                }
                break;  
        }
        // Change the state of the robot to Auto
        // arNavAuto.autoReady = true;
    }

    public void HandleObsticleState()
    {
        switch(obsticleState)
        {
            case ObsticleState.None:
                // Change the state if the robot is close to the obsticle
                if(RobotDistanceToPosition(obsticle.transform.position) < 1.5f)
                {
                    Debug.LogWarning("Obsticle Detected");
                    obsticleState = ObsticleState.Detected;
                }
                break;

            case ObsticleState.Detected:

                // Change the state if the obsticle is moved
                if(Vector3.Distance(obsticle.transform.position, obsticleInitPosition) > 0.5f)
                {
                    Debug.LogWarning("Obsticle Handled");
                    obsticleState = ObsticleState.Handled;
                }
                break;

            default:
                break;
        }
    }
    

    public void CancelAutonomy()
    {
        arNavAuto.CancelAutonomy();
    }

    public void SetTaskWaypoints()
    {
        // Based on the robot's location, set the waypoints to the door and goal or the goal
        List<Vector3> posTraj = new List<Vector3>();
        List<Vector3> rotTraj = new List<Vector3>();

        if(robotInRoom)
        {
            posTraj = new List<Vector3>{goalPosition};
            rotTraj = new List<Vector3>{goalRotation};
        }
        else
        {
            posTraj = new List<Vector3>{door.transform.position};
            rotTraj = new List<Vector3>{door.transform.rotation.eulerAngles};
            posTraj.Add(goalPosition);
            rotTraj.Add(goalRotation);
        }

        arNavAuto.SetWaypoints(posTraj, rotTraj);
    }

    private void UpdateRobotLocation()
    {
        // Use a raycast from the robot to the door to determine if we need to change state
        // Create a ray from the robot to the door, dont check anything past the door
        RaycastHit hit;
        Vector3 direction = door.transform.position - robot.transform.position;
        float maxDistance = Vector3.Distance(door.transform.position, robot.transform.position);

        // Debug.DrawRay(robot.transform.position + Vector3.up * 1.5f , direction,  Color.red);

        if(!Physics.Raycast(robot.transform.position + Vector3.up * 1.5f, direction, out hit, maxDistance))
        {
            // No obsticle detected between the robot and the door
            // Check if the robot is closer to the doorfront or doorback to determine if the robot is in the room or not
            float distanceFront = RobotDistanceToPosition(doorFront.transform.position);
            float distanceBack = RobotDistanceToPosition(doorBack.transform.position);

            if(distanceFront < distanceBack)
            {
                robotInRoom = false;
            }
            else
            {
                robotInRoom = true;
            }
        }
    }

    public float RobotDistanceToPosition(Vector3 position)
    {
        // Get the distance from the robot to the given position
        return Vector3.Distance(robot.transform.position, position);
    }

    public void CreateDoorEntryGameObjects()
    {
        // Creates Two GameObjects infront and behind the door
        // Assume the Z axis is pointed into the room

        Vector3 offset = new Vector3(0, 0, doorOffset);

        // Create the door gameobject
        // door = new GameObject();
        // door.transform.position = doorPosition;
        // door.transform.rotation = Quaternion.Euler(doorRotation);
        // door.name = "DoorEntry";

        // Create the front and back door gameobjects
        doorFront = new GameObject();
        doorFront.transform.parent = door.transform;
        doorFront.transform.localPosition = -offset;
        doorFront.name = "DoorFront";
        
        doorBack = new GameObject();
        doorBack.transform.parent = door.transform;
        doorBack.transform.localPosition = offset;
        doorBack.name = "DoorBack";

    }


}
