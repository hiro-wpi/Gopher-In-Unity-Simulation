using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script sends commands to robot base wheels
///     
///     Two speed modes are available: Slow, and Regular,
///     which correspond to 0.5, 1 of the max velocity.
///     Clipping is also applied to the input.
/// </summary>
public class ArticulationBaseController : BaseController
{
    // Emergency Stop
    [SerializeField] private bool emergencyStop = false;
    public virtual void EmergencyStop(bool stop = true)
    {
        emergencyStop = stop;
    }

    // Local wheel controller
    [SerializeField] private ArticulationWheelController wheelController;

    // Extra speed limits
    // enforced by autonomy, manipulating objects, etc.
    // [linear_forward, linear_backward, angular_left, angular_right]
    private float[] speedLimit = new[] { 100f, 100f, 100f, 100f };
    // A dictionary to store all enforced speed limits
    // ID, [linear_forward, linear_backward, angular_left, angular_right]
    private Dictionary<string, float[]> speedLimitsDict = new();

    // void Start() {}

    void FixedUpdate()
    {
        // Emergency stop
        if (emergencyStop)
        {
            wheelController.StopWheels();
            return;
        }

        wheelController.SetRobotSpeedStep(linearVelocity.z, angularVelocity.y);
    }

    public override void SetVelocity(Vector3 linear, Vector3 angular)
    {
        base.SetVelocity(linear, angular);

        // Extra speed limit
        targetLinearVelocity = Utils.ClampVector3(
            linear, -speedLimit[1], speedLimit[0]
        );
        targetAngularVelocity = Utils.ClampVector3(
            angular, -speedLimit[2], speedLimit[3]
        );
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

    // Extra speed limits for the robot
    public string AddSpeedLimit(float[] speedLimits, string identifier = "")
    {
        if (identifier == "")
            identifier = speedLimitsDict.Count.ToString();

        // Add or set new speed limits
        if (speedLimitsDict.ContainsKey(identifier))
        {
            speedLimitsDict[identifier] = speedLimits;
        }
        else
        {
            speedLimitsDict.Add(identifier, speedLimits);
        }
        UpdateSpeedLimits();

        return identifier;
    }

    public bool RemoveSpeedLimit(string identifier)
    {
        // Remove speed limits if exists
        if (speedLimitsDict.ContainsKey(identifier))
        {
            speedLimitsDict.Remove(identifier);
            UpdateSpeedLimits();
            
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateSpeedLimits()
    {
        // Convert the speed limits dict to array
        float[][] speedLimits = speedLimitsDict.Values.ToArray();

        // Find the minimal speed limits for each direction
        speedLimits = Utils.TransposeArray(speedLimits);
        for (int i = 0; i < speedLimits.Length; i++)
        {
            speedLimit[i] = speedLimits[i].Min();
        }
    }
}
