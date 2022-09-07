using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script converts linear velocity and angular velocity 
///     to joint velocities for differential drive robot.
///     Velocity can be set with SetRobotVelocity().
/// </summary>
/// <remarks>
///     As directly setting articulatio body velocity is still unstable,
///     the better practice is still to set its target position at each time step.
///     Velocity control is simulated by position control.
/// </remarks>
public class ArticulationWheelController : MonoBehaviour
{
    // robot
    public ArticulationBody leftWheel;
    public ArticulationBody rightWheel;
    public float wheelTrackLength;
    public float wheelRadius;

    // hard speed limit
    private float linearSpeedLimitForward = 100f;
    private float linearSpeedLimitBackward = 100f;
    private float angularSpeedLimitLeft = 100f;
    private float angularSpeedLimitRight = 100f;
    // ID, [linear_forward, linear_backward, angular_left, angular_right]
    private Dictionary<string, float[]> speedLimitsMap = 
        new Dictionary<string, float[]>();

    // convertion
    private float targetLinearSpeed;
    private float targetAngularSpeed;
    private float velLeft;
    private float velRight;
    

    void Start()
    {}

    void FixedUpdate()
    {
        SetRobotVelocityStep(targetLinearSpeed, targetAngularSpeed);
    }


    public void SetRobotVelocity(float linearSpeed, float angularSpeed)
    {
        // Set target
        targetLinearSpeed = linearSpeed;
        targetAngularSpeed = angularSpeed;
        // Speed limit (needs to be not less than 0)
        if (linearSpeedLimitForward >= 0)
            targetLinearSpeed = 
                Mathf.Clamp(targetLinearSpeed, -100, linearSpeedLimitForward);
        if (linearSpeedLimitBackward >= 0)
            targetLinearSpeed = 
                Mathf.Clamp(targetLinearSpeed, -linearSpeedLimitBackward, 100);
        if (angularSpeedLimitLeft >= 0)
            targetAngularSpeed =
                Mathf.Clamp(targetAngularSpeed, -100, angularSpeedLimitLeft);
        if (angularSpeedLimitRight >= 0)
            targetAngularSpeed =
                Mathf.Clamp(targetAngularSpeed, -angularSpeedLimitRight, 100);
    }

    private void SetRobotVelocityStep(float targetLinearSpeed, float targetAngularSpeed)
    {
        if (targetLinearSpeed != 0 || targetAngularSpeed != 0)
        {
            // Compute wheel joint velocity based on given speed
            velLeft = -targetAngularSpeed * (wheelTrackLength / 2) + targetLinearSpeed;
            velRight = targetAngularSpeed * (wheelTrackLength / 2) + targetLinearSpeed;
            SetWheelVelocityStep(leftWheel, velLeft / wheelRadius * Mathf.Rad2Deg);
            SetWheelVelocityStep(rightWheel, velRight / wheelRadius * Mathf.Rad2Deg);
        }
        else
        {
            StopWheel(leftWheel);
            StopWheel(rightWheel);
        }
    }
    

    private void SetWheelVelocityStep(ArticulationBody wheel, float jointSpeed)
    {
        ArticulationDrive drive = wheel.xDrive;
        drive.target = drive.target + jointSpeed * Time.fixedDeltaTime;
        wheel.xDrive = drive;
    }

    private void StopWheel(ArticulationBody wheel)
    {
        // Set desired angle as current angle to stop the wheel
        ArticulationDrive drive = wheel.xDrive;
        drive.target = wheel.jointPosition[0] * Mathf.Rad2Deg;
        wheel.xDrive = drive;
    }


    public string AddSpeedLimit(float[] speedLimits, string identifier = "")
    {
        if (identifier == "")
            identifier = speedLimitsMap.Count.ToString();
        
        // set speed limits
        if (speedLimitsMap.ContainsKey(identifier))
        {
            if (speedLimitsMap[identifier] == speedLimits)
                return identifier;
            else
                speedLimitsMap[identifier] = speedLimits;
        }
        else
            speedLimitsMap.Add(identifier, speedLimits);
        
        UpdateSpeedLimits();
        return identifier;
    }
    public bool RemoveSpeedLimit(string identifier)
    {
        // remove speed limits
        if (speedLimitsMap.ContainsKey(identifier)) 
        {
            speedLimitsMap.Remove(identifier);
            UpdateSpeedLimits();
            return true;
        }
        else
            return false;
    }
    private void UpdateSpeedLimits()
    {
        // No speed limits
        if (speedLimitsMap.Count == 0)
        {
            linearSpeedLimitForward = 100f;
            linearSpeedLimitBackward = 100f;
            angularSpeedLimitLeft = 100f;
            angularSpeedLimitRight = 100f;
        }
        // Find the minimal limits
        else
        {
            linearSpeedLimitForward = 100f;
            linearSpeedLimitBackward = 100f;
            angularSpeedLimitLeft = 100f;
            angularSpeedLimitRight = 100f;
            foreach(KeyValuePair<string, float[]> entry in speedLimitsMap)
            {
                if (entry.Value[0] < linearSpeedLimitForward)
                    linearSpeedLimitForward = entry.Value[0];
                if (entry.Value[1] < linearSpeedLimitBackward)
                    linearSpeedLimitBackward = entry.Value[1];
                if (entry.Value[2] < angularSpeedLimitLeft)
                    angularSpeedLimitLeft = entry.Value[2];
                if (entry.Value[3] < angularSpeedLimitRight)
                    angularSpeedLimitRight = entry.Value[3];

                // Make sure they are larger than 0
                linearSpeedLimitBackward = Mathf.Clamp(linearSpeedLimitBackward, 0f, 100f);
                linearSpeedLimitForward = Mathf.Clamp(linearSpeedLimitForward, 0f, 100f);
                angularSpeedLimitLeft = Mathf.Clamp(angularSpeedLimitLeft, 0f, 100f);
                angularSpeedLimitRight = Mathf.Clamp(angularSpeedLimitRight, 0f, 100f);
            }
        } 
    }
}
