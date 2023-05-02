using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class TestAutoNavigation : MonoBehaviour
{
    // Character
    [SerializeField] private UnityAutoNavigation autoNavigation;
    [SerializeField] private  GameObject target;
    private Vector3 goal;

    void Start() {}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            autoNavigation.ResumeNavigation();
            goal = target.transform.position;

            autoNavigation.SetGoal(goal);
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            autoNavigation.StopNavigation();
        }
    }
}
