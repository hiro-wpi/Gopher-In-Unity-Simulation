using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class TestNavigation : MonoBehaviour
{
    // Character
    public CharacterNavigation characterNavigation;
    private NavMeshAgent navMeshAgent;

    public Vector3[] humanTrajectory;
    public bool loop;

    private NavMeshPath path;
    public LineRenderer lineRenderer;
    public bool displayPath;

    private float elapsed = 0.0f;

    void Start()
    {
        characterNavigation.SetTrajectory(humanTrajectory);
        characterNavigation.loop = loop;

        navMeshAgent = characterNavigation.agent;

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.positionCount = 0;
        }

        path = new NavMeshPath();
        elapsed = 0.0f;
    }

    void Update()
    {
        // Update the way to the goal every second.
        elapsed += Time.deltaTime;
        if (elapsed > 1.0f)
        {
            elapsed -= 1.0f;
            NavMesh.CalculatePath(navMeshAgent.transform.position, 
                                  new Vector3(1.5f, 0f, 6.0f), 
                                  NavMesh.AllAreas, path);
        }
        for (int i = 0; i < path.corners.Length - 1; i++)
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
        

        if (lineRenderer != null && navMeshAgent.hasPath)
        {
            //DrawPath();
        }
    }

    private void DrawPath()
    {
        lineRenderer.positionCount = navMeshAgent.path.corners.Length;

        for (int i = 0; i < navMeshAgent.path.corners.Length; ++i)
        {
            lineRenderer.SetPosition(i, navMeshAgent.path.corners[i]);
        }
    }
}
