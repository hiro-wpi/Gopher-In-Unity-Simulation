using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// <summary>
//     Level 1 implementation of Safety: Safety Rated Monitored Stop
//     Behavior
//          If a human enters the robot's workspace, the robot will stop
//          Once the human leaves the robot workspace, the robot will continue 
//              autonomy from where it last was
// </summary>
public class SafetyRatedMonitoredStop : MonoBehaviour
{
    // [SerializeField] private Collider collider;

    [SerializeField] private BaseController baseController;
    [SerializeField] private ChestController chestController;
    [SerializeField] private ArmController leftArmController;
    [SerializeField] private ArmController rightArmController;

    private int HumanCount = 0;
    private bool emergencyStopFlag = false;

    void FixedUpdate()
    {

        if(HumanCount == 0)
        {
            EmergencyStopResume();
        }
        else
        {
            EmergencyStop();
        }

        // Reset the human Count here
        HumanCount = 0;

    }

    // Collision Collbacks
    //      Actively is ran every Fixed Update

    // private void OnTriggerEnter(Collider other) {}
    // private void OnTriggerExit(Collider other) {}
    
    private void OnTriggerStay(Collider other)
    {
        // Debug.Log(other.gameObject.name);
        if(other.gameObject.tag == "Human")
        {
            HumanCount += 1;
        }
    }

    // Emergency Stop Operations
    public void EmergencyStop()
    {
        // ignores messages to stop the robot if it has been already stopped
        if(!emergencyStopFlag)
        {
            emergencyStopFlag = true;
            Debug.Log("Emergency Stopping");
            // Stops the whole robot
            baseController.EmergencyStop();
            chestController.EmergencyStop();
            leftArmController.EmergencyStop();
            rightArmController.EmergencyStop();
        }
    }

    public void EmergencyStopResume()
    {
        if(emergencyStopFlag)
        {
            emergencyStopFlag = false;
            Debug.Log("Emergency Stop Eborted, Resuming Operation");
            // Resume Robot Operation from Stop State
            baseController.EmergencyStopResume();
            chestController.EmergencyStopResume();
            leftArmController.EmergencyStopResume();
            rightArmController.EmergencyStopResume(); 
        }
         
    }

}