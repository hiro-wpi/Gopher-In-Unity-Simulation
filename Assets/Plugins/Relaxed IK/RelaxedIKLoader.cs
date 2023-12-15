using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using fts;
using UnityEngine.Assertions;

/// <summary>
///     This script is used to load the RelaxedIK library plugin.
///     
///     A NativePluginLoader should be attached to a GameObject
///     in the scene to load the plugin.
/// </summary>

[StructLayout(LayoutKind.Sequential)]
public struct Opt
{
    public IntPtr data;
    public int length;
}

public static class RelaxedIKLoader
{
    // Initalize and get an instance of the Relaxed IK
    [DllImport("relaxed_ik_lib.dll")]
    private static extern IntPtr relaxed_ik_new(string info_file_name);
    // Public wrapper
    public static IntPtr New(string infoFileName)
    {
        IntPtr relaxedIK = relaxed_ik_new(infoFileName);
        if (relaxedIK == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "Failed to load Relaxed IK, "
                + "the configuration file may not be found."
            );
        }
        return relaxedIK;
    }

    // // Free memory of an instance of the Relaxed IK
    // // This should not be used because it causes a crash.
    // // Also, it seems that the memory is not leaked 
    // // even it doesn't get called anyway.
    // [DllImport("relaxed_ik_lib.dll")]
    // private static extern void relaxed_ik_free(IntPtr relaxed_ik);

    // Reset the given configuration as home configuration
    // TODO, check if reset is valid by checking if length matches
    // Probably need to modify source code of the wrapper
    [DllImport("relaxed_ik_lib.dll")]
    private static extern void reset(
        IntPtr relaxed_ik, double[] joint_angles, int length
    );
    // Public wrapper
    public static void Reset(IntPtr relaxedIK, float[] jointAngles)
    {
        // Convert from float[] to double[]
        double[] angles = jointAngles.Select(d => (double)d).ToArray();
        // Reset
        reset(relaxedIK, angles, angles.Length);
    }

    // Solve the goal in local frame with an instance of the Relaxed IK
    [DllImport("relaxed_ik_lib.dll")]
    private static extern Opt solve(
        IntPtr relaxed_ik, 
        double[] pos_arr, 
        int pos_len, 
        double[] quat_arr, 
        int quat_len
    );
    // Public wrapper
    public static float[] Solve(
        IntPtr relaxedIK, Vector3[] positions, Quaternion[] rotations
    ) {
        Assert.AreEqual(positions.Length, rotations.Length);

        // Convert from RUF (Unity) to FLU (regular)
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = ToFLU(positions[i]);
            rotations[i] = ToFLU(rotations[i]);
        }

        // Convert to double[]
        double[] posArr = new double[3 * positions.Length];
        double[] quatArr = new double[4 * rotations.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            posArr[i * 3 + 0] = (double) positions[i].x;
            posArr[i * 3 + 1] = (double) positions[i].y;
            posArr[i * 3 + 2] = (double) positions[i].z;
            quatArr[i * 4 + 0] = (double) rotations[i].x;
            quatArr[i * 4 + 1] = (double) rotations[i].y;
            quatArr[i * 4 + 2] = (double) rotations[i].z;
            quatArr[i * 4 + 3] = (double) rotations[i].w;
        }

        // Solve
        Opt xopt = solve(
            relaxedIK, posArr, posArr.Length, quatArr, quatArr.Length
        );
        if (xopt.data == IntPtr.Zero)
        {
            Debug.Log("No solution data from Relaxed IK.");
            return null;
        }

        return GetDataFromOpt(xopt);
    }

    // Get the goal pose of the Relaxed IK in local frame
    [DllImport("relaxed_ik_lib.dll")]
    private static extern Opt get_goal_pose(IntPtr relaxed_ik);
    // Public wrapper
    public static (Vector3[], Quaternion[]) GetGoal(IntPtr relaxedIK)
    {
        Opt xopt = get_goal_pose(relaxedIK);
        if (xopt.data == IntPtr.Zero)
        {
            Debug.Log("No goal data from Relaxed IK.");
            return (null, null);
        }
        if (xopt.length % 7 != 0)
        {
            Debug.Log("Goal data from Relaxed IK is not valid.");
            return (null, null);
        }

        float[] res = GetDataFromOpt(xopt);

        // Extract results
        int numPose = res.Length / 7;
        Vector3[] positions = new Vector3[numPose];
        Quaternion[] rotations = new Quaternion[numPose];
        for (int i = 0; i < numPose; i++)
        {
            positions[i] = new Vector3(
                res[i * 7 + 0], res[i * 7 + 1], res[i * 7 + 2]
            );
            rotations[i] = new Quaternion(
                res[i * 7 + 3], res[i * 7 + 4], res[i * 7 + 5], res[i * 7 + 6]
            );
            positions[i] = FromFLU(positions[i]);
            rotations[i] = FromFLU(rotations[i]);
        }

        return (positions, rotations);
    }

    // Get the initial pose of the Relaxed IK in base frame
    [DllImport("relaxed_ik_lib.dll")]
    private static extern Opt get_init_pose(IntPtr relaxed_ik);
    // Public wrapper
    public static (Vector3[], Quaternion[]) GetInit(IntPtr relaxedIK)
    {
        Opt xopt = get_init_pose(relaxedIK);
        if (xopt.data == IntPtr.Zero)
        {
            Debug.Log("No init data from Relaxed IK.");
            return (null, null);
        }
        if (xopt.length % 7 != 0)
        {
            Debug.Log("Init data from Relaxed IK is not valid.");
            return (null, null);
        }

        float[] res = GetDataFromOpt(xopt);

        // Extract results
        int numPose = res.Length / 7;
        Vector3[] positions = new Vector3[numPose];
        Quaternion[] rotations = new Quaternion[numPose];
        for (int i = 0; i < numPose; i++)
        {
            positions[i] = new Vector3(
                res[i * 7 + 0], res[i * 7 + 1], res[i * 7 + 2]
            );
            rotations[i] = new Quaternion(
                res[i * 7 + 3], res[i * 7 + 4], res[i * 7 + 5], res[i * 7 + 6]
            );
            positions[i] = FromFLU(positions[i]);
            rotations[i] = FromFLU(rotations[i]);
        }

        return (positions, rotations);
    }

    // Get data from Opt
    private static float[] GetDataFromOpt(Opt xopt)
    {
        // Allocate a managed array to hold the data.
        double[] solution = new double[xopt.length];
        try
        {
            // Copy from unmanaged to managed memory.
            Marshal.Copy(xopt.data, solution, 0, xopt.length);
        }
        finally
        {
            // Free the unmanaged memory
            // This does not seem to be necessary
            // Marshal.FreeHGlobal(xopt.data);
        }

        float[] solutionF = solution.Select(d => (float)d).ToArray();
        return solutionF;
    }

    // from FLU (regular) to RUF (Unity)
    private static Vector3 FromFLU(Vector3 v)
    {
        return new Vector3(-v.y, v.z, v.x);
    }

    private static Quaternion FromFLU(Quaternion q)
    {
        return new Quaternion(-q.y, q.z, q.x, -q.w);
    }

    // from RUF (Unity) to FLU (regular)
    public static Vector3 ToFLU(Vector3 v)
    {
        return new Vector3(v.z, -v.x, v.y);
    }

    public static Quaternion ToFLU(Quaternion q)
    {
        return new Quaternion(q.z, -q.x, q.y, -q.w);
    }
}
