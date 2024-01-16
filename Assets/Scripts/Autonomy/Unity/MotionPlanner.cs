using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionPlanner : MonoBehaviour
{
    /// <summary>
    /// Only Goal is to plan out the route
    ///     Use the debug game objects to be able to see what is happening with the planner
    ///     All plans should be based on a request by the motion executer
    ///     Assume the start and the goal are defined, and we need only to concern ourselves with making sure the path created is collision free (relative to the end effector)
    /// </summary>

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public (List<Vector3>, List<Quaternion>) GetPlan(Vector3 startPosition, Quaternion startRotation, Vector3 goalPosition, Quaternion goalRotation)
    {
        List<Vector3> pathPositions = new List<Vector3>();
        List<Quaternion> pathQuaternions = new List<Quaternion>();

        

        return (pathPositions, pathQuaternions);
    }


    ///---------------------------------------------Node----------------------------------------------------///
    private class Node
    {
        Vector3 position;
        Quaternion rotation;
        
        Node previousNode = null;

        // Normal Constructors
        public Node()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }

        public Node(Vector3 p, Quaternion r)
        {
            position = p;
            rotation = r;
        }

        // Contructor Just for RRT
        public Node(Node n, Vector3 deltaPosition)
        {
            position = n.position + deltaPosition;
            rotation = n.rotation; 
            previousNode = n;
        }

        public float GetDistance(Node n)
        {
            return Vector3.Distance(position, n.position);
        }

        // Recursively from one node get the set of nodes to the starting node
        public List<Vector3> GetPath()
        {
           
            if(previousNode == null)  //Terminating Condition
            {
                //Terminating
                return new List<Vector3>();
            }
            else
            {
                //Recursive
                List<Vector3> path = previousNode.GetPath();
                path.Add(position);
                return path;
            }
        }

    }

    ///---------------------------------------------RRT Tree-----------------------------------------------------///
    private class RRTTree
    {
        public List<Node> tree;
        public Node goalNode;

        private int rrtMaxSize = 200;

        private List<Vector3> actions;

        RRTTree(Node start, Node goal)
        {
            // Contruct the tree, initcializing it with a node
            tree = new List<Node>{start};
            goalNode = goal;
            actions = GetActions();
        }

        // Adds a node into the search tree 
        // void Add(Node n)
        // {
        //     tree.Add(n);
        // }


        // Find the nearest node in the tree to the sampleNode
        public Node FindNearest(Node sampleNode)
        {
            return FindNearest(tree, sampleNode);
        }

        // Find the nearest node in the list of nodes
        public Node FindNearest(List<Node> nodeList, Node sampleNode)
        {
            float calcNearValue(Node n)
            {
                float dist = Mathf.Abs(n.GetDistance(sampleNode));
                // float angle = Mathf.Abs(n.pose.GetAngle(sampleNode.pose));
                // 
                return dist;
            }

            // Find the nearest node out of the possibleNodes based on the calcNearValue
            Node nearestNode = nodeList[0];
            float smallestNValue = calcNearValue(nearestNode);
            foreach(Node n in nodeList)
            {
                float nValue = calcNearValue(n);
                if(nValue < smallestNValue)
                {
                    smallestNValue = nValue;
                    nearestNode = n;
                }
            }
            return nearestNode;
        }

        // Get a random sampled point or the goal
        public Node RandomSample()
        {
             // Randomly pass in the goal position or a random point
            float goalFrequency = 0.5f;
            if(Random.Range(0f, 1f) < goalFrequency)
            {
                // pass in the goal:
                return goalNode;
            }
            else
            {
                // Select a random point from our considered space
                // Get a random position in front of the robot

                float r = Random.Range(0f, 1f); // meters


                float theta = Random.Range(-180f, 180f) * Mathf.Deg2Rad; // degrees -> rad
                float deltaX = r*Mathf.Cos(theta);
                float deltaZ = r*Mathf.Sin(theta);

                return new Node( new Vector3(deltaX, 0, deltaZ), Quaternion.identity);
            }
        }

        // From the nearert Node, get the newnest closest node to the sample node
        public Node GetNearestNewNode(Node nearestNode, Node sampleNode)
        {
            // Create a list of all the possible resulting states
            List<Node> possibleNodes = new List<Node>();
            foreach(Vector3 action in actions)
            {
                possibleNodes.Add(new Node(nearestNode, action));
            }

            // Find the state that is closes to the sampleNode
            return FindNearest(possibleNodes, sampleNode);
        }

        private List<Vector3> GetActions()
        {
            List<Vector3> list = new List<Vector3>();
            float[] deltaActions = {-0.4f, -0.2f, 0f, 0.2f, 0.4f};

            foreach(float deltaX in deltaActions)
            {
                foreach(float deltaY in deltaActions)
                {
                    foreach(float deltaZ in deltaActions)
                    {
                        // The action is generated, which matches what the robot EE accepts as inputs
                        Vector3 action = new Vector3(deltaX, deltaY, deltaZ);
                        list.Add(action);
                    }
                }
            }

            return list;
        }

        // Is the path from the previous Node to the current Node Collision Free?
        private bool IsPathCollisionFree(Node n)
        {
            return true;
        }

        // Returns if we are close to the goal
        private bool IsAtGoal(Node n)
        {
            float distance = n.GetDistance(goalNode);
            return distance <= 0.2;
        }

        // Exspand and Search in the RRT tree for the solution
        public List<Vector3> Search()
        {
            // Tree is already initcialized in the contructor, we have start and goal
            while(tree.Count <= rrtMaxSize)
            {
                // Sample a random position
                Node sampleNode = RandomSample();
                // Get the nearest Node
                Node nearestNode = FindNearest(sampleNode);
                // Get a new pose closest to the sample node that is actionable
                Node newNode = GetNearestNewNode(nearestNode, sampleNode);

                if(IsPathCollisionFree(newNode))
                {   
                    tree.Add(newNode);
                    if(IsAtGoal(newNode))
                    {
                        // Successful, we found a path/plan
                        // return newNode;
                        return newNode.GetPath();
                    }
                }
            }

            // Failed, no path found in our conditions
            return null;
        }


    }

    

}
