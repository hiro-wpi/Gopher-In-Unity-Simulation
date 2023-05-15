using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Autonomy for arm manipulation.
///     Plan a simple straight-line trajectory 
///     from current position to target position
/// </summary>
public class UnityAutoManipulation : AutoManipulation
{
    // Parameter
    [SerializeField] 

    // Kinematic solver
    public ForwardKinematics forwardKinematics;
    public InverseKinematics inverseKinematics;

    void Start() {}

    void Update() {}

    public override (float[], float[][], float[][], float[][]) PlanTrajectory(
        float[] currJointAngles, 
        Vector3 targetPosition, 
        Quaternion targetRotation,
        bool cartesianSpace = false
    )
    {
        // Solve IK for target position joint angles
        var (converged, targetJointAngles) = inverseKinematics.SolveIK(
            currJointAngles, targetPosition, targetRotation
        );
        if (!converged)
        {
            Debug.Log("No valid path to given target.");
            return (null, null, null, null);
        }

        

        // Lerp between points to generate a path
        completionTime = (grasping.GetEndEffector().transform.position - 
                          targetPosition).magnitude / automationSpeed;
        yield return LerpJoints(jointAngles, targetJointAngles, completionTime);

        // 2, Move to graspable target
        targetPosition = targetGraspPoint.position;
        targetRotation = targetGraspPoint.rotation;

        // Assume we got to the target
        jointAngles = targetJointAngles;
        (converged, targetJointAngles) = inverseKinematics.SolveIK(
            jointAngles, targetPosition, targetRotation
        );
        if (!converged)
        {
            Debug.Log("No valid IK solution given to grasping point.");
            mode = Mode.Control;
            yield break;
        }

        completionTime = (grasping.GetEndEffector().transform.position - 
                          targetPosition).magnitude / automationSpeed;
        yield return LerpJoints(jointAngles, targetJointAngles, completionTime);
    }

    private IEnumerator LerpJoints(
        float[] currentAngles, 
        float[] targetJointAngles, 
        float seconds
    )
    {
        float elapsedTime = 0;
        // Keep track of starting angles
        var startingAngles = currentAngles.Clone() as float[];
        while (elapsedTime < seconds)
        {
            // lerp each joint angle in loop
            // calculate smallest difference between current and target joint angle
            // using atan2(sin(x-y), cos(x-y))
            for (var i = 0; i < targetJointAngles.Length; i++)
            {
                currentAngles[i] = Mathf.Lerp(startingAngles[i], 
                                              targetJointAngles[i], 
                                              (elapsedTime / seconds));
            }

            elapsedTime += Time.deltaTime;

            jointController.SetJointTargets(currentAngles);
            yield return new WaitForEndOfFrame();
        }
    }
}
