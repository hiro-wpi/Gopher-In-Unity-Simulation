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

    public ARNavigationAutomation arNavAuto;

    // Task Setup (IV Delivery)
    public Vector3 doorPosition = new Vector3(0, 0, 0);
    public Vector3 doorRotation = new Vector3(0, 0, 0);  // Euler angle to go inside the room

    public Vector3 goalPosition = new Vector3(0, 0, 0);
    public Vector3 goalRotation = new Vector3(0, 0, 0);  

    public GameObject doorFront;
    public GameObject doorBack;
    public GameObject door;

    private float doorOffset = 0.75f;
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

        // Create the door entry gameobjects
        if(door == null)
        {
            CreateDoorEntryGameObjects();
        }

        if(!arNavAuto.autoReady)
        {
            return;
        }

        UpdateRobotLocation();
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

    public void HandleChangeAutoState()
    {
        switch(controlState)
        {
            case ControlState.Manual:

                // Start Switching to Auto
                if(Input.GetKeyDown(KeyCode.Space))
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

                // Start Switching to Manual
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    controlState = ControlState.AutoToManual;
                    CancelAutonomy();
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
            posTraj = new List<Vector3>{doorPosition};
            rotTraj = new List<Vector3>{doorRotation};
            posTraj.Add(goalPosition);
            rotTraj.Add(goalRotation);
        }

        arNavAuto.SetWaypoints(posTraj, rotTraj);
    }

    private void UpdateRobotLocation()
    {
        // Check if the robot is in the room or not
        // If the robot is in the room, set the robotInRoom to true
        // If the robot is not in the room, set the robotInRoom to false

        if(!robotInRoom)
        {
            // Check the distance of the robot from the door + some other error in the same z rotation direction

            // Calculate the new position of the door
            // Vector3 doorPos = doorPosition + Quaternion.Euler(0, 0, doorRotation.z) * new Vector3(0, 0, 0.5f);
            if(DistanceToPosition(doorBack.transform.position) < doorOffset)
            {
                robotInRoom = true;
            }
        }

    }

    public float DistanceToPosition(Vector3 position)
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
        door = new GameObject();
        door.transform.position = doorPosition;
        door.transform.rotation = Quaternion.Euler(doorRotation);
        door.name = "DoorEntry";

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
