using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A Timer class used to control functions' process rate.
///     Serve as a similar function as InvokeRepeating(),
///     but allow enable/disable at run time.
///
///     UpdateTimer() should be called in each loop with specified deltaTime.
///     ShouldProcess could be accessed and modified by other scripts.
/// </summary>
public class Timer
{
    // Rate
    private float processPeriod;
    // Elapsed time
    private float timer;
    private float maxTimerScale;
    // Result flag
    public bool ShouldProcess { get; set; } = false;

    public Timer(int publishRate, float maxTimerScale = 10.0f)
    {
        processPeriod = 1.0f / publishRate;
        this.maxTimerScale = maxTimerScale;
    }

    public void UpdateTimer(float deltaTime)
    {
        // The ShouldProcess is NOT initialized to false.
        // It would remain the same as input.
        // This is to ensure that
        // the true flag could remain until it gets processed.
        timer += deltaTime;

        // Check if time to publish
        if (timer >= processPeriod)
        {
            ShouldProcess = true;
            // Using "-=" instead of "= 0" to ensure 
            // consistent publish rate
            timer -= processPeriod;
        }

        // A safe mechanism
        // As "-=" was used to calculate elapsedTime previously,
        // reset elapsedTime in case it somehow gets too large
        if (timer > processPeriod * maxTimerScale)
        {
            timer = 0.0f;
        }
    }
}