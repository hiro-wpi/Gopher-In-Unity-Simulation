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
        for (int j = 0; j < row.Length; ++j)
            row[j] = Array2D[index, j];
        return row;
    }

    public static T[] Shuffle<T> (System.Random random, T[] array)
    {
        int n = array.Length;
        while (n > 1) 
        {
            int k = random.Next(n--);
            (array[n], array[k]) = (array[k], array[n]);
        }
        return array;
    }

    public static T[,] RandomizeRow<T>(System.Random random, T[,] Array2D)
    {
        for (int i = 0; i < Array2D.GetLength(0); ++i)
        {
            T[] temp = Shuffle(random, GetRow(Array2D, i));
            for (int j = 0; j < Array2D.GetLength(1); ++j)
            {
                Array2D[i, j] = temp[j];
            }
        }
        return Array2D;
    }

    // GameObject
    public static void SetGameObjectLayer(GameObject obj, string name, 
                                          bool applyToChild=true)
    {
        int layer = LayerMask.NameToLayer(name);
        obj.layer = layer;
        if (applyToChild)
        {
            for (int i = 0; i < obj.transform.childCount; ++i)
            {
                obj.transform.GetChild(i).gameObject.layer = layer;
            }
        }
    }

    public static string ArrayToCSVLine<T>(T[] array)
    {
        string line = "";
        // Add value to line
        foreach (T value in array)
        {
            if (value is float || value is int)
                line += string.Format("{0:0.000}", value) + ",";
            else if (value is string)
                line += value + ",";
        }
        // Remove "," in the end
        if (line.Length > 0)
            line.Remove(line.Length - 1);
        return line;
    }


    // Transform
    // from RUF (Unity) to FLU (regular)
    public static Vector3 ToFLU(Vector3 v)
    {
        return new Vector3(v.z, -v.x, v.y);
    }
    public static Quaternion ToFLU(Quaternion q)
    {
        return new Quaternion(q.z, -q.x, q.y, -q.w);
    }
    // from FLU (regular) to RUF (Unity)
    public static Vector3 FromFLU(Vector3 v)
    {
        return new Vector3(-v.y, v.z, v.x);
    }
    public static Quaternion FromFLU(Quaternion q)
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
