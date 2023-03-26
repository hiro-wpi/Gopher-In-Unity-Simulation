using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script controls the chest joints
/// </summary>
public abstract class ChestController : MonoBehaviour
{
    // public ArticulationBody chestJoint;
    
    // limits of the stand, located at each soft limit
    // private float lowerLimit = 0.0f; // meters
    // private float upperLimit = 0.44f; // meters

    private float multiplier = 1.0f; // mulitpler for the vel
    
    // public TwistPublisher twistPublisher;

    // private float velocityLimit = 0.1f;  // the absolute max speed going up or down
    // private float accelerationLimit = 0.3f;

    // public float targetPosition;

    // private float homePosition = 0.44f;

    // public float targetSpeed;

    public float[] preset = {0.0f, 0.22f, 0.44f};

    public float velFraction = 0.0f;
    

    void Start() {}

    void FixedUpdate() {}

    //asbtract
    public void VelocityControl(float velFraction_)
    {
        Debug.Log("Reached");
        
        velFraction =  Mathf.Clamp(velFraction_*multiplier, -1.0f, 1.0f);

        Debug.Log(velFraction);
    }


    // public abstract void absolutePositionControl(float position, float velFraction_)
    // {
    //     SetChestJoint(position, Mathf.Abs(velFraction_)*velocityLimit);
    // }

    // public void relativePositionControl(float relativePosition, float velFraction_)
    // {
    //     float pos = GetChestJoint();
    //     SetChestJoint(relativePosition + pos, Mathf.Abs(velFraction_)*velocityLimit);
    // }

    // public void MoveToPreset(int presetIndex)
    // {
    //     SetChestJoint(preset[presetIndex-1], velocityLimit);
    // }

    public abstract void HomeChest();

    public abstract void StopChest();

    public abstract void MoveToPreset(int presetIndex);

}
