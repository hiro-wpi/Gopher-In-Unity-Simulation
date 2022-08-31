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

    private static SortedDictionary<string, Vector3[]> entranceMap = 
               new SortedDictionary<string, Vector3[]>()
    {
        // Room Name -> (position, rotation)
        {"Room S101", new Vector3[] {new Vector3 (-6.8f, 0.0f, -10.0f), 
                                     new Vector3 (0f, 180f, 0f)}},
        {"Room S102", new Vector3[] {new Vector3 ( 7.8f, 0.0f, -10.0f), 
                                     new Vector3 (0f, 180f, 0f)}},
        {"Room P101", new Vector3[] {new Vector3 (-9.0f, 0.0f, -5.0f), 
                                     new Vector3 (0f, -90f, 0f)}},
        {"Room P102", new Vector3[] {new Vector3 (-4.5f, 0.0f, -5.0f), 
                                     new Vector3 (0f,  90f, 0f)}},
        {"Room P103", new Vector3[] {new Vector3 ( 5.5f, 0.0f, -5.0f), 
                                     new Vector3 (0f, -90f, 0f)}},
        {"Room P104", new Vector3[] {new Vector3 (10.0f, 0.0f, -5.0f), 
                                     new Vector3 (0f,  90f, 0f)}},
        {"Room L101", new Vector3[] {new Vector3 (-10.0f, 0.0f, 6.0f), 
                                     new Vector3 (0f, -90f, 0f)}},
        {"Treatment Room 1", new Vector3[] {new Vector3 (-2.5f, 180.0f, 0.5f), 
                                            new Vector3 (0f, 0f, 0f)}},
        {"Treatment Room 2", new Vector3[] {new Vector3 ( 5.0f, 180.0f, 0.5f), 
                                            new Vector3 (0f, 0f, 0f)}},
        {"Nurse Station", new Vector3[] {new Vector3 (10.0f, 0.0f, 1.0f), 
                                         new Vector3 (0f, 90f, 0f)}},
        {"Pharmacy", new Vector3[] {new Vector3 (-7.0f, 0.0f, 10.0f), 
                                    new Vector3 (0f, 0f, 0f)}},
        {"Staff Lounge", new Vector3[] {new Vector3 (-2.0f, 0.0f, 10.0f), 
                                        new Vector3 (0f, 0f, 0f)}},
        {"Office", new Vector3[] {new Vector3 (2.0f, 0.0f, 10.0f), 
                                  new Vector3 (0f, 0f, 0f)}},
        {"Room S103", new Vector3[] {new Vector3 (7.0f, 0.0f, 10.0f), 
                                     new Vector3 (0f, 0f, 0f)}},
        {"Room P105", new Vector3[] {new Vector3 (10.0f, 0.0f, 5.0f), 
                                     new Vector3 (0f, 90f, 0f)}},
    };

    private static SortedDictionary<string, Vector3[]> freeGroundMap = 
               new SortedDictionary<string, Vector3[]>()
    {
        // Room Name -> positions
        {"Room S101", new Vector3[] {
                        new Vector3 (-7.9f, 0.0f, -11.5f),
                        new Vector3 (-5.8f, 0.0f, -11.5f),
                        new Vector3 (-4.2f, 0.0f, -13.3f)
        }},
        {"Room S102", new Vector3[] {
                        new Vector3 ( 6.6f, 0.0f, -11.5f),
                        new Vector3 ( 8.7f, 0.0f, -11.5f),
                        new Vector3 (10.3f, 0.0f, -13.3f)
        }},
        {"Room P101", new Vector3[] {
                        new Vector3 (-11.5f, 0.0f, -7.2f),
                        new Vector3 (-9.8f,  0.0f, -6.8f),
                        new Vector3 (-11.3f, 0.0f, -8.1f)
        }},
        {"Room P102", new Vector3[] {
                        new Vector3 (-4.3f, 0.0f, -7.1f),
                        new Vector3 (-3.7f, 0.0f, -8.3f),
                        new Vector3 (-2.3f, 0.0f, -5.7f)
        }},
        {"Room P103", new Vector3[] {
                        new Vector3 (3.0f, 0.0f, -7.2f),
                        new Vector3 (4.7f, 0.0f, -6.8f),
                        new Vector3 (3.2f, 0.0f, -8.1f)
                        
        }},
        {"Room P104", new Vector3[] {
                        new Vector3 (10.2f, 0.0f, -7.1f),
                        new Vector3 (10.8f, 0.0f, -8.3f),
                        new Vector3 (12.2f, 0.0f, -5.7f)
        }},
        {"Room L101", new Vector3[] {
                        new Vector3 (-11.4f, 0.0f,  3.5f),
                        new Vector3 ( -9.8f, 0.0f,  0.0f),
                        new Vector3 ( -9.8f, 0.0f, -1.5f)
        }},
        {"Treatment Room 1", new Vector3[] {
                        new Vector3 (-3.6f, 0.0f, 1.5f),
                        new Vector3 (-1.8f, 0.0f, 4.5f)
        }},
        {"Treatment Room 2", new Vector3[] {
                        new Vector3 ( 3.4f, 0.0f, 1.0f),
                        new Vector3 ( 5.7f, 0.0f, 4.1f)
        }},
        {"Nurse Station", new Vector3[] {
                        new Vector3 (10.4f, 0.0f, 0.5f),
                        new Vector3 (11.4f, 0.0f, 0.5f),
                        new Vector3 (12.4f, 0.0f, 0.5f)
        }},
        {"Pharmacy", new Vector3[] {
                        new Vector3 (-8.1f, 0.0f, 14.2f),
                        new Vector3 (-6.6f, 0.0f, 13.4f),
                        new Vector3 (-6.6f, 0.0f, 12.4f)
        }},
        {"Staff Lounge", new Vector3[] {
                        new Vector3 (-2.6f, 0.0f, 14.0f),
                        new Vector3 (-3.7f, 0.0f, 11.5f)
        }},
        {"Office", new Vector3[] {
                        new Vector3 ( 0.0f, 0.0f, 10.5f),
                        new Vector3 ( 0.0f, 0.0f, 11.5f)
        }},
        {"Room S103", new Vector3[] {
                        new Vector3 (5.8f, 0.0f, 10.9f),
                        new Vector3 (7.8f, 0.0f, 11.4f),
                        new Vector3 (4.2f, 0.0f, 13.1f)
        }},
        {"Room P105", new Vector3[] {
                        new Vector3 (10.3f, 0.0f, 6.0f),
                        new Vector3 (10.4f, 0.0f, 7.0f),
                        new Vector3 (11.8f, 0.0f, 6.9f)
        }}
    };

    private static SortedDictionary<string, Vector3[]> freeTableMap = 
               new SortedDictionary<string, Vector3[]>()
    {
        // Room Name -> positions
        {"Room S101", new Vector3[] {
                        new Vector3 (-10.1f, 0.8f, -10.4f),
                        new Vector3 ( -3.4f, 0.8f, -10.4f),
                        new Vector3 ( -6.5f, 0.8f, -13.7f)
        }},
        {"Room S102", new Vector3[] {
                        new Vector3 ( 4.4f, 0.8f, -10.4f),
                        new Vector3 (11.1f, 0.8f, -10.4f),
                        new Vector3 ( 8.0f, 0.8f, -13.7f)
        }},
        {"Room P101", new Vector3[] {
                        new Vector3 (-13.3f, 0.8f, -8.9f),
                        new Vector3 (-11.2f, 0.8f, -4.8f),
                        new Vector3 ( -9.9f, 0.5f, -8.4f)
        }},
        {"Room P102", new Vector3[] {
                        new Vector3 (-2.1f, 0.8f, -4.6f),
                        new Vector3 ( 0.0f, 0.8f, -9.1f),
                        new Vector3 (-4.3f, 0.5f, -8.2f)
        }},
        {"Room P103", new Vector3[] {
                        new Vector3 (1.2f, 0.8f, -8.9f),
                        new Vector3 (3.3f, 0.8f, -4.8f),
                        new Vector3 (4.6f, 0.5f, -8.4f)
        }},
        {"Room P104", new Vector3[] {
                        new Vector3 (12.4f, 0.8f, -4.6f),
                        new Vector3 (14.5f, 0.8f, -9.1f),
                        new Vector3 (10.2f, 0.5f, -8.2f)
        }},
        {"Room L101", new Vector3[] {
                        new Vector3 (-11.1f, 0.8f,  0.4f),
                        new Vector3 ( -9.7f, 0.8f, -2.5f),
                        new Vector3 ( -9.7f, 0.9f,  2.7f)
        }},
        {"Treatment Room 1", new Vector3[] {
                        new Vector3 (-1.5f, 0.8f, 1.3f),
                        new Vector3 (-1.6f, 0.9f, 5.5f)
        }},
        {"Treatment Room 2", new Vector3[] {
                        new Vector3 (2.7f, 0.8f, 1.6f),
                        new Vector3 (5.9f, 0.9f, 4.1f)
        }},
        {"Nurse Station", new Vector3[] {
                        new Vector3 ( 9.2f, 1.0f, -1.8f),
                        new Vector3 (10.9f, 1.0f, -0.3f),
                        new Vector3 (13.1f, 1.0f, -0.3f)
        }},
        {"Pharmacy", new Vector3[] {
                        new Vector3 (-8.9f, 1.4f,  9.9f),
                        new Vector3 (-8.9f, 1.1f,  9.9F),
                        new Vector3 (-8.9f, 1.4f, 11.6f),
                        new Vector3 (-8.9f, 1.1f, 11.6f),
                        new Vector3 (-8.9f, 1.4f, 13.3f),
                        new Vector3 (-8.9f, 1.1f, 13.3f),
                        new Vector3 (-5.8f, 0.9f, 11.6f)
        }},
        {"Staff Lounge", new Vector3[] {
                        new Vector3 (-1.8f, 0.9f, 13.3f),
                        new Vector3 (-1.8f, 0.9f, 11.3f)
        }},
        {"Office", new Vector3[] {
                        new Vector3 (-0.2f, 0.9f, 12.0f),
                        new Vector3 (-0.2f, 0.9f, 14.0f)
        }},
        {"Room S103", new Vector3[] {
                        new Vector3 (10.4f, 0.8f, 10.5f),
                        new Vector3 ( 3.6f, 0.8f, 9.8f),
                        new Vector3 ( 6.8f, 0.8f, 13.7f)
        }},
        {"Room P105", new Vector3[] {
                        new Vector3 (14.3f, 0.8f, 8.9f),
                        new Vector3 (12.2f, 0.8f, 4.8f),
                        new Vector3 (10.9f, 0.5f, 8.4f)
        }}
    };

    private static SortedDictionary<string, Vector3[]> monitorMap = 
               new SortedDictionary<string, Vector3[]>()
    {
        // Room Name -> (position1, rotation1, position2, rotation2 ...)
        {"Room S101", new Vector3[] {
                        new Vector3 (-10.2f, 1.1f, -12.4f),
                        new Vector3 (90.0f,  90.0f, 0.0f),
                        new Vector3 ( -3.2f, 1.1f, -12.2f),
                        new Vector3 (90.0f, -90.0f, 0.0f)
        }},
        {"Room S102", new Vector3[] {
                        new Vector3 (  4.3f, 1.1f, -12.4f),
                        new Vector3 (90.0f,  90.0f, 0.0f),
                        new Vector3 ( 11.3f, 1.1f, -12.2f),
                        new Vector3 (90.0f, -90.0f, 0.0f)
        }},
        {"Room P101", new Vector3[] {
                        new Vector3 (-13.6f, 1.1f, -6.6f),
                        new Vector3 (90.0f,  90.0f, 0.0f)
        }},
        {"Room P102", new Vector3[] {
                        new Vector3 (0.1f, 1.1f, -6.6f),
                        new Vector3 (90.0f, -90.0f, 0.0f)
        }},
        {"Room P103", new Vector3[] {
                        new Vector3 (0.9f, 1.1f, -6.6f),
                        new Vector3 (90.0f,  90.0f, 0.0f)
        }},
        {"Room P104", new Vector3[] {
                        new Vector3 (14.6f, 1.1f, -6.6f),
                        new Vector3 (90.0f, -90.0f, 0.0f)
        }},
        {"Room L101", new Vector3[] {
                        new Vector3 (-13.5f, 1.1f, 8.5f),
                        new Vector3 (90.0f, 90.0f, 0.0f),
                        new Vector3 (-13.5f, 1.1f, 5.5f),
                        new Vector3 (90.0f, 90.0f, 0.0f),
                        new Vector3 (-13.5f, 1.1f, 2.5f),
                        new Vector3 (90.0f, 90.0f, 0.0f),
                        new Vector3 (-13.5f, 1.1f,-0.5f),
                        new Vector3 (90.0f, 90.0f, 0.0f)
        }},
        {"Treatment Room 1", new Vector3[0]},
        {"Treatment Room 2", new Vector3[0]},
        {"Nurse Station", new Vector3[0]},
        {"Pharmacy", new Vector3[0]},
        {"Staff Lounge", new Vector3[0]},
        {"Office", new Vector3[0]},
        {"Room S103", new Vector3[] {
                        new Vector3 (10.5f,  1.1f, 12.5f),
                        new Vector3 (90.0f, -90.0f, 0.0f),
                        new Vector3 (3.5f,   1.1f, 12.2f),
                        new Vector3 (90.0f,  90.0f, 0.0f)
        }},
        {"Room P105", new Vector3[] {
                        new Vector3 (14.6f,  1.1f, 6.6f),
                        new Vector3 (90.0f, -90.0f, 0.0f)
        }}
    };

    private static SortedDictionary<string, Vector3[]> passageMap = 
               new SortedDictionary<string, Vector3[]>()
    {
        // Room Name -> (position1, rotation1, position2, rotation2 ...)
        {"Room S101", new Vector3[] {
                        new Vector3 (-6.7f, 0.0f,  -3.0f),
                        new Vector3 ( 0.0f, 180.0f, 0.0f)
        }},
        {"Room S102", new Vector3[] {
                        new Vector3 ( 7.7f, 0.0f,  -3.0f),
                        new Vector3 ( 0.0f, 180.0f, 0.0f)
        }},
        {"Room P101", new Vector3[] {
                        new Vector3 (-6.7f, 0.0f, -3.0f),
                        new Vector3 ( 0.0f, 180.0f, 0.0f)
        }},
        {"Room P102", new Vector3[] {
                        new Vector3 (-6.7f, 0.0f, -3.0f),
                        new Vector3 ( 0.0f, 180.0f, 0.0f)
        }},
        {"Room P103", new Vector3[] {
                        new Vector3 ( 7.7f, 0.0f,  -3.0f),
                        new Vector3 ( 0.0f, 180.0f, 0.0f)
        }},
        {"Room P104", new Vector3[] {
                        new Vector3 ( 7.7f, 0.0f,  -3.0f),
                        new Vector3 ( 0.0f, 180.0f, 0.0f)
        }},
        {"Room L101", new Vector3[] {
                        new Vector3 (-6.7f, 0.0f, 2.7f),
                        new Vector3 ( 0.0f, 0.0f, 0.0f),
                        new Vector3 (-5.9f, 0.0f, 7.5f),
                        new Vector3 ( 0.0f, -90.0f, 0.0f)
        }},
        {"Treatment Room 1", new Vector3[0]},
        {"Treatment Room 2", new Vector3[0]},
        {"Nurse Station", new Vector3[0]},
        {"Pharmacy", new Vector3[] {
                        new Vector3 (-6.7f, 0.0f, 2.7f),
                        new Vector3 ( 0.0f, 0.0f, 0.0f),
                        new Vector3 (-5.9f, 0.0f, 7.5f),
                        new Vector3 ( 0.0f, -90.0f, 0.0f)
        }},
        {"Staff Lounge", new Vector3[] {
                        new Vector3 (-2.0f, 0.0f, 8.5f),
                        new Vector3 ( 0.0f, 0.0f, 0.0f)
        }},
        {"Office", new Vector3[] {
                        new Vector3 ( 2.0f, 0.0f, 8.5f),
                        new Vector3 ( 0.0f, 0.0f, 0.0f)
        }},
        {"Room S103", new Vector3[] {
                        new Vector3 (3.4f,  0.0f, 7.7f),
                        new Vector3 (0.0f, 90.0f, 0.0f),
                        new Vector3 (8.1f,  0.0f, 3.5f),
                        new Vector3 (0.0f,  0.0f, 0.0f)
        }},
        {"Room P105", new Vector3[] {
                        new Vector3 (3.4f,  0.0f, 7.7f),
                        new Vector3 (0.0f, 90.0f, 0.0f),
                        new Vector3 (8.1f,  0.0f, 3.5f),
                        new Vector3 (0.0f,  0.0f, 0.0f)
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

    public static (Vector3, Vector3) GetRoomEntrance(string name)
    {
        Vector3[] entrance;
        entranceMap.TryGetValue(name, out entrance);
        return (entrance[0], entrance[1]);
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

    public static (Vector3[], Vector3[]) GetRoomPassage(string name)
    {
        Vector3[] ps;
        passageMap.TryGetValue(name, out ps);
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
