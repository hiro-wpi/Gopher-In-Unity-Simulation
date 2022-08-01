using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HospitalMapUtil
{
    private static SortedDictionary<string, float[]> map = 
               new SortedDictionary<string, float[]>()
    {
        // Room Name -> (min_x, min_y, max_x, max_y)
        {"Room S101", new float[] {-10.7f, -15f, -2.7f, -10f}},
        {"Room S102", new float[] {  3.8f, -15f, 11.8f, -10f}},
        {"Room P101", new float[] {-14f,  -10f, -8f,  -3.0f}},
        {"Room P102", new float[] {-5.5f, -10f, 0.5f, -3.0f}},
        {"Room P103", new float[] { 0.5f, -10f, 6.5f, -3.0f}},
        {"Room P104", new float[] { 9.0f, -10f, 15f,  -3.0f}},
        {"Room L101", new float[] {-14f, -3.0f, -9.0f, 9.0f}},
        {"Treatment Room 1", new float[] {-5.5f, -0.5f, -1.0f, 6.5f}},
        {"Treatment Room 2", new float[] { 2.0f, -0.5f,  6.5f, 6.5f}},
        {"Nurse Station", new float[] { 9.0f, -3.0f, 15f, 3.0f}},
        {"Room P105", new float[] {9.0f, 3.0f, 15f, 10f}},
        {"Pharmacy", new float[] {-14f, 9.0f, -5.0f, 15f}},
        {"Staff Lounge", new float[] {-5.0f, 9.0f, -1.0f, 15f}},
        {"Office", new float[] {-1.0f, 9.0f, 3.0f, 15f}},
        {"Room S103", new float[] {3.0f, 9.0f, 11f, 15f}}
    };

    public static string GetLocationName(Vector3 position)
    {
        // Only consider 2d space
        float x = position[0];
        float z = position[2];

        // Check it is inside which location
        foreach(KeyValuePair<string, float[]> entry in map)
        {
            if (IsInsideRoom(new Vector2(x, z), entry.Value))
                return entry.Key;
        }
        return "Corridor"; // Default
    }
    private static bool IsInsideRoom(Vector2 pos, float[] roomPosition)
    {
        if ((pos.x > roomPosition[0] && pos.x < roomPosition[2]) &&
            (pos.y > roomPosition[1] && pos.y < roomPosition[3]))
            return true;
        else
            return false;
    }
}