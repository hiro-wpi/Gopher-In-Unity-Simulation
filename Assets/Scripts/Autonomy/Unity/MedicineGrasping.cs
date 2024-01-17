using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicineGrasping : MonoBehaviour
{

    [SerializeField] private SimplePlanner planner;
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private List<int> instructions;  // deals with how to manage the interaction with the medicine
        // if 0, then do nothing
        // if 1, then move forward to the object, grasp, and go back to the hover spot
        // if 2, drop the object
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
                planner.PlanTrajectory(GraspableTransform(startTF), GraspableTransform(waypoints[0]));
            }
            else{
                planner.PlanTrajectory(GraspableTransform(waypoints[i-1]), GraspableTransform(waypoints[i]));
            }

            // wait for the motion planner to start
            yield return new WaitUntil(() => planner.motionInProgress == true);

            //wait for motion to be complete before moving on to the next waypoint
            yield return new WaitUntil(() => planner.motionInProgress == false);

            HandleInstruction(instructions[i]);

            if(planner.goalReached == false)
            {
                Debug.Log("Goal not reached, exiting");
                yield break;
            }
        }
    }

    public void HandleInstruction(int i)
    {
        if(i == 0)
        {
            // do nothing
            Debug.Log("Do Nothing");
        }
        else if(i == 1)
        {
            // grasp the medicine
            Debug.Log("Grasp Medicine");
            planner.armController.CloseGripper();
            
        }
        else if(i == 2)
        {
            // drop the medicine
            Debug.Log("Drop Medicine");
            planner.armController.OpenGripper();
            
        }
    }

    private Transform GraspableTransform(Transform objectTransform)
    {
        // Change the orientation of the waypoint to be the same orientation as the robot end effector
        // This is so that the robot can grasp the object
        Transform graspableTransform = objectTransform;
        graspableTransform.rotation = planner.armEE.transform.rotation;

        return graspableTransform;
    }

    
}
