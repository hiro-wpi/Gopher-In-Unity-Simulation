using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotCollisionDetection : MonoBehaviour
{
    public bool onRobotCollision = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Robot"))
        {
            onRobotCollision = true;
            Debug.Log("Robot collision detected");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Robot"))
        {
            onRobotCollision = false;
            Debug.Log("Robot collision ended");
        }
    }
}