using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.SceneManagement;

public class RunTask : MonoBehaviour
{
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
        DontDestroyOnLoad(gameObject);

        SceneManager.LoadScene(task.SceneName);
        yield return new WaitForSeconds(0.3f);

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
