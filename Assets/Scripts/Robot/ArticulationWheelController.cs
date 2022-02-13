using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script converts linear velocity and 
///     angular velocity to joint velocities for
///     differential drive robot. Velocity can be 
///     set with SetRobotVelocity().
/// </summary>
public class ArticulationWheelController : MonoBehaviour
{
    public ArticulationBody leftWheel;
    public ArticulationBody rightWheel;
    public float wheelTrackLength;
    public float wheelRadius;

    private float vRight;
    private float vLeft;

    void Start()
    {
    }

    void Update()
    {
    }

    public void SetRobotVelocity(float targetLinearSpeed, float targetAngularSpeed)
    {
        // Stop the wheel if target velocity is 0
        if (targetLinearSpeed == 0 && targetAngularSpeed == 0)
        {
            StopWheel(leftWheel);
            StopWheel(rightWheel);
        }
        else
        {
            // Convert from linear x and angular z velocity to wheel speed
            vRight = targetAngularSpeed*(wheelTrackLength/2) + targetLinearSpeed;
            vLeft = -targetAngularSpeed*(wheelTrackLength/2) + targetLinearSpeed;

            SetWheelVelocity(leftWheel, vLeft / wheelRadius * Mathf.Rad2Deg);
            SetWheelVelocity(rightWheel, vRight / wheelRadius * Mathf.Rad2Deg);
        }
    }

    private void SetWheelVelocity(ArticulationBody wheel, float jointSpeed)
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
}
