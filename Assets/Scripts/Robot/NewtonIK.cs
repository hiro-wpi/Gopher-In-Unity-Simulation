using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Import JacobianTools utility functions from JacobianTools.cs
using static JacobianTools;

public class NewtonIK : MonoBehaviour
{
    public ForwardKinematics fk;
    public ArticulationBody root;
    public List<ArticulationBody> IKChain;
    public Transform IKTarget;
    public ArticulationJointController jointController;
    public ArticulationBody endEffector;
    public float dampedSquaresLambda = 20f;
    public float inverseStep = 0.1f;
    public int epochs = 20;

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

    void SolveIK(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        Debug.Log("SolveIK called");
        // root.GetDenseJacobian(ref jacobian);

        // Get the current joint angles
        jointAngles = jointController.GetCurrentJointTargets();

        // Remember target position and rotation
        targetPosition = deltaPosition + endEffector.transform.position;
        targetRotation = deltaRotation * endEffector.transform.rotation;

        for (int e = 0; e < epochs; e++)
        {
            jacobian = fk.ComputeJacobian(jointAngles);

            // Calculate the delta to our target position and rotation
            Vector3 iterativeDeltaPosition = targetPosition - endEffector.transform.position;
            Quaternion iterativeDeltaRotation = targetRotation * Quaternion.Inverse(endEffector.transform.rotation);

            List<float> deltaTarget = new List<float> {
                iterativeDeltaPosition.x, iterativeDeltaPosition.y, iterativeDeltaPosition.z,
                iterativeDeltaRotation.x, iterativeDeltaRotation.y, iterativeDeltaRotation.z, iterativeDeltaRotation.w
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

            // Wrap all angles to PI
            // for (int i = 0; i < deltaAngles.Count; i++)
            // {
            //     deltaAngles[i] = JacobianTools.WrapToPI(deltaAngles[i]);
            // }

            // TODO: This is bad, change to smarter way of getting indices
            for (int i = 0; i < jointAngles.Length; i++)
            {
                // TODO: this crashed unity lol
                jointAngles[i] += deltaAngles[i] / (float)e;
            }
        }

        // Set the new joint angles
        jointController.SetJointTargets(jointAngles);

        // Initialize the error
        float error = Vector3.Distance(IKTarget.position, endEffector.transform.position);
        Debug.Log("Current Error:" + error);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // On press of the "R" key, solve the IK
        float speed = 0.1f;
        Vector3 deltaPosition = Vector3.zero;
        if (Input.GetKey("w"))
        {
            deltaPosition.y += speed;
        }
        if (Input.GetKey("s"))
        {
            deltaPosition.y -= speed;
        }
        if (Input.GetKey("a"))
        {
            deltaPosition.x -= speed;
        }
        if (Input.GetKey("d"))
        {
            deltaPosition.x += speed;
        }
        if (Input.GetKey("q"))
        {
            deltaPosition.z -= speed;
        }
        if (Input.GetKey("e"))
        {
            deltaPosition.z += speed;
        }

        // Quaternion deltaRotation = new Quaternion(0f, 0f, 0f, 0f);

        // Vector3 deltaPosition = IKTarget.position - endEffector.transform.position;
        // Quaternion deltaRotation = targetRotation * Quaternion.Inverse(endEffector.transform.rotation);
        Quaternion deltaRotation = new Quaternion(0f, 0f, 0f, 0f);

        if (Input.GetKey(KeyCode.Space))
        {
            SolveIK(deltaPosition, deltaRotation);
        }

        // Draw a line from the end effector to the target
        Debug.DrawLine(IKChain[IKChain.Count - 1].transform.position, IKTarget.position, Color.red);
    }
}
