using System;
using System.IO;
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
    [SerializeField] private DataRecorder recorder;

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

        // Set GUI
        task.GUI = GUI;
        GUI.SetUIActive(true);
        // This is already done in task.SetRobots()
        // GUI.SetRobot(robot, true);
        // GUI.SetTask(task);

        // Set Robot and Objects
        task.SetRobots(new GameObject[1] { robot });
        task.SetStaticObjects(staticObjects);
        task.SetDynamicObjects(dynamicObjects);
        task.SetTaskObjects(taskObjects);
        task.SetGoalObjects(goalObjects);

        // Set Recorder
        recorder.SetRobot(robot);
        recorder.SetObjects(taskObjects);
    }

    void Update()
    {
        // Check if the task starts
        if (!taskStarted && task.CheckTaskStart())
        {
            taskStarted = true;

            // Start recording
            string recordFolder =
                Application.dataPath
                + "/Data/";
            if (!Directory.Exists(recordFolder))
            {
                Directory.CreateDirectory(recordFolder);
            }
            recorder.StartRecording(
                recordFolder
                + task.TaskName
                + "_"
                + DateTime.Now.ToString("MM-dd HH-mm-ss"),
                task
            );

            // Check current task status until completion every 0.5s
            InvokeRepeating("CheckTaskCompletion", 0f, 0.5f);
        }
    }

    private void CheckTaskCompletion()
    {
        if (task.CheckTaskCompletion())
        {
            // stop recording
            recorder.StopRecording();
            // stop
            CancelInvoke("CheckTaskCompletion");
        }
    }

    void OnDestroy()
    {
        // stop recording in case the task is never completed
        recorder.StopRecording();
    }
}
