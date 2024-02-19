using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileManipulatorHandler : MonoBehaviour
{
    public GameObject arRechability;
    public GenerateARGameObject arGenerator;
    public GameObject gripper;
    public DrawWaypoints drawWaypoints;
    private bool arEnabled = true;

    public ArticulationEndEffectorController leftController;
    public ArticulationEndEffectorController rightController;
    private GameObject left;
    private GameObject right;
    public AutoNavigation autoNavigation;

    public GameObject goal;
    private float replanTime = 1;
    private float timer = 0;

    void Start() 
    {
        // Instantiate two game objects for motion mapping
        left = new GameObject("MotionMappingLeft");
        right = new GameObject("MotionMappingRight");
        arGenerator.Instantiate(
            left,
            gripper
        );
        arGenerator.Instantiate(
            right,
            gripper
        );
    }

    // Update is called once per frame
    void Update()
    {
        // Update left and right
        // left.transform.position = leftController.outPosition;
        // left.transform.rotation = leftController.outRotation;
        // right.transform.position = rightController.outPosition;
        // right.transform.rotation = rightController.outRotation;

        // Check replan
        timer += Time.deltaTime;
        if (timer > replanTime)
        {
            timer = 0;
            autoNavigation.SetGoal(
                goal.transform.position,
                goal.transform.rotation,
                trajectoryReceived
            );
        }

        // key pressed right shift
        if (Input.GetKey(KeyCode.RightShift))
        {
            // move the reachability range
            arEnabled = !arEnabled;
            arRechability.SetActive(arEnabled);
        }
    }

    private void trajectoryReceived(Vector3[] waypoints)
    {
        drawWaypoints.DrawLine(
            "mm-trajectorym",
            waypoints
        );
    }
}
