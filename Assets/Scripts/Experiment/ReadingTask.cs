using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining reading task.
/// </summary>
public class ReadingTask : Task 
{
    public string result;
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
            barCodeScanner.cam = gUI.GetCurrentMainCamera();
            string result = barCodeScanner.Scan(1/2f);
            if (result != "N/A")
                // remove guard pattern for shortening
                result = result.Substring(1, result.Length-2);
            gUI.ShowPopUpMessage("Scan result: " + result, 2.0f);
        }
    }


    public override void CheckInput(string input, bool onlyToRecord)
    {
        // Record input
        base.CheckInput(input);
        if (onlyToRecord)
            return;
        
        // Check result
        userResult = input;
        if (userResult == result)
            isCorrect = true;
        else
            gUI.ShowPopUpMessage("Wrong answer.");
    }
    
    public override bool CheckTaskCompletion()
    {
        if (isCorrect)
            gUI.ShowPopUpMessage("Current Task Completed!");
        return isCorrect;
    }


    // Get current task status
    public override string GetTaskStatus()
    { 
        string status = "Your input was: \n" + userResult;
        return status;
    }

    public override void ResetTaskStatus()
    {
        taskStarted = false;
        isCorrect = false;
    }
}