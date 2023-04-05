using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script converts linear velocity and angular velocity 
///     to articulation wheel speeds
///     for differential drive robot in simulation.
/// </summary>
public class ArticulationWheelController : WheelController
{
    // Robot wheels
    [SerializeField] private ArticulationBody leftWheel;
    [SerializeField] private ArticulationBody rightWheel;
    // parameters
    [SerializeField] private float wheelTrackLength;
    [SerializeField] private float wheelRadius;
    // wheel speed
    private float leftWheelSpeed;
    private float rightWheelSpeed;
    private float leftWheelJointSpeed;
    private float rightWheelJointSpeed;

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

    // Update robot speed for next time step
    private void SetRobotSpeedStep(float linearSpeed, float angularSpeed)
    {
        // Compute wheel joint speeds based on given robot speeds
        if (linearSpeed != 0 || angularSpeed != 0)
        {
            leftWheelSpeed = -angularSpeed * (wheelTrackLength / 2) + linearSpeed;
            rightWheelSpeed = angularSpeed * (wheelTrackLength / 2) + linearSpeed;
            leftWheelJointSpeed = leftWheelSpeed / wheelRadius * Mathf.Rad2Deg;
            rightWheelJointSpeed = rightWheelSpeed / wheelRadius * Mathf.Rad2Deg;

            SetWheelSpeedStep(leftWheel, leftWheelJointSpeed);
            SetWheelSpeedStep(rightWheel, rightWheelJointSpeed);
        }
        // Directly stop the wheel if no speed is given
        else
        {
            StopWheel(leftWheel);
            StopWheel(rightWheel);
        }
    }

    private void SetWheelSpeedStep(ArticulationBody wheel, float jointSpeed)
    {
        // Set joint target to the next timestep position given a speed
        ArticulationBodyUtils.SetJointSpeedStep(wheel, jointSpeed);
    }

    private void StopWheel(ArticulationBody wheel)
    {
        // Set target to current position to stop the joint
        ArticulationBodyUtils.StopJoint(wheel);
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
