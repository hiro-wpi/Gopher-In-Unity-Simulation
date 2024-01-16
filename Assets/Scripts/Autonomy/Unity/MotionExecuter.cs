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

    public GameObject nodeGameObject;


    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> pathPositions = new List<Vector3>();
        List<Quaternion> pathQuaternions = new List<Quaternion>();
        
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Sending in the start and goal, get back the plan
    public (List<Vector3>, List<Quaternion>) GetPlan(Vector3 startPosition, Quaternion startRotation, Vector3 goalPosition, Quaternion goalRotation)
    {
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
