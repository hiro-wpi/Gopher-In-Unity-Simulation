using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IK Solver", menuName = "IK Solver/Newton IK")]
public class NewtonIK : MonoBehaviour
{
    public ArticulationBody[] IKChain;
    public ArticulationBody root;
    public ArticulationBody endEffector;
    public KinematicSolver kinematicSolver;
    public float dampedSquaresLambda = 0.01f;
    public float inverseStep = 0.02f;
    public float moveSpeed = 0.1f;
    public float rotateSpeed = 0.5f;

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
    private List<int> articulationIndices = new List<int>();
    private List<int> articulationDOFs = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        // Get DOF of each joint
        List<int> DOFs = new List<int>();
        int totalDOF = root.GetDofStartIndices(DOFs);

        // For each sub-body in our IK chain, add it's index to the articulationInices list
        foreach (ArticulationBody body in IKChain)
        {
            articulationIndices.Add(body.index);
            articulationDOFs.Add(DOFs[body.index]);
        }
    }

    public float[] SolveIK(float[] jointAngles, Vector3 deltaPosition, Vector3 deltaRotation)
    {
        // translate the deltaPosition to be relative to the end effector
        deltaPosition = endEffector.transform.TransformVector(deltaPosition);
        deltaRotation = endEffector.transform.TransformVector(deltaRotation);

        kinematicSolver.UpdateAngles(jointAngles);
        kinematicSolver.CalculateAllT();
        kinematicSolver.UpdateAllPose();
        jacobian = kinematicSolver.ComputeJacobian();

        Debug.Log("End Real: " + endEffector.transform.position);

        List<float> deltaTarget = new List<float> {
            deltaPosition.x, deltaPosition.y, deltaPosition.z,
            deltaRotation.x, deltaRotation.y, deltaRotation.z,
        };

        // Switch case for different IK types
        ArticulationJacobian invJ = new ArticulationJacobian(1, 1);
        switch (inverseMethod)
        {
            case InverseMethod.Transpose:
                invJ = JacobianTools.Transpose(jacobian);
                invJ = JacobianTools.Multiply(invJ, inverseStep);
                break;
            case InverseMethod.DampedLeastSquares:
                invJ = JacobianTools.DampedLeastSquares(jacobian, dampedSquaresLambda);
                invJ = JacobianTools.Multiply(invJ, inverseStep);
                break;
            case InverseMethod.PsuedoInverse:
                invJ = JacobianTools.PsuedoInverse(jacobian);
                invJ = JacobianTools.Multiply(invJ, inverseStep);
                break;
            case InverseMethod.UnstableInverse:
                invJ = JacobianTools.Inverse(jacobian);
                invJ = JacobianTools.Multiply(invJ, inverseStep);
                break;
            default:
                Debug.Log("Invalid IK type, using Transpose");
                invJ = JacobianTools.Transpose(jacobian);
                invJ = JacobianTools.Multiply(invJ, inverseStep);
                break;
        }

        // Calculate the delta angles
        List<float> deltaAngles = JacobianTools.Multiply(invJ, deltaTarget);

        // TODO: This is bad, change to smarter way of getting indices
        for (int i = 0; i < jointAngles.Length; i++)
        {
            // TODO: this crashed unity lol
            jointAngles[i] += deltaAngles[i];
        }

        // Set the new joint angles
        return jointAngles;
    }
}