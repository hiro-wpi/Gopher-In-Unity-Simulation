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
/// <remarks>
///     As directly setting articulatio body velocity is still unstable,
///     the better practice is still to set its target position at each time step.
///     Velocity control is simulated by position control.
/// </remarks>
public class ArticulationWheelController : MonoBehaviour
{
    public ArticulationBody leftWheel;
    public ArticulationBody rightWheel;
    public float wheelTrackLength;
    public float wheelRadius;

    private float targetLinearSpeed;
    private float targetAngularSpeed;
    private float velLeft;
    private float velRight;
    
    void Start()
    {
    }

    void FixedUpdate()
    {
        SetRobotVelocityStep(targetLinearSpeed, targetAngularSpeed);
    }

    public void SetRobotVelocity(float targetLinearSpeed, float targetAngularSpeed)
    {
        this.targetLinearSpeed = targetLinearSpeed;
        this.targetAngularSpeed = targetAngularSpeed;
    }

    public void SetRobotVelocityStep(float targetLinearSpeed, float targetAngularSpeed)
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
}
