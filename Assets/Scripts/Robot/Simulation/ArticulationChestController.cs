using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script controls the chest joints
/// </summary>
public class ArticulationChestController : ChestController
{
    public ArticulationBody chestJoint;
    
    // limits of the stand, located at each soft limit
    private float lowerLimit = 0.0f; // meters
    private float upperLimit = 0.44f; // meters
    // public TwistPublisher twistPublisher;

    private float velocityLimit = 0.1f;  // the absolute max speed going up or down
    // private float accelerationLimit = 0.3f;

    public float targetPosition;

    private float homePosition = 0.44f;

    public float targetSpeed;

    // private float[] preset = {0.0f, 0.22f, 0.44f};

    // public float velFraction = 0.0f;

    //sim
    void Start()
    {
        targetPosition = lowerLimit;
        targetSpeed = velocityLimit;
        
        // HomeChestJoint();
    }

    //sim
    void FixedUpdate()
    {
                                    
        // Vector3 vel = new Vector3(0, velFraction, 0);
        // twistPublisher.PublishTwist(vel, new Vector3(0,0,0));
        SetJointTargetStep(chestJoint, targetPosition, targetSpeed);
    }

    // public void testPositionControl(float input)
    // {
    //     // if(input == 0.0f) StopChest();
    //     if(input < 0.0f) absolutePositionControl(homePosition, 0.5f);
    //     else if(input > 0.0f) relativePositionControl(-0.1f, 1.0f);
    // }

    //asbtract
    public void velocityControl(float velFraction_)
    {
        velFraction = velFraction_;
        
        if(velFraction_ == 0.0f)
        {
            StopChest();
        }
        else if(velFraction_ < 0.0f) //negative
        {
            SetChestJoint(lowerLimit, Mathf.Abs(velFraction_)*velocityLimit);
        }
        else if(velFraction_ > 0.0f)
        {
            SetChestJoint(upperLimit, Mathf.Abs(velFraction_)*velocityLimit);
        }
    }


    public void absolutePositionControl(float position, float velFraction_)
    {
        SetChestJoint(position, Mathf.Abs(velFraction_)*velocityLimit);
    }

    public void relativePositionControl(float relativePosition, float velFraction_)
    {
        float pos = GetChestJoint();
        SetChestJoint(relativePosition + pos, Mathf.Abs(velFraction_)*velocityLimit);
    }

    public override void MoveToPreset(int presetIndex)
    {
        SetChestJoint(preset[presetIndex-1], velocityLimit);
    }

    public override void StopChest()
    {
        SetChestJoint(GetChestJoint());
    }

    // sim
    public void SetChestJoint(float position, float speed = 0.0f)
    {
        // Clamp
        targetPosition = Mathf.Clamp(position, lowerLimit, upperLimit);
        targetSpeed = Mathf.Clamp(speed, 0.0f, velocityLimit);
    }

    // sim
    public float GetChestJoint()
    {
        return chestJoint.xDrive.target;
    }

    // sim
    // Home joint
    public override void HomeChest()
    {
        Debug.Log("Homing in Progress");
        StartCoroutine(HomeChestJointCoroutine());
    }
    private IEnumerator HomeChestJointCoroutine()
    {
        yield return new WaitUntil(() => HomeChestAndCheck() == true);
    }
    private bool HomeChestAndCheck()
    {
        // SetCameraJoints(yawOffset, pitchOffset, velocityLimit);
        SetChestJoint(homePosition, velocityLimit);

        bool homed = Mathf.Abs(chestJoint.xDrive.target - homePosition) < 0.001;
        
        if(homed)
        {
            Debug.Log("Finished homing Chest");
            SetChestJoint(homePosition, 0.0f);
        }

        return homed;
    }

    // sim
    // Utils
    private void SetJointTargetStep(ArticulationBody joint, float target, float speed)
    {
        if (float.IsNaN(target))
            return;
        
        // Get drive
        ArticulationDrive drive = joint.xDrive;
        float currentTarget = drive.target;

        // Speed limit
        float deltaPosition = speed * Time.fixedDeltaTime;
        if (Mathf.Abs(currentTarget - target) > deltaPosition)
            target = currentTarget + deltaPosition * Mathf.Sign(target-currentTarget);

        // Joint limit
        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            if (target > drive.upperLimit)
                target = drive.upperLimit;
            else if (target < drive.lowerLimit)
                target = drive.lowerLimit;
        }

        // Set target
        drive.target = target;
        joint.xDrive = drive;
    }
}
