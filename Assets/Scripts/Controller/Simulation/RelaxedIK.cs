using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
///     Provide util functions to compute inverse kinematics
///     using Relaxed IK
///     
///     There are two "base/home" defined here
///     1. The base transform of the forward kinematics BaseTransform
///     2. The home local pose of the relaxed IK w.r.t. the BaseTransform
/// </summary>
public class RelaxedIK : InverseKinematics
{
    [SerializeField] private string infoFileName = "";
    private IntPtr relaxedIK;
    private int test;

    // Relaxed IK home
    private Vector3 homePosition;
    private Quaternion homeRotation;
    private Vector3 homeLocalPosition;
    private Quaternion homeLocalRotation;
    private float[] homeJointAngles;
    // Relaxed IK pose in relaxed IK frame
    private Vector3 localPosition;
    private Quaternion localRotation;

    void Awake()
    {
        homeJointAngles = new float[forwardKinematics.NumJoint];
    }

    void Start()
    {
        relaxedIK = RelaxedIKLoader.New(infoFileName);
        SetJointAnglesAsHome(homeJointAngles);
    }

    void Update()
    {
        // relaxedIK pointer can get lost after hot reloading
        // a temporary solution - check if it is null
        if (relaxedIK == IntPtr.Zero)
        {
            relaxedIK = RelaxedIKLoader.New(infoFileName);
            SetJointAnglesAsHome(homeJointAngles);
        }
    }

    public override void SetJointAnglesAsHome(float[] jointAngles)
    {
        // Convert from float[] to double[]
        homeJointAngles = jointAngles.Clone() as float[];
        RelaxedIKLoader.Reset(relaxedIK, homeJointAngles);

        // Get the home world pose
        forwardKinematics.SolveFK(homeJointAngles);
        var (homePosition, homeRotation) = forwardKinematics.GetPose(
            forwardKinematics.NumJoint
        );
        // Get the home pose w.r.t. the BaseTransform
        (homeLocalPosition, homeLocalRotation) = Utils.WorldToLocalPose(
            BaseTransform, homePosition, homeRotation
        );
    }

    public override float[] SolveIK(
        float[] jointAngles, Vector3 targetPosition, Quaternion targetRotation
    ) {
        // Get the target in BaseTransform
        (localPosition, localRotation) = Utils.WorldToLocalPose(
            BaseTransform, targetPosition, targetRotation
        );
        // Get the delta pose in the BaseTransform frame
        localPosition -= homeLocalPosition;
        localRotation *= Quaternion.Inverse(homeLocalRotation);

        // Solve IK
        float[] solution = RelaxedIKLoader.Solve(
            relaxedIK, 
            new Vector3[] {localPosition}, 
            new Quaternion[] {localRotation}
        );

        return solution;
    }
}
