using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class TestAutoNavigation : MonoBehaviour
{
    // Character
    public AutoNavigation autoNavigation;
    public GameObject target;
    private Vector3 goal;

    void Start()
    {
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            autoNavigation.EnableAutonomy();
            autoNavigation.drawPathEnabled = true;
            goal = target.transform.position;

            autoNavigation.SetGoal(goal);
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            autoNavigation.DisableAutonomy();
            autoNavigation.drawPathEnabled = false;
        }
    }
}
