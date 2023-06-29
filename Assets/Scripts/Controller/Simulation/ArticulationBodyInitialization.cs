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
///     So, all joints are still initialized for the purpose of position control.
/// </remarks>
public class ArticulationBodyInitialization : MonoBehaviour
{
    [SerializeField] private GameObject robotRoot;
    [SerializeField] private float stiffness = 10000f;
    [SerializeField] private float damping = 100f;
    [SerializeField] private float forceLimit = 1000f;

    // Either assign to all children or assign to a specific length
    [SerializeField] private bool assignToAllChildren = true;
    [SerializeField] private int robotChainLength = 0;

    private ArticulationBody[] articulationChain;

    private void Start()
    {
        // Get non-fixed joints
        articulationChain = robotRoot.GetComponentsInChildren<ArticulationBody>();
        articulationChain = articulationChain.Where(
            joint => joint.jointType != ArticulationJointType.FixedJoint
        ).ToArray();
        
        // Joint length to assign
        int assignLength = articulationChain.Length;
        if (!assignToAllChildren)
            assignLength = robotChainLength;

        // Setting stiffness, damping and force limit
        const int friction = 100;
        for (var i = 0; i < assignLength; ++i)
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
    }
}
