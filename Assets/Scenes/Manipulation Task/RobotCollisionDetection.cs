using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotCollisionDetection : MonoBehaviour
{
    public bool onRobotCollision = false;
    public string collisionName = "";

    private void OnTriggerEnter(Collider other)
    {
        if (
            other.gameObject.layer == LayerMask.NameToLayer("Robot")
            && other.isTrigger == false
        )
        {
            onRobotCollision = true;
            collisionName = other.transform.name;
            // Debug.Log("Robot collision detected");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (
            other.gameObject.layer == LayerMask.NameToLayer("Robot")
            && other.isTrigger == false
        )
        {
            onRobotCollision = false;
            // Debug.Log("Robot collision ended");
        }
    }
}