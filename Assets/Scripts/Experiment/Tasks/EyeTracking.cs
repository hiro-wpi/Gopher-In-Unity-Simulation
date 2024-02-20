using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;

/// <summary>
///     This class is used to get the gaze point from the 
///     Tobii Eye Tracker 5.
/// </summary>
public class EyeTracking : MonoBehaviour
{
    // [field:serializeField, ReadOnly]
    public Vector2 Pixel;
    private GazePoint gazePoint;
    [SerializeField] private TextWriter textWriter;

    [SerializeField] private Task task;

    void Start()
    {
        if (!TobiiAPI.IsConnected)
        {
            Debug.Log("No device is connected.");
        }

        string recordFolder = Application.dataPath + "/Data/";
        if (!Directory.Exists(recordFolder))
        {
            Directory.CreateDirectory(recordFolder);
        }
        textWriter = new StreamWriter(
            recordFolder + task.TaskName + "_gaze.csv", false
        );
    }

    void Update()
    {
        // Update gaze position
        if (gazePoint.IsValid)
        {
            gazePoint = TobiiAPI.GetGazePoint();
            Pixel = gazePoint.Screen;
        }
    }   

    void FixedUpdate()
    {
        textWriter.WriteLine(
            Time.realtimeSinceStartup 
            + "," 
            + Pixel.x
            + ","
            + Pixel.y
        );
    }

    void OnDestroy()
    {
        textWriter.Close();
    }
}
