using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining painting/disinfection task.
/// </summary>
public class PaintingTask : Task 
{
    private PaintShooting paintShooting;
    private Paintable[] paintables;

    void Start()
    {
    }

    void Update() 
    {
        if (paintShooting != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                paintShooting.PlayPainting();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                paintShooting.StopPainting();
            }
        }
    }
    
    public override bool CheckTaskCompletion()
    {
        if (robot == null || paintables == null)
            return false;
        
        // Check if all paintable objects done painting
        for (int i = 0; i < paintables.Length; ++i)
            if (paintables[i].GetCoverage() < 0.99)
            {
                return false;
            }
        
        gUI.ShowPopUpMessage("Current Task Completed!");
        return true;
    }


    // Get current task status
    public override string GetTaskStatus()
    { 
        if (paintables == null || paintables.Length == 0)
            return "";

        // Get all the coverage check results
        string coverageResults = "";
        foreach (Paintable paintable in paintables)
            coverageResults += "\n" + paintable.gameObject.name + ": "
                            + (paintable.GetCoverage() * 100).ToString("0.0") + "%";

        return "Disinfected area: " + 
            coverageResults;
    }


    public override (GameObject[], GameObject[], GameObject[]) GenerateStaticObjects()
    {
        staticObjects = SpawnGameObjectArray(staticObjectSpawnArray);
        taskObjects = SpawnGameObjectArray(taskObjectSpawnArray);
        goalObjects = SpawnGameObjectArray(goalObjectSpawnArray);

        // Painter
        paintShooting = taskObjects[0].GetComponentInChildren<PaintShooting>();
        // attach to end effector (default right)
        GameObject rightEndEffector = robot.GetComponentsInChildren<Grasping>()[1].endEffector.gameObject;
        taskObjects[0].transform.parent = rightEndEffector.transform;
        taskObjects[0].transform.localPosition = Vector3.zero;
        taskObjects[0].transform.localRotation = Quaternion.identity;
        
        // Paintable
        paintables = new Paintable[goalObjects.Length];
        for (int i = 0; i < goalObjects.Length; ++i)
            paintables[i] = goalObjects[i].GetComponentInChildren<Paintable>();
        // highlight
        for (int i = 0; i < goalObjects.Length; ++i)
            HighlightUtils.HighlightObject(goalObjects[i], Color.cyan);

        return (staticObjects, taskObjects, goalObjects);
    }

    public override void DestroyObjects(bool deStatic = true, bool deTask = true,
                                       bool deGoal = true, bool deDynamic = true, 
                                       float delayTime = 0f)
    {
        if (deStatic)
            DestoryGameObjects(staticObjects, delayTime);
        // override, destroy task object (painter/shooter) immediately
        if (deTask)
            DestoryGameObjects(taskObjects, 0f);
        if (deGoal)
            DestoryGameObjects(goalObjects, delayTime);
        if (deDynamic)
            DestoryGameObjects(dynamicObjects, delayTime);
    }
}