using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Import JacobianTools utility functions from JacobianTools.cs
using static JacobianTools;

public class NewtonIK : MonoBehaviour
{
    public ArticulationBody root;
    public List<ArticulationBody> IKChain;
    public Transform IKTarget;
    public ArticulationJointController jointController;
    public ArticulationBody endEffector;
    public float dampedSquaresLambda = 20f;
    public float inverseStep = 0.1f;

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

    private float[] jointAngles;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool success;

    private void PrintList<T>(List<T> list)
    {
        string listString = "";
        for (int i = 0; i < list.Count; i++)
        {
            listString += list[i] + " ";
        }
        Debug.Log(listString);
    }

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
        Debug.Log(totalDOF);
        PrintList(articulationIndices);
        PrintList(articulationDOFs);
    }

    void SolveIK(Vector3 deltaPosition, Vector3 deltaRotation) {
        Debug.Log("SolveIK called");
        root.GetDenseJacobian(ref jacobian);

        int endEffectorIndex = articulationIndices[articulationIndices.Count - 1]; // the index of the end effector
        Debug.Log("End effector index: " + endEffectorIndex);

        // Test alternative
        // TODO: If base is not fixed, need to subtract 6
        int offset = 0;
        if (!root.immovable)
        {
            offset = 6;
        }
        
        ArticulationJacobian minJ = JacobianTools.FillMatrix(endEffectorIndex*6 - offset, articulationDOFs, jacobian);

        // Convert the euler angles in the jacobian to quaternions
        // minJ = JacobianTools.EulerToQuaternion(minJ);
        
        Vector3 endEffectorPosition = endEffector.transform.position;
        Quaternion endEffectorRotation = endEffector.transform.rotation;

        // Get the target position
        targetPosition = IKTarget.position;
        targetRotation = IKTarget.rotation;

        List<float> deltaTarget = new List<float> {
            deltaPosition.x, deltaPosition.y, deltaPosition.z,
            deltaRotation.x, deltaRotation.y, deltaRotation.z
        };
        Debug.Log("Target list");
        PrintList(deltaTarget);
        // List<float> deltaTarget = new List<float> {deltaPosition.x, deltaPosition.y, deltaPosition.z, 0f, 0f, 0f};

        // Solve for the psuedo inverse Jacobian
        // Degenerate? Switching to transpose
        
        // Switch case for different IK types
        ArticulationJacobian invJ = new ArticulationJacobian(1, 1);
        switch (inverseMethod)
        {
            case InverseMethod.Transpose:
                invJ = JacobianTools.Transpose(minJ);
                invJ = JacobianTools.Multiply(invJ, inverseStep);
                break;
            case InverseMethod.DampedLeastSquares:
                invJ = JacobianTools.DampedLeastSquares(minJ, dampedSquaresLambda);
                invJ = JacobianTools.Multiply(invJ, inverseStep);
                break;
            case InverseMethod.PsuedoInverse:
                invJ = JacobianTools.PsuedoInverse(minJ);
                invJ = JacobianTools.Multiply(invJ, inverseStep);
                break;
            case InverseMethod.UnstableInverse:
                invJ = JacobianTools.Inverse(minJ);
                invJ = JacobianTools.Multiply(invJ, inverseStep);
                break;
            default:
                Debug.Log("Invalid IK type, using Transpose");
                invJ = JacobianTools.Transpose(minJ);
                invJ = JacobianTools.Multiply(invJ, inverseStep);
                break;
        }

        // Calculate the delta angles
        List<float> deltaAngles = JacobianTools.Multiply(invJ, deltaTarget);
        
        // Wrap all angles to PI
        for (int i = 0; i < deltaAngles.Count; i++)
        {
            deltaAngles[i] = JacobianTools.WrapToPI(deltaAngles[i]);
        }

        Debug.Log("Predicted Angles:");
        PrintList(deltaAngles);

        // Get the current joint angles
        jointAngles = jointController.GetCurrentJointTargets();

        // Calculate the new joint angles
        List<float> newJointAngles = new List<float>();
        // TODO: This is bad, change to smarter way of getting indices
        for (int i = 0; i < articulationIndices.Count; i++)
        {
            newJointAngles.Add(jointAngles[i] + deltaAngles[i]);
        }
        Debug.Log("New Joint Angles:");
        PrintList(newJointAngles);

        // Set the new joint angles
        jointController.SetJointTargets(newJointAngles.ToArray());

        // TODO: Compute Newton-Rapshon
        // Initialize the error
        float error = Vector3.Distance(IKTarget.position, endEffector.transform.position);
        Debug.Log("Current Error:" + error);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // On press of the "R" key, solve the IK
        Vector3 deltaPosition = Vector3.zero;
        if (Input.GetKey("w")) {
            deltaPosition.y += 1f;
        }
        if (Input.GetKey("s")) {
            deltaPosition.y -= 1f;
        }
        if (Input.GetKey("a")) {
            deltaPosition.x -= 1f;
        }
        if (Input.GetKey("d")) {
            deltaPosition.x += 1f;
        }
        if (Input.GetKey("q")) {
            deltaPosition.z -= 1f;
        }
        if (Input.GetKey("e")) {
            deltaPosition.z += 1f;
        }

        Vector3 deltaRotation = Vector3.zero;
        if (Input.GetKey("i")) {
            deltaRotation.y += 1f;
        }
        if (Input.GetKey("k")) {
            deltaRotation.y -= 1f;
        }
        if (Input.GetKey("j")) {
            deltaRotation.x -= 1f;
        }
        if (Input.GetKey("l")) {
            deltaRotation.x += 1f;
        }
        if (Input.GetKey("u")) {
            deltaRotation.z -= 1f;
        }
        if (Input.GetKey("o")) {
            deltaRotation.z += 1f;
        }

        if (Input.GetKey(KeyCode.Space)) {
            SolveIK(deltaPosition, deltaRotation);
        }

        // Draw a line from the end effector to the target
        Debug.DrawLine(IKChain[IKChain.Count - 1].transform.position, IKTarget.position, Color.red);
    }
}
