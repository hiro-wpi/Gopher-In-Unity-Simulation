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
    [SerializeField] private GameObject guiBlocker;


    // [SerializeField] private RectTransform gaze;

    void Start()
    {
        // if (!TobiiAPI.IsConnected)
        // {
        //     Debug.Log("No device is connected.");
        // }    
    }

    void Update()
    {
        // Check if the simulation is paused
        if(Time.deltaTime == 0f)
        {
            // Simulation is paused, pass a point that is not on the screen
            Pixel.x = -3000;
            Pixel.y = -3000;
            return;
        }
    }

    void FixedUpdate()
    {

        // Check if the gui blocker is active in the scene
        if(guiBlocker.activeInHierarchy)  
        {
            return;
        }

        // Check that the canvas Pixel to Gui is ready
        if(!canvasPixelToGui.isReady)
        {
            return;
        }

        // TODO (Remove) Testing without eye tracking
        // Check if the gui blocker is active in the scene
        
        Pixel = Input.mousePosition;
        string guiString;
        string ar2d;
        string ar3d;
        (guiString, ar2d, ar3d) = canvasPixelToGui.GetGUIAR(Pixel);
        dataString += Time.realtimeSinceStartup.ToString() + "," + Pixel.x + "," + Pixel.y + "," + guiString + "," + ar2d + "," + ar3d + "\n";

        // Debug.Log(GetGUIAR(Input.mousePosition));

        

        // TODO Add it to the data string to get converted over to csv


        // Update gaze position
        // if (gazePoint.IsValid)
        // {
        //     gazePoint = TobiiAPI.GetGazePoint();
        //     Pixel = gazePoint.Screen;
        
        //     string guiString;
        //    string ar2d;
        //    string ar3d;
        //    (guiString, ar2d, ar3d) = canvasPixelToGui.GetGUIAR(Pixel);
        //    dataString += Time.realtimeSinceStartup.ToString() + "," + Pixel.x + "," + Pixel.y + "," + guiString + "," + ar2d + "," + ar3d + "\n";

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
