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
    public float linearSpeedLimit = -1f;
    public float angularSpeedLimit = -1f;
    private Dictionary<string, float[]> speedLimitsMap;

    // convertion
    public float targetLinearSpeed;
    public float targetAngularSpeed;
    private float velLeft;
    private float velRight;
    
    void Start()
    {
        speedLimitsMap = new Dictionary<string, float[]>();
    }

    void FixedUpdate()
    {
        SetRobotVelocityStep(targetLinearSpeed, targetAngularSpeed);
    }

    public void SetRobotVelocity(float targetLinearSpeed, float targetAngularSpeed)
    {
        // Set target
        this.targetLinearSpeed = targetLinearSpeed;
        this.targetAngularSpeed = targetAngularSpeed;
        // Speed limit (needs to be not less than 0)
        if (linearSpeedLimit >= 0)
            this.targetLinearSpeed = 
                Mathf.Clamp(this.targetLinearSpeed, -linearSpeedLimit, linearSpeedLimit);
        if (linearSpeedLimit >= 0)
            this.targetAngularSpeed =
                Mathf.Clamp(this.targetAngularSpeed, -angularSpeedLimit, angularSpeedLimit);
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


    public string AddSpeedLimit(float linearSpeedLimit, float angularSpeedLimit, 
                                string identifier = "")
    {
        if (identifier == "")
            identifier = speedLimitsMap.Count.ToString();
        
        // set speed limits
        if (speedLimitsMap.ContainsKey(identifier))
            speedLimitsMap[identifier] = new float[] {linearSpeedLimit, angularSpeedLimit};
        else
            speedLimitsMap.Add(identifier, new float[] {linearSpeedLimit, angularSpeedLimit});
        
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
            linearSpeedLimit = -1f;
            angularSpeedLimit = -1f;
        }
        // Find the minimal limits
        else
        {
            linearSpeedLimit = float.PositiveInfinity;
            angularSpeedLimit = float.PositiveInfinity;
            foreach(KeyValuePair<string, float[]> entry in speedLimitsMap)
            {
                // Value smaller than 0 will be ignored 
                // considered as no limit
                if (entry.Value[0] >= 0 && entry.Value[0] < linearSpeedLimit)
                    linearSpeedLimit = entry.Value[0];
                if (entry.Value[1] >= 0 && entry.Value[1] < linearSpeedLimit)
                    angularSpeedLimit = entry.Value[1];
            }
        } 
    }
}
