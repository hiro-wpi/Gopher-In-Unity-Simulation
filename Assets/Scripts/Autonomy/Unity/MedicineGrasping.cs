using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicineGrasping : MonoBehaviour
{

    [SerializeField] private SimplePlanner planner;
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private Transform startTF;

    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Planning And Executing Trajectory");
            // PlanTrajectory(startTF, goalTF);
            StartCoroutine(WaitForMotionToComplete());
        }
        
    }

    IEnumerator WaitForMotionToComplete()
    {
        for(int i = 0; i < waypoints.Count; i++)
        {
            // Start
            if(i == 0)
            {
                planner.PlanTrajectory(startTF, waypoints[0]);
            }
            else{
                planner.PlanTrajectory(waypoints[i-1], waypoints[i]);
            }

            // wait for the motion planner to start
            yield return new WaitUntil(() => planner.motionInProgress == true);

            //wait for motion to be complete before moving on to the next waypoint
            yield return new WaitUntil(() => planner.motionInProgress == false);

            if(planner.goalReached == false)
            {
                Debug.Log("Goal not reached, exiting");
                yield break;
            }
        }
    }
}
