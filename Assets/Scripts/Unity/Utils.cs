using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    // Array related
    public static T[] ConcatenateArray<T>(T[] array1, T[] array2)
    {
        // Create a new container and copy all values
        T[] newArray = new T[array1.Length + array2.Length];
        array1.CopyTo(newArray, 0);
        array2.CopyTo(newArray, array1.Length);
        return newArray;
    }

    public static int ArgMinArray<T>(T[] array)
    {
        if (array.Length == 0) 
            return -1;

        // Get smallest value and find the index
        T minValue = array.Min();
        return array.ToList().IndexOf(minValue);
    }
    
    public static T[] GetRow<T>(T[,] Array2D, int index)
    {
        // Create a new container and copy all values in the row
        T[] row = new T[Array2D.GetLength(1)];
        for (int i = 0; i < row.Length; ++i)
            row[i] = Array2D[index, i];
        return row;
    }


    // Transform
    // from RUF (Unity) to FLU (regular)
    public static Vector3 ToFlu(Vector3 v)
    {
        return new Vector3(v.z, -v.x, v.y);
    }
    public static Quaternion ToFlu(Quaternion q)
    {
        return new Quaternion(q.z, -q.x, q.y, -q.w);
    }
    // from FLU (regular) to RUF (Unity)
    public static Vector3 FromFlu(Vector3 v)
    {
        return new Vector3(-v.y, v.z, v.x);
    }
    public static Quaternion FromFlu(Quaternion q)
    {
        return new Quaternion(-q.y, q.z, q.x, -q.w);
    }

    
    // Angles
    public static float WrapToPi(float angle)
    {
        return (angle + Mathf.PI) % (2f * Mathf.PI) - Mathf.PI;
    }
    public static float WrapToTwoPi(float angle)
    {
        return (angle + 2f * Mathf.PI) % (2f * Mathf.PI);
    }
}
