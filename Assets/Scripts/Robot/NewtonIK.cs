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

    void SolveIK() {
        Debug.Log("SolveIK called");
        root.GetDenseJacobian(ref jacobian);

        int endEffectorIndex = articulationIndices[articulationIndices.Count - 1]; // the index of the end effector
        Debug.Log("End effector index: " + endEffectorIndex);

        JacobianTools.Print(jacobian);

        // Test alternative
        // TODO: If base is not fixed, need to subtract 6
        ArticulationJacobian minJ = JacobianTools.FillMatrix(endEffectorIndex*6, articulationDOFs, jacobian);
        JacobianTools.Print(minJ);
        
        Vector3 endEffectorPosition = endEffector.transform.position;
        Quaternion endEffectorRotation = endEffector.transform.rotation;

        // Get the target position
        targetPosition = IKTarget.position;
        targetRotation = IKTarget.rotation;

        // Calculate the delta position between the end effector and the target
        Vector3 deltaPosition = targetPosition - endEffectorPosition;
        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(endEffectorRotation);
        // Delta rotation needs to be converted to a vector
        Vector3 deltaRotationVector = deltaRotation * Vector3.forward;
        // List<float> deltaTarget = new List<float> {deltaPosition.x, deltaPosition.y, deltaPosition.z, deltaRotationVector.x, deltaRotationVector.y, deltaRotationVector.z};
        List<float> deltaTarget = new List<float> {deltaPosition.x, deltaPosition.y, deltaPosition.z, 0f, 0f, 0f};

        // Solve for the psuedo inverse Jacobian
        // Degenerate? Switching to transpose
        ArticulationJacobian invJ = JacobianTools.PsuedoInverse(minJ);
        // ArticulationJacobian invJ = JacobianTools.Transpose(minJ);

        // Calculate the delta angles
        List<float> deltaAngles = JacobianTools.Multiply(invJ, deltaTarget);

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
    void Update()
    {
        // On press of the "R" key, solve the IK
        if (Input.GetKeyDown("r")) {
            SolveIK();
        }

        // Draw a line from the end effector to the target
        Debug.DrawLine(IKChain[IKChain.Count - 1].transform.position, IKTarget.position, Color.red);
    }
}
