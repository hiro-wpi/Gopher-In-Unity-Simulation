using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicineGrasping : MonoBehaviour
{

    [SerializeField] private SimplePlanner planner;

    [SerializeField] private List<GameObject> graspableObjects;
    [SerializeField] private List<GameObject> medicineContainers;
    // public List<Transform> waypoints = new List<Transform>();
    [SerializeField] private List<Vector3> positionWaypoints = new List<Vector3>();
    [SerializeField] private List<Quaternion> rotationnWaypoints = new List<Quaternion>();
    public List<int> instructions = new List<int>();  // deals with how to manage the interaction with the medicine
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
            SetUpWaypoints();
            StartCoroutine(WaitForMotionToComplete());
        }
        
    }

    // Given our medicine containters and the medince we want to grasp, we need to figure out the waypoints
    public void SetUpWaypoints()
    {
        foreach(GameObject graspableObject in graspableObjects)
        {
            // Handle picking up medicine

            Vector3 graspablePosition = graspableObject.transform.position;
            Vector3 hoverSpotPosition = graspableObject.transform.Find("hoverspot").gameObject.transform.position;

            // waypoints.Add(hoverSpotPosition);
            // waypoints.Add(graspablePosition);
            // waypoints.Add(hoverSpotPosition);
            positionWaypoints.Add(hoverSpotPosition);
            positionWaypoints.Add(graspablePosition);
            positionWaypoints.Add(hoverSpotPosition);


            instructions.Add(0);
            instructions.Add(1);
            instructions.Add(0);

            // Handle dropping medicine

            Vector3 hoverSpotMedPosition = medicineContainers[0].transform.Find("hoverspot").gameObject.transform.position;

            // waypoints.Add(hoverSpotMedPosition);
            positionWaypoints.Add(hoverSpotMedPosition);
            instructions.Add(2);

        }
    }

    IEnumerator WaitForMotionToComplete()
    {
        // Grab the robot ee position
        Quaternion robotEERotation = planner.armEE.transform.rotation;

        for(int i = 0; i < positionWaypoints.Count; i++)
        {
            // Start
            Debug.Log("Starting Motion");
            if(i == 0)
            {
                planner.PlanTrajectory(startTF.position, robotEERotation, positionWaypoints[0], robotEERotation);
            }
            else{
                planner.PlanTrajectory(positionWaypoints[i-1], robotEERotation, positionWaypoints[i], robotEERotation);
            }
            
            // wait for the motion planner to start
            yield return new WaitUntil(() => planner.motionInProgress == true);

            //wait for motion to be complete before moving on to the next waypoint
            yield return new WaitUntil(() => planner.motionInProgress == false);

            // Debug.Log("End Motion");

            HandleInstruction(instructions[i]);

            yield return new WaitForSeconds(0.2f);

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
