using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.AI.Navigation;

public class TaskSceneTest : MonoBehaviour
{
    [SerializeField] private GameObject robot;
    [SerializeField] private GraphicalInterface GUI;
    [SerializeField] private GameObject NavMeshGameObject;
    private NavMeshSurface[] navMeshSurfaces;
    [SerializeField] private Task task;

    void Start()
    {
        navMeshSurfaces = 
            NavMeshGameObject.GetComponentsInChildren<NavMeshSurface>();
        foreach (NavMeshSurface surface in navMeshSurfaces)
        {
            surface.BuildNavMesh();
        }

        GUI.SetUIActive(true);
        GUI.SetRobot(robot, true);
        GUI.SetTask(task);
    }

    void Update()
    {
        
    }
}
