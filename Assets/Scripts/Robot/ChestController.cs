using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     An abstract class to handle Unity input for chest control.
///     For speed control, chest control takes speed fraction as input,
///     which is defined as the ratio of the maximum avaliable speed.
///
///     The input (speedFraction or position) would be handled differently
///     for simulation robot or physical robot
/// </summary>
public abstract class ChestController : MonoBehaviour
{
    // Control parameters
    [SerializeField] protected float speedFractionMultiplier = 1.0f;
    [SerializeField] protected float lowerLimit = 0.0f;
    [SerializeField] protected float upperLimit = 0.44f;

    // Arm control mode
    public enum ControlMode { Speed, Position }
    protected ControlMode controlMode = ControlMode.Speed;

    // Variable to hold speed fraction
    [SerializeField, ReadOnly] protected float speedFraction = 0.0f;
    [SerializeField, ReadOnly] protected float position = 0.0f;

    // Preset positions: low, home, high
    [SerializeField] protected float[] preset = {0.0f, 0.22f, 0.44f};

    void Start() {}

    void Update() {}

    public virtual void SetSpeedFraction(float fraction)
    {
        
        speedFraction = Mathf.Clamp(
            fraction * speedFractionMultiplier, -1.0f, 1.0f
        );
        Debug.Log("ChestController << SetSpeedFraction Function");
        // Debug.Log(speedFraction);

    }

    public virtual void SetPosition(float pos)
    {
        position = Mathf.Clamp(pos, lowerLimit, upperLimit);
    }

    public virtual void SetControlMode(ControlMode mode)
    {
        controlMode = mode;
    }

    public virtual void StopChest() {}

    public virtual void HomeChest() {}

    public virtual void MoveToPreset(int presetIndex) {}

    // Do Not Use 
    // -> Issue with Intergration
    //    See GopherControl.cs function OnChestBreaker
    public virtual void ChestBreaker(float value) {}
    
}
