using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;
using System.IO;

/// <summary>
///     This class is used to get the gaze point from the 
///     Tobii Eye Tracker 5.
/// </summary>
public class EyeTracking : MonoBehaviour
{
    // [field:serializeField, ReadOnly]
    public Vector2 Pixel;
    private GazePoint gazePoint;

    [SerializeField] private GraspingTask graspingTask;
    bool isDataRecorded = false;
    private string dataString = "";
    public string fileName;
    private TextWriter textWriter;

    // [SerializeField] private RectTransform gaze;

    void Start()
    {
        if (!TobiiAPI.IsConnected)
        {
            Debug.Log("No device is connected.");
        }    
    }

    void FixedUpdate()
    {
        // Update gaze position
        if (gazePoint.IsValid)
        {
            gazePoint = TobiiAPI.GetGazePoint();
            Pixel = gazePoint.Screen;

            // add the gaze point to the data string separated by a comma
            dataString += Time.realtimeSinceStartup.ToString() + "," + Pixel.x + "," + Pixel.y + "\n";

            // if (gaze != null)
            // {
            //     gaze.anchoredPosition = new Vector2(
            //         Pixel.x,
            //         Pixel.y
            //     );
            // }
        }

        if (!isDataRecorded && graspingTask.CheckTaskCompletion())
        {
            isDataRecorded = true;
            textWriter = new StreamWriter(fileName + "_eye_tracking_manipulation_task.csv", false);
            textWriter.WriteLine(dataString);
            textWriter.Close();
        }
    }

    void OnDestroy()
    {
        if (!isDataRecorded)
        {
            isDataRecorded = true;
            textWriter = new StreamWriter(fileName + "_eye_tracking_manipulation_task.csv", false);
            textWriter.WriteLine(dataString);
            textWriter.Close();
        }
    }   
}
