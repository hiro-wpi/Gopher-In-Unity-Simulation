using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script sends Unity input for end-effector control 
///     to ROS as Twist message. The gripper command 
///     is set by sending a request to ROS.
///     
///     Two speed modes are available: Slow, and Regular,
///     which correspond to 0.5, 1 of the max velocity.
///     Clipping is also applied to the input.
///     Smoothing is handled by low level joint controller
///     
///     The current velocity is adjusted w.r.t. the robot base,
///     and published to ROS at a fixed rate. 
/// </summary>
public class PhysicalArmController : ArmController
{
    // Physical arm rotation offset
    // physical arm rotation w.r.t robot base, in x, y and z direction.
    [SerializeField] private Vector3 armRotationOffset;
    // Container for computing global velocity
    private Quaternion armRotationOffsetQuaternion;
    private Vector3 globalLinearVelocity;
    private Vector3 globalAngularVelocity;

    // ROS communication
    [SerializeField] private TwistCommandPublisher twistCommandPublisher;
    [SerializeField] private GripperCommandService gripperCommandService;
    [SerializeField] protected int publishRate = 60;

    void Start()
    {
        // Compute transformation w.r.t. robot base
        armRotationOffsetQuaternion = Quaternion.Euler(armRotationOffset);

        // Keep publishing the velocity at a fixed rate
        InvokeRepeating("PublishVelocity", 1.0f, 1.0f / publishRate);
    }

    void Update() {}

    // Publish the current velocity
    private void PublishVelocity()
    {
        // Convert velocities to robot base coordinate
        globalLinearVelocity = armRotationOffsetQuaternion * linearVelocity;
        // TODO: check why this is incorrect
        // globalAngularVelocity = armRotationOffsetQuaternion * angularVelocity;
        globalAngularVelocity = angularVelocity;

        // Publish to ROS
        twistCommandPublisher.PublishTwistCommand(
            globalLinearVelocity, globalAngularVelocity
        );
    }

    // Request service when gripper speed is set
    public override void SetGripperPosition(float position)
    {
        base.SetGripperPosition(position);

        // Request service
        gripperCommandService.SendGripperCommandService(gripperPosition);
    }

    // TODO
    public override void SetEndEffectorPose(Vector3 position, Quaternion rotation)
    {
        
    }

    public override void HomeJoints()
    {
        
    }

    // Autonomy function
    public override void SetAutonomyTarget(
        Vector3 position, Quaternion rotation
    )
    {

    }

    public override void CancelAutonomyTarget()
    {
        
    }

    public override void MoveToAutonomyTarget()
    {

    }

    // Send command to move to pre-defined positions
    public override void MoveToPreset(int presetIndex) 
    {

    }

    // TODO
    // Emergency Stop
    public override void EmergencyStop()
    {
        
    }
    
    // TODO
    public override void EmergencyStopResume()
    {
        
    }

    // TODO
    // Some getter functions
    public override float[] GetJointAngles()
    {
        return new float[0];
    }
    
    // Get the actual end effector pose
    public override (Vector3, Quaternion) GetEEPose()
    {
        return (Vector3.zero, Quaternion.identity);
    }

    // Get the end effector target pose
    public override (Vector3, Quaternion) GetEETargetPose()
    {
        return (Vector3.zero, Quaternion.identity);
    }

    // Compute end effector target pose given joint angle
    public override (Vector3, Quaternion) GetEETargetPose(float[] angles)
    {
        return (Vector3.zero, Quaternion.identity);
    }

    public override void StopAllJoints()
    {
        return;
    }
}
