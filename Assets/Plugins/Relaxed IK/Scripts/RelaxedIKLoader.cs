using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using fts;

/// <summary>
///     This script is used to load the RelaxedIK library plugin.
/// </summary>

[StructLayout(LayoutKind.Sequential)]
public struct Opt
{
    public IntPtr data;
    public int length;
}

[PluginAttr("relaxed_ik_lib")]
public static class RelaxedIKLoader
{
    // Initalize and get an instance of the Relaxed IK
    [PluginFunctionAttr("relaxed_ik_new")]
    public static NewDelegate RelaxedIKNew = null;
    public delegate IntPtr NewDelegate(string info_file_name);

    // Free memory of an instance of the Relaxed IK
    [PluginFunctionAttr("relaxed_ik_free")]
    public static FreeDelegate RelaxedIKFree = null;
    public delegate void FreeDelegate(IntPtr relaxedIK);

    // Solve the goal with an instance of the Relaxed IK
    [PluginFunctionAttr("solve")]
    private static SolveDelegate RelaxedIKSolve = null;
    private delegate Opt SolveDelegate(
        IntPtr relaxedIK, 
        double[] posArr, 
        int posLen, 
        double[] quatArr, 
        int quatLen
    );

    // Public wrapper method
    public static float[] Solve(
        IntPtr relaxedIK, double[] posArr, int posLen, double[] quatArr, int quatLen
    ){
        Opt xopt = RelaxedIKSolve(relaxedIK, posArr, posLen, quatArr, quatLen);
        if (xopt.data == IntPtr.Zero)
        {
            throw new InvalidOperationException("No data from Relaxed IK.");
        }

        // Allocate a managed array to hold the data.
        float[] solution = new float[xopt.length];
        try
        {
            // Copy from unmanaged to managed memory.
            Marshal.Copy(xopt.data, solution, 0, xopt.length);
        }
        finally
        {
            // Free the unmanaged memory if required,
            // Marshal.FreeHGlobal(xopt.data);
        }

        return solution;
    }
}
