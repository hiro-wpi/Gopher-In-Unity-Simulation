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
    // [SerializeField] private TransformSubscriber tfSubscriber;

    void Start() {}

    // void Update() {}

    public override void UpdateLocalization()
    {
        (Vector3 position, Vector3 rotation) = amclPoseSubscriber.GetPose();
        // (Vector3 position, Vector3 rotation) = tfSubscriber.GetPose();
        Position = position;
        RotationEuler = rotation;
    }
}