using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script initializes the articulation bodies by 
///     setting stiffness, damping and force limit of
///     the non-fixed ones.
/// </summary>
public class ArticulationGripperController : MonoBehaviour
{
    [System.Serializable]
    private class FingerJoint {
        public ArticulationBody body;
        public float target;
    }

    [SerializeField]
    private FingerJoint[] fingerJoints;

    void Start()
    {
    }

    public void SetGrippers(float closeValue) {
        foreach (FingerJoint fj in fingerJoints) {
            SetTarget(fj.body, closeValue * fj.target);
        }
    }

    void SetTarget(ArticulationBody joint, float target)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.target = target;
        joint.xDrive = drive;
    }

    public void StopJoint(ArticulationBody joint)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.target = joint.jointPosition[0];
        joint.xDrive = drive;
    }
}