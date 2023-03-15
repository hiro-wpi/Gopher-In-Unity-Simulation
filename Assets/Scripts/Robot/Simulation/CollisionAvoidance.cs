using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAvoidance : MonoBehaviour
{
    public float updateRate;
    public ArticulationWheelController wheelController;
    public Laser laser;
    public float detectionAngleMin = -0.1589f;
    public float detectionAngleMax =  0.1589f;
    private int minIndex = -1;
    private int maxIndex = -1;
    // critical params
    public float slowMultiplier = 0.5f;
    public float stopDistance = 0.2f;

    public string wheelSpeedLimitID = "collision avoidance limit";


    void Start()
    {
        // Get the index of the given range
        float angleIncrement = (laser.angleMax - laser.angleMin) / (laser.samples - 1);
        for (int i = 0; i < laser.samples; ++i)
        {
            float angle = laser.angleMin + i * angleIncrement;
            if (minIndex == -1 && angle > detectionAngleMin)
                minIndex = i;
            if (maxIndex == -1 && angle > detectionAngleMax)
                maxIndex = i;
        }
        if (minIndex == -1 || maxIndex == -1)
            Debug.LogWarning("The provided detection range does not" +
                             "fall into the laser detection range.");

        InvokeRepeating("SpeedLimitUpdate", 1.0f, 1f / updateRate);
    }


    private void SpeedLimitUpdate()
    {
        float minDistance = GetMinDistanceToObstacle();
        if (minDistance < stopDistance)
            minDistance = 0f;
        // only linear speed limit
        wheelController.AddSpeedLimit(new float[] 
                                      {minDistance * slowMultiplier, 100f,
                                       100f, 100f},
                                      wheelSpeedLimitID);
    }

    private float GetMinDistanceToObstacle()
    {
        // Get the minimal distance to the obstacles forward
        float minDistance = 100f;
        for (int i = minIndex; i < maxIndex; ++i)
        {
            if (laser.obstacleRanges[i] != 0 &&
                laser.obstacleRanges[i] < minDistance)
            {
                minDistance = laser.obstacleRanges[i];
            }
        }
        return minDistance;
    }
}
