using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewtonIK : MonoBehaviour
{
    public ArticulationBody root;
    public ArticulationBody endEffector;
    public Transform localToWorldTransform;
    public KinematicSolver kinematicSolver;
    public float dampedSquaresLambda = 0.01f;

    // Enum for the type of inverse method to use
    public enum InverseMethod
    {
        Transpose,
        DampedLeastSquares,
        PsuedoInverse,
        UnstableInverse
    }

    public InverseMethod inverseMethod = InverseMethod.Transpose;

    // We have to initialize it, or Unity CRASHES (gosh I love Unity, this definitely didn't take an hour to figure out)
    private ArticulationJacobian jacobian = new ArticulationJacobian(1, 1);

    private float WrapToPi(float angle)
    {
        // Wrap angle to [-pi, pi]
        float pi = Mathf.PI;
        angle = angle - 2 * pi * Mathf.Floor((angle + pi) / (2 * pi));
        return angle;
    }

    private Vector3 WrapRadians(Vector3 angles)
    {
        angles.x = WrapToPi(angles.x);
        angles.y = WrapToPi(angles.y);
        angles.z = WrapToPi(angles.z);
        return angles;
    }

    public (bool, float[]) SolveIK(float[] jointAngles, Vector3 targetPosition, Quaternion targetRotation, bool local = true)
    {
        float[] endEffectorAngles = jointAngles.Clone() as float[];

        targetRotation.z = -targetRotation.z;
        targetRotation.x = -targetRotation.x;
        targetRotation.y = -targetRotation.y;

        Vector3 endEffectorPosition;
        Quaternion endEffectorRotation;

        int EPOCHS = 30;
        for (int e = 0; e < EPOCHS; e++)
        {
            // calculate error between our current end effector position and the target position
            kinematicSolver.UpdateAngles(endEffectorAngles);
            kinematicSolver.CalculateAllT();
            kinematicSolver.UpdateAllPose();

            (endEffectorPosition, endEffectorRotation) = kinematicSolver.GetPose(kinematicSolver.numJoint);

            Vector3 positionError = endEffectorPosition - targetPosition;
            Quaternion rotationError = Quaternion.Inverse(endEffectorRotation) * targetRotation;

            // Orientation is stored in the jacobian as a scaled rotation axis
            // Where the axis of rotation is the vector, and the angle is the length of the vector (in radians)
            // So, use ToAngleAxis to grab axis and angle
            Vector3 rotationAxis;
            float rotationAngle;
            rotationError.ToAngleAxis(out rotationAngle, out rotationAxis);

            // Function returns angle in degrees, so convert to radians
            rotationAngle = Mathf.Deg2Rad * rotationAngle;
            // Now scale the rotation axis by the angle
            rotationAxis *= rotationAngle; // Prioritize the position

            // decay lambda over time
            float lambda = (1 - e / EPOCHS);

            rotationAxis *= lambda;
            positionError *= lambda * 0.5f;

            jacobian = kinematicSolver.ComputeJacobian();

            List<float> errorTarget = new List<float> {
                positionError.x, positionError.y, positionError.z,
                rotationAxis.x, rotationAxis.y, rotationAxis.z,
            };


            // Switch case for different IK types
            ArticulationJacobian invJ = new ArticulationJacobian(1, 1);
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

            // TODO: This is bad, change to smarter way of getting indices
            for (int i = 0; i < endEffectorAngles.Length; i++)
            {
                // TODO: this crashed unity lol
                endEffectorAngles[i] += errorAngles[i];
            }
        }

        // check convergence
        kinematicSolver.UpdateAngles(endEffectorAngles);
        kinematicSolver.CalculateAllT();
        kinematicSolver.UpdateAllPose();

        (endEffectorPosition, endEffectorRotation) = kinematicSolver.GetPose(kinematicSolver.numJoint);
        bool converged = (endEffectorPosition - targetPosition).magnitude < 0.01f;

        // Set the new joint angles
        return (converged, endEffectorAngles);
    }
}