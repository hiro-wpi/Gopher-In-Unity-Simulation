using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Provide util functions to compute inverse kinematics
///     using Jacobian numeric IK method
/// </summary>
public class JacobianIK : InverseKinematics
{
    // Parameter
    [SerializeField] private int iterations = 10;
    [SerializeField] private float positionTolerance = 0.001f;
    [SerializeField] private float rotationTolerance = 0.02f;

    // Damped least squares lambda
    [SerializeField] private float dampedSquaresLambda = 0.01f;
    private ArticulationJacobian jacobian = new(1, 1);

    // Enum for the type of inverse method to use
    public enum InverseMethod
    {
        Transpose,
        DampedLeastSquares,
        PsuedoInverse,
        UnstableInverse
    }
    [SerializeField] 
    private InverseMethod inverseMethod = InverseMethod.Transpose;

    void Start() {}

    void Update() {}

    public override float[] SolveIK(
        float[] jointAngles, Vector3 targetPosition, Quaternion targetRotation
    ) {
        float[] newJointAngles = jointAngles.Clone() as float[];

        // Containers
        Vector3 eePosition;
        Quaternion eeRotation;
        Vector3 positionError;
        Quaternion rotationError;

        // Solve IK iteratively
        for (int e = 0; e < iterations; e++)
        {
            // calculate error between our current 
            // end effector position and the target position
            forwardKinematics.SolveFK(newJointAngles, updateJacobian: true);
            (eePosition, eeRotation) = forwardKinematics.GetPose(
                forwardKinematics.NumJoint
            );

            positionError = eePosition - targetPosition;
            rotationError = (
                eeRotation * Quaternion.Inverse(targetRotation)
            );

            // Orientation is stored in the jacobian as a scaled rotation axis
            // Where the axis of rotation is the vector, 
            // and the angle is the length of the vector (in radians)
            // So, use ToAngleAxis to get axis and angle
            rotationError.ToAngleAxis(
                out float rotationAngle, out Vector3 rotationAxis
            );
            // Wrap angle into [-pi, pi]
            // (not exactly sure why this is necessary)
            rotationAngle = Mathf.DeltaAngle(0f, rotationAngle);
            rotationAngle *= Mathf.Deg2Rad;

            // Prevent rotationAxis being NaN crashed the algorithm
            if (Mathf.Abs(rotationAngle) < 1e-3)
            {
                rotationAxis = Vector3.zero;
            }
            // Scale the rotation axis by the angle
            // prioritize the position
            else
            {
                rotationAxis *= rotationAngle;
            }

            // Check convergence
            if (positionError.magnitude < positionTolerance
                && Mathf.Abs(rotationAngle) < rotationTolerance
            ) {
                break;
            }

            // Decay lambda over time
            float lambda = (1 - e / iterations) * 0.5f;
            positionError *= lambda;
            rotationAxis *= lambda;

            // Get solutions
            var errorAngles = CalculateError(positionError, rotationAxis);
            for (int i = 0; i < newJointAngles.Length; i++)
            {
                newJointAngles[i] += errorAngles[i];
            }
        }

        // Return the new joint angles
        return newJointAngles;
    }

    private List<float> CalculateError(
        Vector3 positionError, Vector3 rotationError
    ) {
        List<float> errorTarget = new List<float>
        {
            positionError.x, positionError.y, positionError.z,
            rotationError.x, rotationError.y, rotationError.z,
        };

        // Switch case for different IK types
        jacobian = forwardKinematics.GetJacobian();

        ArticulationJacobian invJ;
        switch (inverseMethod)
        {
            case InverseMethod.Transpose:
                invJ = JacobianTools.Transpose(jacobian);
                break;
            case InverseMethod.DampedLeastSquares:
                invJ = JacobianTools.DampedLeastSquares(
                    jacobian, dampedSquaresLambda
                );
                break;
            case InverseMethod.PsuedoInverse:
                invJ = JacobianTools.PsuedoInverse(jacobian);
                break;
            case InverseMethod.UnstableInverse:
                invJ = JacobianTools.Inverse(jacobian);
                break;
            default:
                Debug.Log("Invalid IK type, using Transpose");
                invJ = JacobianTools.Transpose(jacobian);
                break;
        }

        // Calculate the delta angles
        List<float> errorAngles = JacobianTools.Multiply(invJ, errorTarget);
        return errorAngles;
    }


    public bool CheckGoalReached(float[] jointAngles, Vector3 targetPosition, Quaternion targetRotation)
    {
        // Containers
        Vector3 eePosition;
        Quaternion eeRotation;
        Vector3 positionError;
        Quaternion rotationError;

        // calculate error between our current 
        // end effector position and the target position
        forwardKinematics.SolveFK(jointAngles, updateJacobian: true);
        (eePosition, eeRotation) = forwardKinematics.GetPose(
            forwardKinematics.NumJoint
        );

        positionError = eePosition - targetPosition;
        rotationError = (
            eeRotation * Quaternion.Inverse(targetRotation)
        );

        // Orientation is stored in the jacobian as a scaled rotation axis
        // Where the axis of rotation is the vector, 
        // and the angle is the length of the vector (in radians)
        // So, use ToAngleAxis to get axis and angle
        rotationError.ToAngleAxis(
            out float rotationAngle, out Vector3 rotationAxis
        );
        // Wrap angle into [-pi, pi]
        // (not exactly sure why this is necessary)
        rotationAngle = Mathf.DeltaAngle(0f, rotationAngle);
        rotationAngle *= Mathf.Deg2Rad;

        // Prevent rotationAxis being NaN crashed the algorithm
        if (Mathf.Abs(rotationAngle) < 1e-3)
        {
            rotationAxis = Vector3.zero;
        }
        // Scale the rotation axis by the angle
        // prioritize the position
        else
        {
            rotationAxis *= rotationAngle;
        }

        // Check convergence
        if (positionError.magnitude < positionTolerance
            && Mathf.Abs(rotationAngle) < rotationTolerance
        ) {
            return true;
        }
        else
        {
            return false;
        }
    }
}
