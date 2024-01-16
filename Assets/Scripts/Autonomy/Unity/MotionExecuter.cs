using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionExecuter : MonoBehaviour
{
    /// <summary>
    /// Handle choosing the start and end, send a request to the motion planner, the motion planner will return back a plan, and the executer will start the motion
    ///     It will know how to control the kinova arms properly
    /// </summary>
    /// 

    public MotionPlanner planner;
    public Transform startTF;
    public Transform goalTF;

    public GameObject startNodeGameObject;
    public GameObject goalNodeGameObject;

    public GameObject nodeGameObject;

    private bool isHomed = false;
    public bool requestExecution = false;

    public bool debug = true;


    // Start is called before the first frame update
    void Start()
    {

        // Wait for the robot to be homed
        StartCoroutine(StartUp());

        // requestExecution = true;
        
    }

    // Update is called once per frame
    void Update()
    {

        // if not homed
        if(!isHomed)
        {
           return;

        }

        if(requestExecution == true)
        {
            List<Vector3> pathPositions = new List<Vector3>();
            List<Quaternion> pathQuaternions = new List<Quaternion>();

            Debug.Log("requesting execution");
            
            (pathPositions, pathQuaternions) = GetPlan(startTF.position, startTF.rotation, goalTF.position, goalTF.rotation);

            if(pathPositions != null)
            {
                Debug.Log("We have values. We have " + pathPositions.Count + " waypoints!" );
                foreach(Vector3 position in pathPositions)
                {
                    VisualizeWaypoint(position);
                }
            }
            else
            {
                Debug.Log("No waypoints!");
            }

            requestExecution = false;
        }

    }

    IEnumerator StartUp()
    {
        yield return new WaitForSeconds(3f);
        // planner = GetComponent<MotionPlanner>();
        // planner.Initialize();
        isHomed = true;
    }

    // Sending in the start and goal, get back the plan
    public (List<Vector3>, List<Quaternion>) GetPlan(Vector3 startPosition, Quaternion startRotation, Vector3 goalPosition, Quaternion goalRotation)
    {
        if(debug == true)
        {
            VisualizeStartAndGoal(startPosition, startRotation, goalPosition, goalRotation);
        }
        
        List<Vector3> pathPositions = new List<Vector3>();
        List<Quaternion> pathQuaternions = new List<Quaternion>();

        (pathPositions, pathQuaternions) = planner.GetPlan(startPosition, startRotation, goalPosition, goalRotation);

        return (pathPositions, pathQuaternions);
    }

    // Executes the plan/path
    public void ExecutePlan(List<Vector3> pathPositions, List<Quaternion> pathQuaternions)
    {
        //TODO
    }

    // Display the node in the world
    public void VisualizeWaypoint(Vector3 position)
    {
        Instantiate(nodeGameObject, position, Quaternion.identity, nodeGameObject.transform.parent);
    }

    public void VisualizeStartAndGoal(Vector3 startPosition, Quaternion startRotation, Vector3 goalPosition, Quaternion goalRotation)
    {
        Instantiate(startNodeGameObject, startPosition, startRotation, startNodeGameObject.transform.parent);
        Instantiate(goalNodeGameObject, goalPosition, goalRotation, goalNodeGameObject.transform.parent);
    }

    // Display a set of Nodes relative to any node
    // public void VisualizeActionNodesFromNode(Node nInit)
    // {
    //     List<Node> nodes = localPlanner.GetNodesFromActionList(nInit);
    //     foreach(Node n in nodes)
    //     {
    //         VisualizeNode(n);
    //         // Debug.Log(localPlanner.actionList.Count);
    //     }
    // }

    
}
