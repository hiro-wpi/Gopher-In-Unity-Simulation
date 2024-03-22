using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Tobii.Gaming;
using System.IO;

/// <summary>
///     This class is used to get the gaze point from the 
///     Tobii Eye Tracker 5.
/// </summary>
public class EyeTrackingNavigation : MonoBehaviour
{
    [SerializeField, ReadOnly]
    public Vector2 Pixel;

    // private GazePoint gazePoint;

    // bool isDataRecorded = false;
    private string dataString = "";
    public string fileName;
    private TextWriter textWriter;

    [SerializeField] private CanvasPixelToGui canvasPixelToGui;


    // [SerializeField] private RectTransform gaze;

    void Start()
    {
        // if (!TobiiAPI.IsConnected)
        // {
        //     Debug.Log("No device is connected.");
        // }    
    }

    void FixedUpdate()
    {
        // TODO Check that the canvas Pixel to Gui is ready
        // TODO Request for the ar gameobject and gui
        // TODO Add it to the data string to get converted over to csv

        // Update gaze position
        // if (gazePoint.IsValid)
        // {
        //     gazePoint = TobiiAPI.GetGazePoint();
        //     Pixel = gazePoint.Screen;

        //     // add the gaze point to the data string separated by a comma
        //     dataString += Time.realtimeSinceStartup.ToString() + "," + Pixel.x + "," + Pixel.y + "\n";

        //     // if (gaze != null)
        //     // {
        //     //     gaze.anchoredPosition = new Vector2(
        //     //         Pixel.x,
        //     //         Pixel.y
        //     //     );
        //     // }
        //}
    }

    void OnDestroy()
    {
        LogResponse();
    }

    public void LogResponse()
    {
        textWriter = new StreamWriter(fileName + "_eye_tracking_navigation_task.csv", false);
        textWriter.WriteLine(dataString);
        textWriter.Close();
    }
}
