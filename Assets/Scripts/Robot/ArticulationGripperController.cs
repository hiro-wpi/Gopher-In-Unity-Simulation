using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script is used to control the robotic
///     end-effector. This script is specifically used 
///     for robotic 2F-85. But others can also be used
///     with minor modification.
/// </summary>
public class ArticulationGripperController : MonoBehaviour
{

    // New class to hold the articulation body and close/open values
    [System.Serializable]
    public class ArticulationGripper
    {
        public ArticulationBody articulationBody;
        public float closeValue;
        public float openValue;
    }

    public ArticulationGripper[] leftFingerChain;
    public ArticulationGripper[] rightFingerChain;

    void Start()
    {
    }

    public void SetGrippers(float value)
    {
        for (int i = 0; i < leftFingerChain.Length; ++i)
        {
            float leftValue = Mathf.Lerp(leftFingerChain[i].openValue, leftFingerChain[i].closeValue, value);
            SetTarget(leftFingerChain[i].articulationBody, leftValue);

            float rightValue = Mathf.Lerp(rightFingerChain[i].openValue, rightFingerChain[i].closeValue, value);
            SetTarget(rightFingerChain[i].articulationBody, rightValue);
        }
    }

    void SetTarget(ArticulationBody joint, float target)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.target = target;
        joint.xDrive = drive;
    }
}