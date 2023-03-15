using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
///     This script converts linear velocity and angular velocity 
///     to joint velocities for differential drive robot in simulation.
///
///     Three control modes are available: Slow, Regular, and Fast,
///     which correspond to 0.25, 0.5, 1.0 of the max velocity.
///     Clipping and smoothing are also applied to the input.
/// </summary>
/// <remarks>
///     As directly setting articulatio body velocity is still unstable,
///     the better practice is still to set its target position at each time step.
///     Velocity control is simulated by position control.
/// </remarks>
public class ArticulationWheelController : WheelController
{
    // Robot
    [SerializeField] private ArticulationBody leftWheel;
    [SerializeField] private ArticulationBody rightWheel;
    // parameters
    [SerializeField] private float wheelTrackLength;
    [SerializeField] private float wheelRadius;
    // wheel speed
    private float leftWheelSpeed;
    private float rightWheelSpeed;

    // Extra speed limits
    // enforced by autonomy, manipulating objects, etc.
    // [linear_forward, linear_backward, angular_left, angular_right]
    private float[] speedLimit = new[] { 100f, 100f, 100f, 100f };
    // A dictionary to store all enforced speed limits
    // ID, [linear_forward, linear_backward, angular_left, angular_right]
    private Dictionary<string, float[]> speedLimitsDict = new();

    // void Start() { }

    void FixedUpdate()
    {
        SetRobotSpeedStep(linearVelocity.z, angularVelocity.y);
    }


    public override void SetVelocity(Vector3 linear, Vector3 angular)
    {
        base.SetVelocity(linear, angular);

        // Extra speed limit
        targetLinearVelocity = Utils.ClampVector3(
            linear, -speedLimit[1], speedLimit[0]
        );
        targetAngularVelocity = Utils.ClampVector3(
            angular, -speedLimit[2], speedLimit[3]
        );
    }


    // Update robot speed for one time step
    private void SetRobotSpeedStep(float linearSpeed, float angularSpeed)
    {
        // Compute wheel joint velocity based on given speeds
        if (linearSpeed != 0 || angularSpeed != 0)
        {
            leftWheelSpeed = -angularSpeed * (wheelTrackLength / 2) + linearSpeed;
            rightWheelSpeed = angularSpeed * (wheelTrackLength / 2) + linearSpeed;
            SetWheelSpeedStep(leftWheel, leftWheelSpeed / wheelRadius * Mathf.Rad2Deg);
            SetWheelSpeedStep(rightWheel, rightWheelSpeed / wheelRadius * Mathf.Rad2Deg);
        }
        // Directly stop the wheel if no speed is given
        else
        {
            StopWheel(leftWheel);
            StopWheel(rightWheel);
        }
    }

    // Update wheel target for one time step
    private void SetWheelSpeedStep(ArticulationBody wheel, float jointSpeed)
    {
        ArticulationDrive drive = wheel.xDrive;
        drive.target += jointSpeed * Time.fixedDeltaTime;
        wheel.xDrive = drive;
    }

    // Set desired angle as current angle to stop the wheel
    private void StopWheel(ArticulationBody wheel)
    {
        ArticulationDrive drive = wheel.xDrive;
        drive.target = wheel.jointPosition[0] * Mathf.Rad2Deg;
        wheel.xDrive = drive;
    }


    // Extra speed limits for the robot
    public string AddSpeedLimit(float[] speedLimits, string identifier = "")
    {
        if (identifier == "")
            identifier = speedLimitsDict.Count.ToString();

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

        // Find the minimal speed limits for each direction
        speedLimits = Utils.TransposeArray(speedLimits);
        for (int i = 0; i < speedLimits.Length; i++)
        {
            speedLimit[i] = speedLimits[i].Min();
        }
    }
}
