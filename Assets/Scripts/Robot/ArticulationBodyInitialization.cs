using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script initializes the articulation bodies by 
///     setting stiffness, damping and force limit of
///     the non-fixed ones.
/// </summary>
public class ArticulationBodyInitialization : MonoBehaviour
{
    private ArticulationBody[] _articulationChain;
    private ArticulationBody[] _massChain;

    public ArticulationBody[] ignoreList;
    public ArticulationBody[] massIgnoreList;

    public GameObject robotRoot;
    public bool assignToAllChildren = true;
    public int robotChainLength = 0;
    public float stiffness = 10000f;
    public float damping = 100f;
    public float forceLimit = 1000f;
    public float mass = 10f;

    void Start()
    {
        // Get non-fixed joints
        _articulationChain = robotRoot.GetComponentsInChildren<ArticulationBody>();
        _massChain = robotRoot.GetComponentsInChildren<ArticulationBody>();
        _articulationChain = _articulationChain.Where(joint => joint.jointType
                                                             != ArticulationJointType.FixedJoint).ToArray();

        // remove joints from ignore list
        _articulationChain = _articulationChain.Where(joint => !ignoreList.Contains(joint)).ToArray();
        _massChain = _massChain.Where(joint => !massIgnoreList.Contains(joint)).ToArray();

        // Joint length to assign
        var assignLength = _articulationChain.Length;
        if (!assignToAllChildren)
            assignLength = robotChainLength;

        // Setting stiffness, damping and force limit
        var friction = 100;
        for (int i = 0; i < assignLength; ++i)
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
        
        // For each in mass chain
        foreach (var joint in _massChain)
        {
            joint.mass = mass;
        }
    }
}