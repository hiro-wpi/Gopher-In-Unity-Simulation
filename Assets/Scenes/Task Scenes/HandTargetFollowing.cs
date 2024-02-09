using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTargetFollowing : MonoBehaviour
{
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform leftHandHome;
    [SerializeField] private Transform rightHandHome;
    [SerializeField] private Transform leftHandRaise;

    void Update()
    {
        // // Set the left hand target to follow the left hand home
        // leftHandTarget.position = leftHandHome.position;

        // // Set the right hand target to follow the right hand home
        // rightHandTarget.position = rightHandHome.position;

        // Set the left hand target to follow the left hand raise
        leftHandTarget.position = leftHandRaise.position;
    }
}