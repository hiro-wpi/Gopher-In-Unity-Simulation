using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///    TODO in the future

public class DynamicWindowApproach : MonoBehaviour
{
    private Vector3[] GenerateTrajectory(Vector3 position, Quaternion rotation,
                                         float speed, float angularSpeed,
                                         float simTime = 1f, int numSteps = 5)
    {
        float dt = simTime / numSteps;
        Vector3[] trajectory = new Vector3[numSteps];
        for (int i = 0; i < numSteps; ++i) 
        {
            //add the point to the trajectory
            trajectory[i] = position;
            // update the position and rotation
            position += rotation * Vector3.forward * speed;
            rotation = Quaternion.Euler(rotation.eulerAngles + 
                                        new Vector3(0f, angularSpeed * dt, 0f));
        }
        return trajectory;
    }

    private float ScoreTrajectory(Vector3[] trajectory, 
                                  float bestCost)
    {
        float trajectoryCost = 0;
        foreach (Vector3 point in trajectory) 
        {
            // compute cost
            float cost = ScorePoint(point);
            // collision
            if (cost < 0) 
                return cost;
            // accumulative
            trajectoryCost += cost;
            // if already worse than the best
            if (bestCost > 0 && trajectoryCost > bestCost)
                break;
        }
        return trajectoryCost;
    }

    private float ScorePoint(Vector3 point)
    {
        return 0;
    }
}