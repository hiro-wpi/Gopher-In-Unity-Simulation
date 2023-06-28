using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Localization using ROS package AMCL
/// </summary>
public class ROSLocalization : Localization
{
    // Localization Results
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
        (position, rotation) = tfSubscriber.GetPose();
        Position = position;
        RotationEuler = rotation;
    }
}
