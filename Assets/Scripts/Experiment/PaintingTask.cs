using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for defining painting/disinfection task.
/// </summary>
public class PaintingTask : Task 
{
    private PaintShooting paintShooting;
    private Paintable paintable;

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
        if (paintable != null && paintable.GetCoverage() > 0.95)
            return true;
        else
            return false;
    }


    // Get current task status
    public override string GetTaskStatus()
    { 
        if (paintable == null)
            return "";

        return "The disinfected area percentage is: " + 
               paintable.GetCoverage();
    }


    public override (GameObject[], GameObject[], GameObject[]) GenerateStaticObjects()
    {
        base.GenerateStaticObjects();

        // Painter
        paintShooting = taskObjects[0].GetComponentInChildren<PaintShooting>();
        // Find end effector (default right)
        GameObject rightEndEffector = robot.GetComponent<StateReader>().extraObjects[1];
        taskObjects[0].transform.parent = rightEndEffector.transform;
        // Paintable
        paintable = taskObjects[1].GetComponentInChildren<Paintable>();

        return (staticObjects, taskObjects, goalObjects);
    }
}