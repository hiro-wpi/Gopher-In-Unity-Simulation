using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script converts linear velocity and angular velocity 
///     to articulation wheel speeds
///     for differential drive robot in simulation.
/// </summary>
public class ArticulationWheelController : MonoBehaviour
{
    // Robot wheels
    [SerializeField] private ArticulationBody leftWheel;
    [SerializeField] private ArticulationBody rightWheel;
    // parameters for Freight base
    [SerializeField] private float wheelTrackLength = 0.37476f;
    [SerializeField] private float wheelRadius = 0.0605f;
    // wheel speed
    private float leftWheelSpeed;
    private float rightWheelSpeed;
    private float leftWheelJointSpeed;
    private float rightWheelJointSpeed;

    void Start() {}

    void Update() {}

    // Update robot speed for next time step
    public void SetRobotSpeedStep(float linearSpeed, float angularSpeed)
    {
        // Compute wheel joint speeds based on given robot speeds
        if (linearSpeed != 0 || angularSpeed != 0)
        {
            leftWheelSpeed = -angularSpeed * (wheelTrackLength / 2) + linearSpeed;
            rightWheelSpeed = angularSpeed * (wheelTrackLength / 2) + linearSpeed;
            leftWheelJointSpeed = leftWheelSpeed / wheelRadius * Mathf.Rad2Deg;
            rightWheelJointSpeed = rightWheelSpeed / wheelRadius * Mathf.Rad2Deg;

            ArticulationBodyUtils.SetJointSpeedStep(leftWheel, leftWheelJointSpeed);
            ArticulationBodyUtils.SetJointSpeedStep(rightWheel, rightWheelJointSpeed);
        }
        // Directly stop the wheel if no speed is given
        else
        {
            StopWheels();
        }
    }

    public void StopWheels()
    {
        ArticulationBodyUtils.StopJoint(leftWheel);
        ArticulationBodyUtils.StopJoint(rightWheel);
    }
}
