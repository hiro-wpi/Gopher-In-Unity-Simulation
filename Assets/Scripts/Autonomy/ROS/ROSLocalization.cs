using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Localization using ROS package AMCL
/// </summary>
public class ROSLocalization : Localization
{
    // AMCL results
    [SerializeField] private AMCLPoseSubscriber amclPoseSubscriber;
    [SerializeField] private TFListenerSubscriber tfSubscriber;

    private Vector3 position;
    private Vector3 rotation;

    void Start() 
    {
        position = Vector3.zero;
        rotation = Vector3.zero;
    }

    // void Update() {}

    public override void UpdateLocalization()
    {
        // (Vector3 position, Vector3 rotation) = amclPoseSubscriber.GetPose();
        (position, rotation) = tfSubscriber.GetPose();
        Position = position;
        RotationEuler = rotation;
    }
}
