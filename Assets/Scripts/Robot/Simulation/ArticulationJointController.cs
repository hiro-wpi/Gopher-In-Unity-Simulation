using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script allows robot joints to be controlled with 
///
///     SetJointTarget (use coroutine to move to target positions),
///         Recommended for setting joint targets that are largely different
///     
///     SetJointTargetStep (move to the target positions in one delta time),
///         Recommended for real-time servoing,
///         in which the angles change is certainly small
///
///     SetJointSpeedStep (move at the target velocity in one delta time).
///         Recommended for velocity control,
///         in which desired joint velocities are known in each time step
/// </summary>
public class ArticulationJointController : MonoBehaviour
{
    // Control parameters
    [SerializeField] private float jointMaxSpeed = 1.0f; 
    
    // Articulation Bodies Presets
    // When joint position is set to be IGNORE_VAL, don't change it
    public static float IGNORE_VAL = -100f;
    
    // Articulation Bodies
    [SerializeField] private ArticulationBody[] articulationChain;
    private Collider[] colliders;

    // Coroutine for joint movement (move to target positions)
    private Coroutine currCoroutine;

    void Awake()
    {
        // Get colliders of all articulation bodies
        // Only consider those that are active by default
        colliders = articulationChain[0].GetComponentsInChildren<Collider>();
        colliders = colliders.Where(collider => collider.enabled == true).ToArray();
    }

    void Start() {}

    void Update() {}

    // Set joint target
    // Use coroutine to move to target positions given joint speed limits
    // Recommended for setting joint targets that are largely 
    // different from the current joint positions
    public void SetJointTargets(float[] targets, bool disableColliders = false)
    {
        // Stop current coroutine
        if (currCoroutine != null)
        {
            StopCoroutine(currCoroutine);
        }
        // Move to target positions
        currCoroutine = StartCoroutine(
            SetJointTargetsCoroutine(targets, disableColliders)
        );
    }

    // Coroutine for joint movement (move to target positions)
    private IEnumerator SetJointTargetsCoroutine(
        float[] jointPositions, bool disableColliders = false)
    {
        // Disable colliders before homing to avoid collisions
        // when the robot starts in a narrow space
        if (disableColliders)
        {
            SetCollidersActive(false);
        }
        yield return new WaitUntil(() => SetJointTargetsStepAndCheck(jointPositions) == true);
        if (disableColliders)
        {
            SetCollidersActive(true);
        }
    }

    // This is only for the purpose of initializing joint positions
    // at the start of the Unity simulation
    private void SetCollidersActive(bool active)
    {
        foreach (Collider collider in colliders)
        {
            collider.enabled = active;
        }
    }

    private bool SetJointTargetsStepAndCheck(float[] positions)
    {
        // Set joint targets
        SetJointTargetsStep(positions);

        // Check if current joint targets are set to the target positions
        float[] currTargets = GetCurrentJointTargets();
        for (int i = 0; i < positions.Length; ++i)
        {
            if ((positions[i] != IGNORE_VAL) && 
                (Mathf.Abs(currTargets[i] - positions[i]) > 0.00001))
            {
                return false;
            }
        }
        return true;
    }

    // Set joint target step
    // Directly setting target positions
    // Only recommended for real-time servoing / velocity control 
    // in which the angles change is certainly small
    public void SetJointTargetsStep(float[] targets)
    {
        for (int i = 0; i < articulationChain.Length; ++i)
        {
            if (targets[i] == IGNORE_VAL)
            {
                continue;
            }
            SetJointTargetStep(articulationChain[i], targets[i]);
        }
    }

    private void SetJointTargetStep(ArticulationBody joint, float target)
    {
        ArticulationBodyUtils.SetJointTargetStep(
            joint, target * Mathf.Rad2Deg, jointMaxSpeed * Mathf.Rad2Deg
        );
    }

    // Set joint speed
    public void SetJointSpeedsStep(float[] speeds)
    {
        for (int i = 0; i < articulationChain.Length; ++i)
        {
            SetJointSpeedStep(articulationChain[i], speeds[i]);
        }
    }

    private void SetJointSpeedStep(ArticulationBody joint, float speed)
    {
        speed = Mathf.Clamp(speed, 0.0f, jointMaxSpeed);
        ArticulationBodyUtils.SetJointSpeedStep(joint, speed * Mathf.Rad2Deg);
    }

    // Set joint trajectory
    public void SetJointTrajectories(
        float[][] targets, float[][] speeds, float[][] acclerations = null)
    {
        
    }

    // Stop joints
    public void StopJoints()
    {
        foreach (ArticulationBody joint in articulationChain)
        {
            ArticulationBodyUtils.StopJoint(joint);
        }
    }

    // Getters
    public int GetNumJoints()
    {
        // Get joint length
        if (articulationChain == null)
        {
            return 0;
        }
        return articulationChain.Length;
    }

    public ArticulationBody[] GetJoints()
    {
        return articulationChain;
    }

    public float[] GetCurrentJointTargets()
    {
        // Container
        float[] targets = new float[articulationChain.Length];
        // Get each joint target from xDrive
        for (int i = 0; i < articulationChain.Length; ++i)
        {
            targets[i] = articulationChain[i].xDrive.target;
            targets[i] *= Mathf.Deg2Rad;
        }
        return targets;
    }
}
