using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicineGrasping : MonoBehaviour
{

    [SerializeField] private SimplePlanner planner;

    [SerializeField] private List<GameObject> graspableObjects;
    [SerializeField] private List<GameObject> medicineContainers;
    // public List<Transform> waypoints = new List<Transform>();
    private List<Vector3> positionWaypoints = new List<Vector3>();
    private List<Quaternion> rotationnWaypoints = new List<Quaternion>();
    private List<int> instructions = new List<int>();  // deals with how to manage the interaction with the medicine
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
            // Planning and executing the motion
            SetUpWaypoints();
            StartCoroutine(StartAndWaitForMotionToComplete());
        }
        
    }

    // Given our medicine containters and the medince we want to grasp, we need to figure out the waypoints
    public void SetUpWaypoints()
    {

        for(int i = 0; i < graspableObjects.Count; i++)
        {
            // Handle picking up medicine

            Vector3 graspablePosition = graspableObjects[i].transform.position;
            Vector3 hoverSpotPosition = graspableObjects[i].transform.Find("hoverspot").gameObject.transform.position;

            positionWaypoints.Add(hoverSpotPosition);
            positionWaypoints.Add(graspablePosition);
            positionWaypoints.Add(hoverSpotPosition);


            instructions.Add(0);
            instructions.Add(1);
            instructions.Add(0);

            // Handle dropping medicine

            Vector3 hoverSpotMedPosition = medicineContainers[i].transform.Find("hoverspot").gameObject.transform.position;

            positionWaypoints.Add(hoverSpotMedPosition);
            instructions.Add(2);

        }
    }

    IEnumerator StartAndWaitForMotionToComplete()
    {
        // Grab the robot ee position
        Quaternion robotEERotation = planner.armEE.transform.rotation;
        

        for(int i = 0; i < positionWaypoints.Count; i++)
        {
            
            // Set the next goal from our trajectory
            // Set the start position as current EE position
            planner.PlanTrajectory(planner.armEE.transform.position, planner.armEE.transform.rotation, positionWaypoints[i], robotEERotation);

            // Start
            // Debug.Log("Starting Motion");
            // if(i == 0)
            // {
            //     planner.PlanTrajectory(startTF.position, robotEERotation, positionWaypoints[0], robotEERotation);
            // }
            // else{
            //     planner.PlanTrajectory(planner.armEE.transform.position, robotEERotation, positionWaypoints[i], robotEERotation);
            // }
            
            // wait for the motion planner to start
            yield return new WaitUntil(() => planner.motionInProgress == true);

            //wait for motion to be complete before moving on to the next waypoint
            yield return new WaitUntil(() => planner.motionInProgress == false);

            // Debug.Log("End Motion");
            // yield return new WaitForSeconds(1f);

            HandleInstruction(instructions[i]);

            // Wait for the robot to finish the action of open or closing the gripper
            yield return new WaitForSeconds(0.5f);

            if(planner.goalReached == false)
            {
                Debug.Log("Goal not reached, exiting");
                yield break;
            }
        }
    }

    public void HandleInstruction(int i)
    {
        // if (i == 0){} // do nothing
        if(i == 1)
        {
            // grasp the medicine
            planner.armController.CloseGripper();
        }
        else if(i == 2)
        {
            // drop the medicine
            planner.armController.OpenGripper();
            
        }
        else{} // do nothing
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
