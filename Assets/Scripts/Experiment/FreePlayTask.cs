using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining free play.
/// Can also be used as tutorial before experiment.
//  No actual task and goal are given so that the task would not end.
/// </summary>
public class FreePlayTask : Task 
{
    // Bar code scanner for some tasks
    private BarCodeScanner barCodeScanner;
    private PaintShooting paintShooting;
    
    void Start()
    {
        barCodeScanner = gameObject.AddComponent<BarCodeScanner>();
    }
    void Update()
    {
        if (robot == null) 
            return;

        if (Input.GetKeyDown(KeyCode.N))
        {
            // Get active cameras
            Camera[] cameras = gUI.GetCurrentActiveCameras();
            // Try all active cameras
            foreach(Camera cam in cameras)
            {
                barCodeScanner.cam = cam;
                string result = barCodeScanner.Scan(1/2f);
                // until one camera succeeded
                if (result != "N/A")
                {
                    // remove guard pattern for shortening
                    result = result.Substring(1, result.Length-2);
                    break;
                }
            }
            gUI.ShowPopUpMessage("Scan result: " + result, 2.0f);
        }

        
        if (Input.GetMouseButtonDown(0))
        {
            paintShooting = robot.GetComponentInChildren<PaintShooting>();
            if (paintShooting != null)
                paintShooting.PlayPainting();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (paintShooting != null)
                paintShooting.StopPainting();
        }
    }


    public override bool CheckTaskStart()
    {
        return false;
    }

    public override bool CheckTaskCompletion()
    {
        return false;
    }


    public override string GetTaskStatus()
    {
        return "N/A";
    }
}