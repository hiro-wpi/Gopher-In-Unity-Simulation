using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DataRecorder : MonoBehaviour
{
    // Recording
    private float updateRate = 10;
    private bool isRecording;

    public bool IsRecording { get; }
    public bool UpdateRate { get; set; }

    // Recoded task
    private Task task;

    // CSV writter
    private TextWriter valueTextWriter;
    private TextWriter stringTextWriter;

    void Start()
    {
        // Initialization
        isRecording = false;
        updateRate = 10;
    }
    
    // Start a new recording
    public void StartRecording(string fileName, Task task,
                               string[] valueHeaders = null, string[] stringHeaders = null)
    {
        // Stop previous recording if any
        if (isRecording)
            StopRecording();
    
        // Create new files
        valueTextWriter = new StreamWriter(name + "_value.csv", false);
        stringTextWriter = new StreamWriter(name + "_string.csv", false);
        if (valueHeaders != null)
            valueTextWriter.WriteLine(ArrayToCSVLine<string>(valueHeaders));  
        if (stringHeaders != null)
            valueTextWriter.WriteLine(ArrayToCSVLine<string>(stringHeaders));

        // Start getting data
        this.task = task;
        InvokeRepeating("WriteData", 1f, 1/updateRate);

        isRecording = true;
    }

    // Get data from the task
    private void WriteData()
    {
        return;
        if (task.ValueToRecord != null || task.ValueToRecord.Length > 0)
            valueTextWriter.WriteLine(ArrayToCSVLine<float>(task.ValueToRecord));  
        
        if (task.StringToRecord != null || task.StringToRecord.Length > 0)
            stringTextWriter.WriteLine(ArrayToCSVLine<string>(task.StringToRecord));
    }

    // Stop current recording
    public void StopRecording()
    {
        // If not recording
        if (!isRecording) 
            return;
    
        // Cancel getting data
        CancelInvoke("WriteData");
        // Stop writers and save files
        valueTextWriter.Close();
        stringTextWriter.Close();

        isRecording = false;
    }

    // Utils
    private string ArrayToCSVLine<T>(T[] array)
    {
        string line = "";
        // Add value to line
        foreach (T value in array)
        {
            if (value is float || value is int)
                line += string.Format("{0:0.000}", value) + ",";
            else if (value is string)
                line += value + ",";
        }
        // Remove "," in the end
        line.Remove(line.Length - 1);
        return line;
    }
}
