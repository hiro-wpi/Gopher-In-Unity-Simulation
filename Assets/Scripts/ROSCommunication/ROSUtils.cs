using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Utils function specifically for ROS communication
/// </summary>
public static class ROSUtils
{
    public static void CheckPublish(
        ref bool shouldPublish,
        ref float timer,
        float deltaTime,
        float publishPeriod,
        int maxElapsedTimeScale = 10
    )
    {
        // shouldPublish is NOT initialized to false.
        // It would remain the same as input.
        // This is to ensure that,
        // even if CheckPublish is called too frequently,
        // the true flag could remain until it gets processed.

        timer += deltaTime;

        // Check if time to publish
        if (timer >= publishPeriod)
        {
            shouldPublish = true;
            // Using "-=" instead of "= 0" to ensure 
            // consistent publish rate
            timer -= publishPeriod;
        }

        // A safe mechanism
        // As "-=" was used to calculate elapsedTime previously,
        // reset elapsedTime in case it somehow gets too large
        if ((maxElapsedTimeScale > 1)
            && (timer > publishPeriod * maxElapsedTimeScale)
        )
        {
            timer = 0.0f;
        }
    }
}