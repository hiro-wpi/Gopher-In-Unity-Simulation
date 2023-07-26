using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// <summary>
    // Gets us the status of the collider if a human is in the collider
// </summary>
public class DiscreteSpeedAndSeperation : MonoBehaviour
{
    
    [SerializeField] private ColliderHandler OuterCollider;
    [SerializeField] private ColliderHandler MiddleCollider;
    [SerializeField] private ColliderHandler InnerCollider;

    public int colliderLayer = 3;
    
    void FixedUpdate()
    {
        if(InnerCollider.ColliderActive)        colliderLayer = 0;
        else if (MiddleCollider.ColliderActive) colliderLayer = 1;
        else if (OuterCollider.ColliderActive)  colliderLayer = 2;
        else                                    colliderLayer = 3;

    }



    

}