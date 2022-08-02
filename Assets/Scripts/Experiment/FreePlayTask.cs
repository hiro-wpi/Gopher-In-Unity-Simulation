using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining free play. 
//  No actual task and goal are given so that the task would not end.
/// </summary>
public class FreePlayTask : Task 
{
    // Bar code scanner for some tasks
    private BarCodeScanner barCodeScanner;
    
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
            barCodeScanner.cam = gUI.GetCurrentMainCamera();
            string res = barCodeScanner.Scan();
            gUI.ShowPopUpMessage("Scan result: " + res, 1.0f);
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