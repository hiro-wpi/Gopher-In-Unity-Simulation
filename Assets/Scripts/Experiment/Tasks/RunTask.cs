using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.AI.Navigation;

/// <summary>
///     A Task Loader script, used to run a task.
///     The scene, robot, and obejcts and loaded by the script.
///     
///     The other task runner is TaskRunner.cs.
///     The main difference is that in the other script,
///     one needs to set the robot and objects manually,
///     and the scene is also assumed to be already there.
/// </summary>
// TaskLoader
public class RunTask : MonoBehaviour
{
    [Header("Task")]
    [SerializeField] private Task task;
    [SerializeField] private GameObject NavMeshGameObject;
    private NavMeshSurface[] navMeshSurfaces;
    private bool taskStarted = false;

    void Start()
    {
        navMeshSurfaces = 
            NavMeshGameObject.GetComponentsInChildren<NavMeshSurface>();
        StartCoroutine(LoadTaskCoroutine());
    }

    private IEnumerator LoadTaskCoroutine()
    {
        Time.timeScale = 1f;

        // Load scene
        task.LoadScene();
        yield return new WaitForSeconds(0.5f);

        // Generate static objects
        task.GenerateStaticObjects();
        yield return new WaitForSeconds(0.2f);

        foreach (NavMeshSurface surface in navMeshSurfaces)
        {
            surface.BuildNavMesh();
        }

        // Generate task and goal objects
        task.GenerateTaskObjects();
        task.GenerateGoalObjects();
        yield return new WaitForSeconds(0.2f);

        // Generate robots
        task.GenerateRobots();
        yield return new WaitForSeconds(0.5f);

        // Generate dynamic objects
        task.GenerateDynamicObjects();

        // UI
        task.GUI.SetUIActive(true);
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
