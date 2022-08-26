using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Provide util functions to compute inverse kinematics
///     for Kinova Gen3 7-DOF robotic arm
///     using Newton numeric IK method
/// </summary>
public class NewtonIK : MonoBehaviour
{
    public ArticulationBody root;
    public ArticulationBody endEffector;
    public Transform localToWorldTransform;
    public KinematicSolver kinematicSolver;
    public float dampedSquaresLambda = 0.01f;

    void Start() {}

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
        angle -= 2 * pi * Mathf.Floor((angle + pi) / (2 * pi));
        return angle;
    }

    private Vector3 WrapRadians(Vector3 angles)
    {
        angles.x = WrapToPi(angles.x);
        angles.y = WrapToPi(angles.y);
        angles.z = WrapToPi(angles.z);
        return angles;
    }


    private List<float> CalculateError(Vector3 positionError, Vector3 rotationAxis)
    {
        List<float> errorTarget = new List<float>
        {
            positionError.x, positionError.y, positionError.z,
            rotationAxis.x, rotationAxis.y, rotationAxis.z,
        };

        // Switch case for different IK types
        jacobian = kinematicSolver.GetJacobian();
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
        return errorAngles;
    }


    public (bool, float[]) SolveIK(float[] jointAngles, Vector3 targetPosition, Quaternion targetRotation)
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
            kinematicSolver.SolveFK(newJointAngles);
            (endEffectorPosition, endEffectorRotation) = kinematicSolver.GetPose(kinematicSolver.numJoint);

            Vector3 positionError = endEffectorPosition - targetPosition;
            Quaternion rotationError = endEffectorRotation * Quaternion.Inverse(targetRotation);
            /*
            Vector3 rotationErrorEuler = new Vector3(Mathf.DeltaAngle(endEffectorRotation.eulerAngles[0], 
                                                                           targetRotation.eulerAngles[0]),
                                                     Mathf.DeltaAngle(endEffectorRotation.eulerAngles[1], 
                                                                           targetRotation.eulerAngles[1]),
                                                     Mathf.DeltaAngle(endEffectorRotation.eulerAngles[2], 
                                                                           targetRotation.eulerAngles[2]));
            Quaternion rotationError = Quaternion.Euler(rotationErrorEuler);
            // rotationError = Quaternion.identity;
            // Debug.Log(endEffectorRotation.eulerAngles);
            // Debug.Log(targetRotation.eulerAngles);
            // Debug.Log(targetPosition);
            */

            // Orientation is stored in the jacobian as a scaled rotation axis
            // Where the axis of rotation is the vector, and the angle is the length of the vector (in radians)
            // So, use ToAngleAxis to grab axis and angle
            Vector3 rotationAxis;
            float rotationAngle;
            rotationError.ToAngleAxis(out rotationAngle, out rotationAxis);
            // rotationAngle = Mathf.DeltaAngle(rotationAngle, 0f);
            // Debug.Log(rotationAngle);
            
            // Function returns angle in degrees, so convert to radians
            rotationAngle = rotationAngle * Mathf.Deg2Rad;
            // Now scale the rotation axis by the angle
            rotationAxis *= rotationAngle; // Prioritize the position

            // decay lambda over time
            float lambda = (1 - e / EPOCHS) * 0.5f;
            positionError *= lambda;
            rotationAxis *= lambda;

            var errorAngles = CalculateError(positionError, rotationAxis);
            for (int i = 0; i < newJointAngles.Length; i++)
                newJointAngles[i] += errorAngles[i];
        }

        // check convergence
        kinematicSolver.UpdateAngles(newJointAngles);
        kinematicSolver.UpdateAllTs();
        kinematicSolver.UpdateAllPose();
        (endEffectorPosition, endEffectorRotation) = kinematicSolver.GetPose(kinematicSolver.numJoint);
        
        bool converged = (endEffectorPosition - targetPosition).magnitude < 0.1f;

        // Set the new joint angles
        return (converged, newJointAngles);
    }


    public float[] SolveVelocityIK(float[] jointAngles, Vector3 positionError, Quaternion rotationError)
    {
        float[] newJointAngles = jointAngles.Clone() as float[];

        // calculate error between our current end effector position and the target position
        kinematicSolver.SolveFK(newJointAngles);

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

        // transform to local
        positionError = localToWorldTransform.TransformVector(positionError);
        rotationAxis = localToWorldTransform.TransformVector(rotationAxis);

        var errorAngles = CalculateError(positionError, rotationAxis);
        for (int i = 0; i < newJointAngles.Length; i++)
            newJointAngles[i] += errorAngles[i];

        return newJointAngles;
    }
}