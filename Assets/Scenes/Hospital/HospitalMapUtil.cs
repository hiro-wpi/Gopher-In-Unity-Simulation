using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HospitalMapUtil
{
    private static SortedDictionary<string, float[]> rangeMap = 
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
        {"Pharmacy", new float[] {-14f, 9.0f, -5.0f, 15f}},
        {"Staff Lounge", new float[] {-5.0f, 9.0f, -1.0f, 15f}},
        {"Office", new float[] {-1.0f, 9.0f, 3.0f, 15f}},
        {"Room S103", new float[] {3.0f, 9.0f, 11f, 15f}},
        {"Room P105", new float[] {9.0f, 3.0f, 15f, 10f}},
    };

    private static SortedDictionary<string, Vector3> entryMap = 
               new SortedDictionary<string, Vector3>()
    {
        // Room Name -> (x, y, z)
        {"Room S101", new Vector3 (-6.8f, 0.0f, -10.0f)},
        {"Room S102", new Vector3 ( 7.8f, 0.0f, -10.0f)},
        {"Room P101", new Vector3 (-9.0f, 0.0f, -5.0f)},
        {"Room P102", new Vector3 (-4.5f, 0.0f, -5.0f)},
        {"Room P103", new Vector3 ( 5.5f, 0.0f, -5.0f)},
        {"Room P104", new Vector3 (10.0f, 0.0f, -5.0f)},
        {"Room L101", new Vector3 (-10.0f, 0.0f, 6.0f)},
        {"Treatment Room 1", new Vector3 (-2.5f, 0.0f, 0.5f)},
        {"Treatment Room 2", new Vector3 ( 5.0f, 0.0f, 0.5f)},
        {"Nurse Station", new Vector3 (10.0f, 0.0f, 1.0f)},
        {"Pharmacy", new Vector3 (-7.0f, 0.0f, 10.0f)},
        {"Staff Lounge", new Vector3 (-2.0f, 0.0f, 10.0f)},
        {"Office", new Vector3 (2.0f, 0.0f, 10.0f)},
        {"Room S103", new Vector3 (7.0f, 0.0f, 10.0f)},
        {"Room P105", new Vector3 (10.0f, 0.0f, 5.0f)}
    };

    private static SortedDictionary<string, Vector3[]> freeGroundMap = 
               new SortedDictionary<string, Vector3[]>()
    {
        // Room Name -> (x, y, z)
        {"Room S101", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room S102", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P101", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P102", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P103", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P104", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room L101", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Treatment Room 1", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Treatment Room 2", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Nurse Station", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Pharmacy", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Staff Lounge", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Office", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room S103", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P105", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }}
    };

    private static SortedDictionary<string, Vector3[]> freeTableMap = 
               new SortedDictionary<string, Vector3[]>()
    {
        // Room Name -> (x, y, z)
        {"Room S101", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room S102", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P101", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P102", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P103", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P104", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room L101", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Treatment Room 1", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Treatment Room 2", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Nurse Station", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Pharmacy", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Staff Lounge", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Office", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room S103", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P105", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }}
    };

    private static SortedDictionary<string, Vector3[]> monitorMap = 
               new SortedDictionary<string, Vector3[]>()
    {
        // Room Name -> (position1, rotation1, position2, rotation2 ...)
        {"Room S101", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room S102", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P101", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P102", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P103", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P104", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room L101", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Treatment Room 1", new Vector3[0]},
        {"Treatment Room 2", new Vector3[0]},
        {"Nurse Station", new Vector3[0]},
        {"Pharmacy", new Vector3[0]},
        {"Staff Lounge", new Vector3[0]},
        {"Office", new Vector3[0]},
        {"Room S103", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }},
        {"Room P105", new Vector3[] {
                        new Vector3 (0.0f, 0.0f, 0.0f),
                        new Vector3 (0.0f, 0.0f, 0.0f)
        }}
    };


    public static string[] GetRoomNames()
    {
        string[] names = new string[rangeMap.Count];
        int count = 0;
        foreach(KeyValuePair<string, float[]> entry in rangeMap)
        {
            names[count] = entry.Key;
            count++;
        }
        return names;
    }

    public static string GetLocationName(Vector3 position)
    {
        // Only consider 2d space
        float x = position[0];
        float z = position[2];

        // Check it is inside which location
        foreach(KeyValuePair<string, float[]> entry in rangeMap)
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

    public static Vector3 GetRoomEntry(string name)
    {
        Vector3 entry;
        entryMap.TryGetValue(name, out entry);
        return entry;
    }

    public static Vector3[] GetFreeGroundSpace(string name)
    {
        Vector3[] freeGroundSpace;
        freeGroundMap.TryGetValue(name, out freeGroundSpace);
        return freeGroundSpace;
    }

    public static Vector3[] GetFreeTableSpace(string name)
    {
        Vector3[] freeTableSpace;
        freeTableMap.TryGetValue(name, out freeTableSpace);
        return freeTableSpace;
    }

    public static (Vector3[], Vector3[]) GetMonitorPose(string name)
    {
        Vector3[] ps;
        monitorMap.TryGetValue(name, out ps);
        Vector3[] positions = new Vector3[ps.Length / 2];
        Vector3[] rotations = new Vector3[ps.Length / 2];
        for (int i = 0; i < ps.Length / 2; ++i)
        {
            positions[i] = ps[2 * i];
            rotations[i] = ps[2 * i + 1];
        }
        return (positions, rotations);
    }
}
