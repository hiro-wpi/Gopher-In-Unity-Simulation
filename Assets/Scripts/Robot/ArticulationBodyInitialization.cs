using System.Linq;
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
    public bool applyMass = false;
    public float mass = 10f;

    private void Start()
    {
        // Get non-fixed joints
        _articulationChain = robotRoot.GetComponentsInChildren<ArticulationBody>();
        _articulationChain = _articulationChain.Where(joint => joint.jointType != ArticulationJointType.FixedJoint)
            .ToArray();
        // remove joints from ignore list
        _articulationChain = _articulationChain.Where(joint => !ignoreList.Contains(joint)).ToArray();

        // Joint length to assign
        var assignLength = _articulationChain.Length;
        if (!assignToAllChildren)
            assignLength = robotChainLength;

        // Setting stiffness, damping and force limit
        const int friction = 100;
        for (var i = 0; i < assignLength; ++i)
        {
            var joint = _articulationChain[i];
            var drive = joint.xDrive;

            joint.jointFriction = friction;
            joint.angularDamping = friction;

            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = forceLimit;
            joint.xDrive = drive;
        }

        if (!applyMass) return;
        
        _massChain = robotRoot.GetComponentsInChildren<ArticulationBody>();
        _massChain = _massChain.Where(joint => !massIgnoreList.Contains(joint)).ToArray();

        // For each in mass chain
        foreach (var joint in _massChain)
        {
            joint.mass = mass;
        }
    }
}