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
    public virtual void EmergencyStop(bool stop = true)
    {
        emergencyStop = stop;
    }

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

        Debug.Log(chestJoint.jointVelocity[0]);

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
        ArticulationBodyUtils.StopJoint(chestJoint, true);
    }

    public override void HomeChest() 
    {
        // Home is half the range
        SetJointPosition(upperLimit / 2.0f);
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
        controlMode = ControlMode.Position;
        SetPosition(position);
        
        yield return new WaitUntil(() => CheckPositionReached(position) == true);
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
}
