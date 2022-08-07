using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining reading task.
/// </summary>
public class ReadingTask : Task 
{
    public string result;
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
            string res = barCodeScanner.Scan();
            gUI.ShowPopUpMessage("Scan result: " + res, 1.0f);
        }
    }


    public override void CheckInput(string input)
    {
        // Record input
        base.CheckInput(input);
        
        // Check result
        if (input == result)
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
        string status = "Your input was: ";
        if (userInputs != null)
            status += userInputs[userInputIndex];
        return status;
    }

    public override void ResetTaskStatus()
    {
        taskStarted = false;
        isCorrect = false;
    }
}