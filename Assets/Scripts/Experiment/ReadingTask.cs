using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining reading task.
/// </summary>
public class ReadingTask : Task 
{
    private string userResult = "";
    private bool isCorrect = false;
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
            // Get active cameras
            Camera[] cameras = GUI.GetCurrentActiveCameras();
            // Try all active cameras
            string output = "";
            foreach(Camera cam in cameras)
            {
                barCodeScanner.cam = cam;
                string result = barCodeScanner.Scan(1/2f);
                // until one camera succeeded
                if (result != "N/A")
                {
                    // remove guard pattern for shortening
                    output = result.Substring(1, result.Length-2);
                    break;
                }
            }
            GUI.ShowPopUpMessage("Scan result: " + output, 2.0f);
        }
    }


    public override void CheckInput(string input)
    {
        // Record input
        base.CheckInput(input);

        // Check result
        // ignore result with only one digit
        if (input.Length < 2)
            return;
        // check
        userResult = input;
        if (userResult == result)
        {
            isCorrect = true;
        }
        else
        {
            // GUI.ShowPopUpMessage("Wrong answer.");
        }
    }
    
    public override bool CheckTaskCompletion()
    {
        if (isCorrect)
            GUI.ShowPopUpMessage("Current Task Completed!");
        return isCorrect;
    }


    // Get current task status
    public override string GetTaskStatus()
    { 
        string status = "Your input was: \n" + userResult;
        return status;
    }

    public override void ResetTask()
    {
        taskStarted = false;
        isCorrect = false;
    }
}