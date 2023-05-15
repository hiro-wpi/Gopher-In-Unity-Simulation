using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Autonomy for arm manipulation.
///     Plan a simple start-line trajectory 
///     from current position to target position
/// </summary>
public abstract class UnityAutoManipulation : AutoManipulation
{
    public NewtonIK newtonIK;

    void Start() {}

    void Update() {}

    public override (float[], float[][], float[][], float[][]) PlanTrajectory(
        float[] currJointAngles, Vector3 targetPosition, Quaternion targetRotation
    )
    {
        // Solve IK for target position joint angles
        var (converged, targetJointAngles) = newtonIK.SolveIK(
            currJointAngles, targetPosition, targetRotation
        );
        if (!converged)
        {
            Debug.Log("No valid IK solution given to hover point.");
            return (null, null, null, null);
        }

        // Lerp between points
        completionTime = (grasping.GetEndEffector().transform.position - 
                          targetPosition).magnitude / automationSpeed;
        yield return LerpJoints(jointAngles, targetJointAngles, completionTime);

        // 2, Move to graspable target
        targetPosition = targetGraspPoint.position;
        targetRotation = targetGraspPoint.rotation;

        // Assume we got to the target
        jointAngles = targetJointAngles;
        (converged, targetJointAngles) = newtonIK.SolveIK(jointAngles, targetPosition, targetRotation);
        if (!converged)
        {
            Debug.Log("No valid IK solution given to grasping point.");
            mode = Mode.Control;
            yield break;
        }

        completionTime = (grasping.GetEndEffector().transform.position - 
                          targetPosition).magnitude / automationSpeed;
        yield return LerpJoints(jointAngles, targetJointAngles, completionTime);

        // Give back to manual control
        mode = Mode.Control;
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
