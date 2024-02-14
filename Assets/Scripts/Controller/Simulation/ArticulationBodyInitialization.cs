using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script initializes the articulation bodies by 
///     setting stiffness, damping and force limit of
///     the non-fixed ones.
/// </summary>
/// <remarks>
///     As directly setting articulatio body velocity is still unstable,
///     the better practice is still to set its target position.
///     So, all joints are still initialized for position control.
/// </remarks>
public class ArticulationBodyInitialization : MonoBehaviour
{
    // Assign to all children
    [Header("Assign to all joints")]
    [SerializeField] private GameObject robotRoot;
    [SerializeField] private float stiffness = 10000f;
    [SerializeField] private float damping = 100f;
    [SerializeField] private float forceLimit = 1000f;
    private ArticulationBody[] articulationChain;

    // Assign to specific joints
    [System.Serializable]
    public struct JointsSetting
    {
        public ArticulationBody[] joints;
        public float stiffness;
        public float damping;
        public float forceLimit;
    }
    [Header("Assign to specific joints")]
    [SerializeField] private JointsSetting[] specificJointsSettings;

    private void Awake()
    {
        // Get non-fixed joints
        articulationChain = robotRoot.GetComponentsInChildren<ArticulationBody>();
        articulationChain = articulationChain.Where(
            joint => joint.jointType != ArticulationJointType.FixedJoint
        ).ToArray();

        // Setting stiffness, damping and force limit
        // for all joints
        const int friction = 100;
        for (var i = 0; i < articulationChain.Length; ++i)
        {
            ArticulationBody joint = articulationChain[i];
            ArticulationDrive drive = joint.xDrive;

            joint.jointFriction = friction;
            joint.angularDamping = friction;

            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = forceLimit;
            joint.xDrive = drive;
        }

        // Setting stiffness, damping and force limit
        // for specific joints
        foreach (var setting in specificJointsSettings)
        {
            for (var i = 0; i < setting.joints.Length; ++i)
            {
                ArticulationBody joint = setting.joints[i];
                ArticulationDrive drive = joint.xDrive;

                joint.jointFriction = friction;
                joint.angularDamping = friction;

                drive.stiffness = setting.stiffness;
                drive.damping = setting.damping;
                drive.forceLimit = setting.forceLimit;
                joint.xDrive = drive;
            }
        }
    }
}
