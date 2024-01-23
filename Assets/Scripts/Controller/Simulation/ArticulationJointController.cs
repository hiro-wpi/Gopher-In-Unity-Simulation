using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script allows robot joints to be controlled with 
///
///     SetJointTarget (use coroutine to move to target positions)
///     SetJointTargetStep (move to the target positions in one delta time)
///     SetJointSpeedStep (move at the target velocity in one delta time)
///     SetJointTrajectory (use coroutine to move along a trajectory).
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
    // trajectory control
    private List<float[]> targetPositions;
    private int currTrajectoryIndex = 0;

    void Awake()
    {
        // Get colliders of all articulation bodies
        // Only consider those that are active by default
        colliders = articulationChain[0].GetComponentsInChildren<Collider>();
        colliders = colliders.Where(
            collider => collider.enabled == true
        ).ToArray();
    }

    void Start() {}

    void Update() {}

    // Set joint target
    // Use coroutine to move to target positions given joint speed limits
    // Recommended for setting joint targets that are largely 
    // different from the current joint positions
    public void SetJointTargets(
        float[] targets, bool disableColliders = false, Action reached = null
    )
    {
        // Stop current coroutine
        if (currCoroutine != null)
        {
            StopCoroutine(currCoroutine);
        }
        // Move to target positions
        currCoroutine = StartCoroutine(
            SetJointTargetsCoroutine(targets, disableColliders, reached)
        );
    }

    // Coroutine for joint movement (move to target positions)
    private IEnumerator SetJointTargetsCoroutine(
        float[] jointPositions, bool disableColliders = false, Action reached = null
    )
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
        reached?.Invoke();
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
        // Check if reached
        return CheckJointTargetStep(positions);
    }

    private bool CheckJointTargetStep(float[] positions)
    {
        // Check if current joint targets are set to the target positions
        float[] currTargets = GetCurrentJointTargets();
        for (int i = 0; i < positions.Length; ++i)
        {
            if ((positions[i] != IGNORE_VAL) && 
                (Mathf.Abs(currTargets[i] - positions[i]) > 1e-5f))
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
    public void SetJointTrajectory(
        float[] timeSteps,
        float[][] targets,
        float[][] speeds = null,
        float[][] accelerations = null,
        Action reached = null
    )
    {
        // Due to the fact that the robot control is actually
        // just position control, interpolation is needed to convert
        // targets, speeds and accelerations values into a single list
        // where each element represents the target positions
        // at each Time.fixedDeltaTime
        targetPositions = new List<float[]>();

        // For each waypoint/timestep (starting at 1)
        float prevTime = 0;
        for (int i = 1; i < timeSteps.Length; i++)
        {
            // For each time frame in this timestep
            prevTime = timeSteps[i - 1];
            int frames = Mathf.RoundToInt(
                (timeSteps[i] - prevTime) / Time.fixedDeltaTime
            );
            for (int frame = 0; frame < frames; frame++)
            {

                // For each joint in this time frame
                float[] positions = new float[articulationChain.Length];
                for (int joint = 0; joint < articulationChain.Length; joint++)
                {
                    float t = (float) (frame + 1) / frames;
                    // Calculate the target positions 
                    positions[joint] = 
                        // TODO implement model with speed and acceleration
                        // targets[i][joint] :
                        // + speeds[i][joint] * t 
                        // + 0.5f * accelerations[i][joint] * t * t : 
                        // Only target is given, use linear interpolation
                        Mathf.Lerp(
                            targets[i - 1][joint], targets[i][joint], t
                        );
                }
                // Add the target positions to the list
                targetPositions.Add(positions);
            }
        }

        // Stop current coroutine
        if (currCoroutine != null)
        {
            StopCoroutine(currCoroutine);
        }
        // Move along trajectory
        currCoroutine = StartCoroutine(SetJointTrajectoryCoroutine(reached));
    }

    private IEnumerator SetJointTrajectoryCoroutine(Action reached = null)
    {
        currTrajectoryIndex = 0;
        while(currTrajectoryIndex < targetPositions.Count)
        {
            
            SetJointTargetsStep(targetPositions[currTrajectoryIndex]);
            // Introduce a delay for fixedDeltaTime pause between each frame
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            currTrajectoryIndex++;
        }

        reached?.Invoke();
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

    // Get Current Joint Positions
    // TODO Not Tested Yet
    private float[] GetCurrentJointPositions()
    {
        // Container
        float[] positions = new float[articulationChain.Length];
        // Get each joint position from xDrive
        for (int i = 0; i < articulationChain.Length; ++i)
        {
            positions[i] = (
                articulationChain[i].jointPosition[0] * Mathf.Deg2Rad
            );
        }
        return positions;
    }


}
