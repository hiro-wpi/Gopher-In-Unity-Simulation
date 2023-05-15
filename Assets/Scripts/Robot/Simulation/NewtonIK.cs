using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Provide util functions to compute inverse kinematics
///     for Kinova Gen3 7-DOF robotic arm
///     using Newton numeric IK method
/// </summary>
public class NewtonIK : InverseKinematics
{
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
    [SerializeField] private InverseMethod inverseMethod = InverseMethod.Transpose;

    void Start() {}

    void Update() {}

    private List<float> CalculateError(Vector3 positionError, Vector3 rotationAxis)
    {
        List<float> errorTarget = new List<float>
        {
            positionError.x, positionError.y, positionError.z,
            rotationAxis.x, rotationAxis.y, rotationAxis.z,
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
                invJ = JacobianTools.DampedLeastSquares(jacobian, dampedSquaresLambda);
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

    public override (bool, float[]) SolveIK(
        float[] jointAngles, Vector3 targetPosition, Quaternion targetRotation
    )
    {
        float[] newJointAngles = jointAngles.Clone() as float[];

        // Containers
        Vector3 endEffectorPosition;
        Quaternion endEffectorRotation;

        // Solve IK iteratively
        int EPOCHS = 30;
        for (int e = 0; e < EPOCHS; e++)
        {
            // calculate error between our current end effector position and the target position
            forwardKinematics.SolveFK(newJointAngles);
            (endEffectorPosition, endEffectorRotation) = forwardKinematics.GetPose(forwardKinematics.NumJoint);

            Vector3 positionError = endEffectorPosition - targetPosition;
            Quaternion rotationError = endEffectorRotation * Quaternion.Inverse(targetRotation);

            // Orientation is stored in the jacobian as a scaled rotation axis
            // Where the axis of rotation is the vector, and the angle is the length of the vector (in radians)
            // So, use ToAngleAxis to get axis and angle
            rotationError.ToAngleAxis(out float rotationAngle, out Vector3 rotationAxis);
            // Wrap angle into [-pi, pi]
            // (not exactly sure why this is necessary)
            rotationAngle = Mathf.DeltaAngle(0f, rotationAngle);

            // Prevent rotationAxis being NaN crashed the algorithm
            if (Mathf.Abs(rotationAngle) < 1e-3)
                rotationAxis = Vector3.zero;
            // Scale the rotation axis by the angle
            // prioritize the position
            else
                rotationAxis *= rotationAngle * Mathf.Deg2Rad; 

            // Decay lambda over time
            float lambda = (1 - e / EPOCHS) * 0.5f;
            positionError *= lambda;
            rotationAxis *= lambda;

            var errorAngles = CalculateError(positionError, rotationAxis);
            for (int i = 0; i < newJointAngles.Length; i++)
                newJointAngles[i] += errorAngles[i];
        }

        // Result validation check
        foreach (float jointAngle in newJointAngles)
            if (jointAngle == float.NaN)
                return (false, jointAngles);
        
        // Result Convergence check
        forwardKinematics.UpdateAngles(newJointAngles);
        forwardKinematics.UpdateAllTs();
        forwardKinematics.UpdateAllPose();
        // Check only position
        (endEffectorPosition, _) = forwardKinematics.GetPose(forwardKinematics.NumJoint);
        bool converged = (endEffectorPosition - targetPosition).magnitude < 0.1f;

        // Return the new joint angles
        return (converged, newJointAngles);
    }

    public override float[] SolveVelocityIK(
        float[] jointAngles, Vector3 positionDelta, Quaternion rotationDelta
    )
    {
        float[] newJointAngles = jointAngles.Clone() as float[];

        // calculate error between our current end effector position and the target position
        forwardKinematics.SolveFK(newJointAngles);

        // Orientation is stored in the jacobian as a scaled rotation axis
        // Where the axis of rotation is the vector, and the angle is the length of the vector (in radians)
        // So, use ToAngleAxis to get axis and angle
        rotationDelta.ToAngleAxis(out float rotationAngle, out Vector3 rotationAxis);

        // Function returns angle in degrees, so convert to radians
        rotationAngle = Mathf.Deg2Rad * rotationAngle;
        // Now scale the rotation axis by the angle
        rotationAxis *= rotationAngle; // Prioritize the position

        // transform to local
        positionDelta = BaseTransform.TransformVector(positionDelta);
        rotationAxis = BaseTransform.TransformVector(rotationAxis);

        var errorAngles = CalculateError(positionDelta, rotationAxis);
        for (int i = 0; i < newJointAngles.Length; i++)
            newJointAngles[i] += errorAngles[i];

        return newJointAngles;
    }
}
