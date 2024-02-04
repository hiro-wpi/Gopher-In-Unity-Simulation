using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.UrdfImporter;
using UnityEngine;

public class TestAutonomy : MonoBehaviour
{
    [SerializeField] private GenerateARGameObject arGenerator;
    [SerializeField] private DrawWaypoints drawWaypoints;
    private GameObject armWaypointParent;

    [SerializeField] private ObjectSelector objectSelector;
    [SerializeField] private FloorSelector floorSelector;

    [SerializeField] private ArticulationBaseController baseController;
    [SerializeField] private ArticulationArmController armController;
    [SerializeField] private AutoGrasping autoGrapsing;

    private GameObject robot;

    void Start()
    {
        armWaypointParent = new GameObject("Arm Waypoints");
        armWaypointParent.transform.SetParent(transform);
        armWaypointParent.transform.localPosition = Vector3.zero;
        armWaypointParent.transform.localRotation = Quaternion.identity;
    }

    void OnEnable()
    {
        // Subscribe to the event
        floorSelector.OnFloorSelected += OnFloorSelected;
        objectSelector.OnObjectSelected += OnObjectSelected;
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up
        floorSelector.OnFloorSelected -= OnFloorSelected;
        objectSelector.OnObjectSelected -= OnObjectSelected;

        if (baseController != null)
        {
            baseController.OnAutonomyTrajectory -= OnBaseTrajectoryGenerated;
        }
        if (armController != null)
        {
            armController.OnAutonomyTrajectory -= OnArmTrajectoryGenerated;
        }
    }

    private void OnFloorSelected(Vector3 position, Quaternion rotation)
    {
        if (robot == null)
        {
            return;
        }

        baseController.SetAutonomyTarget(position, rotation);
    }

    private void OnObjectSelected(GameObject gameObject, Vector3 position)
    {
        if (robot == null || gameObject == null)
        {
            return;
        }

        // Don't use this
        // armController.SetAutonomyTarget(
        //     gameObject.transform.position, 
        //     gameObject.transform.rotation
        // );
        autoGrapsing.SetTargetObject(gameObject);
    }

    private void OnBaseTrajectoryGenerated()
    {
        var (globalWaypoints, LocalWaypoints) = 
            baseController.GetTrajectories();
        
        // Clear old waypoints
        drawWaypoints.RemoveLine("Global Path");
        // Add new waypoints
        drawWaypoints.DrawLine("Global Path", globalWaypoints);
    }

    private void OnArmTrajectoryGenerated()
    {
        var (time, angles, velocities, accelerations) = 
            armController.GetAutonomyTrajectory();

        // Clear old waypoints
        foreach (Transform child in armWaypointParent.transform)
        {
            arGenerator.Destroy(child.gameObject);
            Destroy(child.gameObject);
        }

        // Add new waypoints
        foreach (var angle in angles)
        {
            GameObject waypoint = new GameObject("Waypoint");
            waypoint.transform.SetParent(armWaypointParent.transform);

            (waypoint.transform.position, waypoint.transform.rotation) =
                armController.GetEETargetPose(angle);

            arGenerator.Instantiate(
                waypoint,
                GenerateARGameObject.ARObjectType.Cube,
                scale: Vector3.one * 0.05f
            );
        }
    }

    void Update()
    {
        if (robot == null)
        {
            robot = GameObject.Find("Gopher(Clone)");
            if (robot != null)
            {
                baseController = 
                    robot.GetComponentInChildren<ArticulationBaseController>();
                armController =
                    robot.GetComponentInChildren<ArticulationArmController>();
                autoGrapsing = 
                    robot.GetComponentInChildren<AutoGrasping>();

                baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
                armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
            }
        }

        // Keyboard press space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            armController.MoveToAutonomyTarget();
        }

        // Keyboard press enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            baseController.MoveToAutonomyTarget();
        }
    }
}
