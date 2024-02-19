using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.AI.Navigation;

/// <summary>
///     A Task Runner script, used to run a task.
///     One needs to set the robot and object manually.
///     The scene is also assumed to be already there.
///     
///     The other task runner is TaskLoader.cs.
///     The main difference is that in the other script,
///     the scene, robot, and obejcts and loaded by the script.
/// </summary>
public class TaskRunner : MonoBehaviour
{
    [Header("Task")]
    [SerializeField] private Task task;
    [SerializeField] private GameObject NavMeshGameObject;
    private NavMeshSurface[] navMeshSurfaces;
    private bool taskStarted = false;

    [Header("Robot")]
    [SerializeField] private GameObject robot;
    [SerializeField] private GraphicalInterface GUI;

    [Header("Objects")]
    [SerializeField] private GameObject[] staticObjects = new GameObject[0];
    [SerializeField] private GameObject[] dynamicObjects = new GameObject[0];
    [SerializeField] private GameObject[] taskObjects = new GameObject[0];
    [SerializeField] private GameObject[] goalObjects = new GameObject[0];
    
    void Start()
    {
        navMeshSurfaces = 
            NavMeshGameObject.GetComponentsInChildren<NavMeshSurface>();
        foreach (NavMeshSurface surface in navMeshSurfaces)
        {
            surface.BuildNavMesh();
        }

        task.SetRobots(new GameObject[1] {robot});
        task.SetStaticObjects(staticObjects);
        task.SetDynamicObjects(dynamicObjects);
        task.SetTaskObjects(taskObjects);
        task.SetGoalObjects(goalObjects);

        task.GUI = GUI;
        GUI.SetUIActive(true);
        // This is already done in task.SetRobots()
        // GUI.SetRobot(robot, true);
        // GUI.SetTask(task);
    }

    void Update() 
    {
        // Check if the task starts
        if (!taskStarted && task.CheckTaskStart())
        {
            taskStarted = true;
            // check current task status until completion every 0.5s
            InvokeRepeating("CheckTaskCompletion", 0f, 0.5f);
        }
    }

    private void CheckTaskCompletion() 
    {
        if (task.CheckTaskCompletion())
        {
            // stop
            CancelInvoke("CheckTaskCompletion");
        }
    }
}
