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
    private ArticulationBody[] _articulationChain;
    //public ArticulationBody[] ignoreList;

    public GameObject robotRoot;
    public bool assignToAllChildren = true;
    public int robotChainLength = 0;
    public float stiffness = 10000f;
    public float damping = 100f;
    public float forceLimit = 1000f;

    private void Start()
    {
        // Get non-fixed joints
        _articulationChain = robotRoot.GetComponentsInChildren<ArticulationBody>();
        _articulationChain = _articulationChain.Where(joint => joint.jointType 
                                                      != ArticulationJointType.FixedJoint).ToArray();
        // remove joints from ignore list
        // _articulationChain = _articulationChain.Where(joint => !ignoreList.Contains(joint)).ToArray();

        // Joint length to assign
        int assignLength = _articulationChain.Length;
        if (!assignToAllChildren)
            assignLength = robotChainLength;

        // Setting stiffness, damping and force limit
        const int friction = 100;
        for (var i = 0; i < assignLength; ++i)
        {
            ArticulationBody joint = _articulationChain[i];
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