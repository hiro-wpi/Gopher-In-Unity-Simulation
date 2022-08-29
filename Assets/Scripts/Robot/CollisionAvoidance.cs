using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAvoidance : MonoBehaviour
{
    public float updateRate;
    public ArticulationWheelController wheelController;
    public Laser laser;
    // public float detectionAngleMin;
    // public float detectionAngleMax;
    public string wheelSpeedLimitID = "collision avoidance limit";

    void Start()
    {
        InvokeRepeating("SpeedLimitUpdate", 1.0f, 1f / updateRate);
    }

    private void SpeedLimitUpdate()
    {
        float minDistance = GetMinDistanceToObstacle();
        // only linear speed limit
        wheelController.AddSpeedLimit(minDistance, -1f, wheelSpeedLimitID);
    }

    private float GetMinDistanceToObstacle()
    {

        return -1f;
    }
}
