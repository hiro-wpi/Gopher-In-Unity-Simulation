using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script controls the chest joints
/// </summary>
public class ArticulationChestController : ChestController
{
    // Emergency Stop
    [SerializeField] private bool emergencyStop = false;

    // Chest joint
    [SerializeField] private ArticulationBody chestJoint;
    [SerializeField] private float maximumSpeed = 0.1f;
    private float speed = 0.0f;

    void Start() 
    {
        HomeChest();
    }

    void FixedUpdate()
    {
        // Emergency stop
        if (emergencyStop)
        {
            StopChest();
            return;
        }

        // Speed control
        if (controlMode == ControlMode.Speed)
        {
            speed = speedFraction * maximumSpeed;
            ArticulationBodyUtils.SetJointSpeedStep(chestJoint, speed);
        }
        // Position control
        else
        {
            ArticulationBodyUtils.SetJointTargetStep(chestJoint, position, maximumSpeed);
        }
    }

    public override void StopChest() 
    {
        ArticulationBodyUtils.StopJoint(chestJoint);
    }

    public override void HomeChest() 
    {
        // Home is half the range
        SetJointPosition(lowerLimit + (upperLimit - lowerLimit) / 2.0f);
    }

    public override void MoveToPreset(int presetIndex) 
    {
        SetJointPosition(preset[presetIndex]);
    }

    // Set joint position with coroutine
    private void SetJointPosition(float position) 
    {
        StartCoroutine(SetJointPositionCoroutine(position));
    }

    private IEnumerator SetJointPositionCoroutine(float position)
    {
        // Move to given position
        controlMode = ControlMode.Position;
        SetPosition(position);
        // Check if reached
        yield return new WaitUntil(() => CheckPositionReached(position) == true);
        // Switch back to velocity control
        controlMode = ControlMode.Speed;
    }

    private bool CheckPositionReached(float position)
    {
        // Check if current joint target is set to the position
        if (Mathf.Abs(chestJoint.xDrive.target - position) > 0.00001)
        {
            return false;
        }
        else
        {
            return true;
        }
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
}
