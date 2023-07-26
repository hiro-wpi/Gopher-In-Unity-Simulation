using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// <summary>
    // Gets us the status of the collider if a human is in the collider
// </summary>
public class ColliderHandler : MonoBehaviour
{
    // [SerializeField] private Collider collider;

    private int HumanCount = 0;
    public bool ColliderActive = false;
    
    void FixedUpdate()
    {

        if(HumanCount == 0)
        {
            ColliderActive = false;
        }
        else
        {
            ColliderActive = true;
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

    

}