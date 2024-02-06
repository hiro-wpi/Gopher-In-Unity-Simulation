using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script sends commands to robot base wheels
///     
///     Two speed modes are available: Slow, and Regular,
///     which correspond to 0.5, 1 of the max velocity.
///     Clipping is also applied to the input.
/// </summary>
public class ArticulationBaseController : BaseController
{
    // Emergency Stop
    [SerializeField] private bool emergencyStop = false;

    // Local wheel controller
    [Header("Local Controller")]
    [SerializeField] private ArticulationWheelController wheelController;
    [Header("Autonomy")]
    [SerializeField] private AutoNavigation autoNavigation;

    // TODO
    // Base control mode
    // // manual & auto
    // private enum ControlMode { Manual, Auto }
    // [Header("Control Mode")]
    // [SerializeField, ReadOnly]
    // private ControlMode controlMode = ControlMode.Manual;

    // Automatic navigation
    [Header("Autonomy Planning")]
    [SerializeField, ReadOnly] private Vector3 targetPosition;
    [SerializeField, ReadOnly] private Quaternion targetRotation;
    // autonomy event (triggered if valid trajectory generated)
    public delegate void AutonomyTrajectoryHandler();
    public event AutonomyTrajectoryHandler OnAutonomyTrajectory;
    // autonomy event (triggered if valid trajectory generated)
    public delegate void AutonomyCompleteHandler();
    public event AutonomyCompleteHandler OnAutonomyComplete;
    // planning coroutine
    private Coroutine currentCoroutine;

    // Extra speed limits
    // enforced by autonomy, manipulating objects, etc.
    // [linear_forward, linear_backward, angular_left, angular_right]
    private float[] speedLimit = new[] { 100f, 100f, 100f, 100f };
    // A dictionary to store all enforced speed limits
    // ID, [linear_forward, linear_backward, angular_left, angular_right]
    private Dictionary<string, float[]> speedLimitsDict = new() {};

    // void Start() {}

    void FixedUpdate()
    {
        // Emergency stop
        if (emergencyStop)
        {
            wheelController.StopWheels();
            return;
        }

        // Extra speed limit
        linearVelocity = Utils.ClampVector3(
            linearVelocity, -speedLimit[1], speedLimit[0]
        );
        angularVelocity = Utils.ClampVector3(
            angularVelocity, -speedLimit[2], speedLimit[3]
        );

        // Velocity smoothing process is done 
        // in the BaseController parent class
        wheelController.SetRobotSpeedStep(
            linearVelocity.z, 
            angularVelocity.y
        );
    }

    // Autonomous mode
    public override void SetAutonomyTarget(
        Vector3 position, Quaternion rotation = new Quaternion()
    )
    {
        targetPosition = position;
        targetRotation = rotation;

        // Try to plan a path to the target
        // Debug.Log("Sending request to move to the navigation target.");
        autoNavigation.SetGoal(position, rotation, TrajectoryGenerated);
    }

    public override void CancelAutonomyTarget()
    {
        targetPosition = Vector3.zero;
        targetRotation = new Quaternion();

        autoNavigation.StopNavigation();
    }

    public override void MoveToAutonomyTarget()
    {
        autoNavigation.StartNavigation(OnAutonomyDone);
    }

    public void TrajectoryGenerated(Vector3[] positions)
    {
        // check validity of the path
        if (positions == null || positions.Length <= 1)
        {
            return;
        }

        OnAutonomyTrajectory?.Invoke();
    }

    private void OnAutonomyDone()
    {
        OnAutonomyComplete?.Invoke();
        CancelAutonomyTarget();
    }

    public (Vector3[], Vector3[]) GetTrajectories()
    {
        return (
            autoNavigation.GlobalWaypoints,
            autoNavigation.LocalWaypoints
        );
    }

    // Emergency Stop
    public override void EmergencyStop()
    {
        emergencyStop = true;
    }

    public override void EmergencyStopResume()
    {
        emergencyStop = false;
    }

    // Extra speed limits for the robot
    public string AddSpeedLimit(float[] speedLimits, string identifier = "")
    {
        if (identifier == "")
        {
            identifier = speedLimitsDict.Count.ToString();
        }

        // Add or set new speed limits
        if (speedLimitsDict.ContainsKey(identifier))
        {
            speedLimitsDict[identifier] = speedLimits;
        }
        else
        {
            speedLimitsDict.Add(identifier, speedLimits);
        }
        UpdateSpeedLimits();

        return identifier;
    }

    public bool RemoveSpeedLimit(string identifier)
    {
        // Remove speed limits if exists
        if (speedLimitsDict.ContainsKey(identifier))
        {
            speedLimitsDict.Remove(identifier);
            UpdateSpeedLimits();
            
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateSpeedLimits()
    {
        // Convert the speed limits dict to array
        float[][] speedLimits = speedLimitsDict.Values.ToArray();
        if (speedLimits.Length == 0)
        {
            speedLimit = new[] { 100f, 100f, 100f, 100f };
        }

        // Find the minimal speed limits for each direction
        speedLimits = Utils.TransposeArray(speedLimits);
        for (int i = 0; i < speedLimits.Length; i++)
        {
            speedLimit[i] = speedLimits[i].Min();
        }
    }
}
