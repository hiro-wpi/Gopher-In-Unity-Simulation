using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelaxedIK : InverseKinematics
{
    [SerializeField] private string infoFileName = "";
    private IntPtr relaxedIK;

    private Vector3 currPosition = Vector3.zero;
    private Quaternion currRotation = Quaternion.identity;

    [SerializeField] private Transform home;

    void Awake()
    {
        relaxedIK = RelaxedIKLoader.RelaxedIKNew(infoFileName);
    }

    void OnDestroy()
    {
        // TODO
        // Theoretically, we should free the memory allocated by RelaxedIKNew.
        // But calling the following function causes a crash.
        // However, it seems that the memory is not leaked even it doesn't get called anyway.
        // RelaxedIKLoader.RelaxedIKFree(relaxedIK);
    }

    public Vector3 Trans(Vector3 v)
    {
        return new Vector3(v.y, v.z, -v.x);
    }

    public override (bool, float[]) SolveIK(
        float[] jointAngles, Vector3 targetPosition, Quaternion targetRotation
    ){
        // Get the goal position and rotation w.r.t. the home position transform
        currPosition = home.InverseTransformPoint(targetPosition);
        currRotation = Quaternion.Inverse(home.rotation) * targetRotation;

        // Vector3 position = Utils.ToFLU(currPosition);
        // Quaternion rotation = Utils.ToFLU(currRotation);
        Vector3 position = Trans(currPosition);
        Quaternion rotation = Utils.FromFLU(currRotation);
        // Vector3 position = currPosition;
        // Quaternion rotation = currRotation;


        double[] posArr = new double[3];
        double[] quatArr = new double[4];
        string posStr = "Goal position: ";
        string quatStr = "Goal rotation: ";

        posArr[0] = position.x;
        posArr[1] = position.y;
        posArr[2] = position.z;
        quatArr[0] = rotation.x;
        quatArr[1] = rotation.y;
        quatArr[2] = rotation.z;
        quatArr[3] = rotation.w;

        posStr += currPosition.ToString("F3");
        quatStr += currRotation.ToString("F3");
        Debug.Log(name + " " + posStr);
        Debug.Log(name + " " + quatStr);

        var solution = RelaxedIKLoader.Solve(relaxedIK, posArr, posArr.Length, quatArr, quatArr.Length);
        // Convert double[] to float[]
        var solutionFloat = new float[solution.Length];
        for (int i = 0; i < solution.Length; i++)
        {
            solutionFloat[i] = (float)solution[i];
        }
        return (true, solutionFloat);
    }

    public override float[] SolveVelocityIK(
        float[] jointAngles, Vector3 positionError, Quaternion rotationError
    ){
        currPosition = currPosition + positionError;
        currRotation = currRotation * rotationError;

        double[] posArr = new double[3];
        double[] quatArr = new double[4];
        string posStr = "Goal position: ";
        string quatStr = "Goal rotation: ";

        posArr[0] = currPosition.x;
        posArr[1] = currPosition.y;
        posArr[2] = currPosition.z;
        quatArr[0] = currRotation.x;
        quatArr[1] = currRotation.y;
        quatArr[2] = currRotation.z;
        quatArr[3] = currRotation.w;

        posStr += currPosition.ToString("F3");
        quatStr += currRotation.ToString("F3");
        Debug.Log(name + " " + posStr);
        Debug.Log(name + " " + quatStr);

        var solution = RelaxedIKLoader.Solve(relaxedIK, posArr, posArr.Length, quatArr, quatArr.Length);
        // Convert double[] to float[]
        var solutionFloat = new float[solution.Length];
        for (int i = 0; i < solution.Length; i++)
        {
            solutionFloat[i] = (float)solution[i];
        }
        return solutionFloat;
    }
}
