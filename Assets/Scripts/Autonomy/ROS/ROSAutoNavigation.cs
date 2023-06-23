using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;

/// <summary>
///     Autonomy for 2D navigation.
///
///     The global planner references the 
///     navigation_stack for global planning.
///
///     The local planning uses dwa or teb local planners
/// </summary>
public class ROSAutoNavigation : AutoNavigation
{
    // Move base services
    [SerializeField] private MoveBaseCancelGoalService cancelGoalService;
    [SerializeField] private MoveBaseSendGoalService sendGoalService;

    // Local and global planners
    [SerializeField] private PathSubscriber pathPlanner;

    // AMCL
    [SerializeField] private PoseWithCovarianceStampedPublisher poseWithCovarianceStampedPublisher;

    [SerializeField] private TwistSubscriber twistSubscriber;
    [SerializeField] private PolygonPublisher polygonPublisher;

    [SerializeField] private GameObject robot;

    // Flags
    private bool isTwistSubscriberPaused = false;
    private bool isInitcialPosePublished = false;

    // Robot Radius
    private float robotFootprintRadius = 0.45f;



    private bool updateWaypoints = true;

    void Start()
    {   
        StartCoroutine(PauseTwistSubscriber());
        StartCoroutine(PublishInitcialPose());
    }

    // Pauses the Twist Subscriber from controlling the base given Twist Commands
    IEnumerator PauseTwistSubscriber()
    {
        yield return new WaitForFixedUpdate();

        // Pause Twist Subscriber
        twistSubscriber.Pause(true);
        isTwistSubscriberPaused = true;
    }

    // Publishes the Initcial pose of the robot to AMCL
    IEnumerator PublishInitcialPose()
    {
        yield return new WaitForFixedUpdate();

        // Send the initcial Pose
        poseWithCovarianceStampedPublisher.PublishPoseStampedCommand(robot.transform.position, robot.transform.rotation.eulerAngles);
        isInitcialPosePublished = true;
    }


    void Update()
    {
        
        // Initcialize twist subsciber
        if(isTwistSubscriberPaused == false)
        {
            return;
        }
        
        // Publish initial pose
        if(isInitcialPosePublished == false)
        {
            return;
        }

        if(updateWaypoints)
        {
            GlobalWaypoints = pathPlanner.getGlobalWaypoints();
            LocalWaypoints = pathPlanner.getLocalWaypoints();
        }
        
    }    

    // Set goal, regardless of the goal orientation
    public override void SetGoal(Vector3 position)
    {
        SetGoal(position, new Vector3(0,0,0));
    }

    // Set goal, with goal orientation
        // Local and global waypoints are displayed based on the sent goal
        // The robot doesn't move 
    public override void SetGoal(Vector3 position, Vector3 orientation)
    {
        TargetPosition = position;
        TargetOrientationEuler = orientation;

        sendGoalService.SendGoalCommandService(position, orientation);
        UpdateNav(false);
        updateWaypoints = true;
    }

    // Start, pause and resume navigation
    // Start is essentially the same as resume
        // Allow the robot to move along the planned waypoints
    public override void StartNavigation()
    {
        UpdateNav(true);
        updateWaypoints = true;
    }

    // Pause robot navigation
        // Stop the motion of the robot. Still allow the waypoints to be seen
    public override void PauseNavigation()
    {
        UpdateNav(false);
        updateWaypoints = true;
    }

    // Resume navigation
    public override void ResumeNavigation()
    {
        UpdateNav(true);
        updateWaypoints = true;
    }

    // Stop navigation, clear previous plan
    public override void StopNavigation()
    {
        cancelGoalService.CancelGoalCommand();
        GlobalWaypoints = new Vector3[0];
        LocalWaypoints = new Vector3[0];
        UpdateNav(false);
        updateWaypoints = false;
    }

    // Update the navigation status of the robot, and pause listening to the twist subsriber
    private void UpdateNav(bool isNav)
    {
        IsNavigating = isNav;
        twistSubscriber.Pause(!isNav);
    }

    // Update the footprint of the robot used in the navigation stack by the DWA local planner
    public void UpdateFootprint(Vector3[] poly)
    {
        polygonPublisher.PublishPolygon(poly);
    }

    // Test Functions 
    public void SetToNormalFootprint()
    {   
        Vector3[] polygon = GetRobotFootprint();
        Debug.Log(polygon);
        UpdateFootprint(polygon);
    }

    public void SetToBaseWithCartFootprint()
    {   
        // Test
        GameObject cartGameObject = new GameObject("cartTest");
        Transform cartTf = cartGameObject.GetComponent<Transform>();
        cartTf.Translate(new Vector3(0f, 0, 0.5f), Space.World);
        cartTf.Rotate(new Vector3(0, 0, 0), Space.World);
        Vector3[] polygon = GetMedicalCartFootprint(cartTf);

        UpdateFootprint(polygon);
    }

    public void SetToBaseWithIVFootprint()
    {   
        // Test
        GameObject ivGameObject = new GameObject("ivTest");
        Transform ivTf = ivGameObject.GetComponent<Transform>();
        ivTf.Translate(new Vector3(0.2f, 0.1f, 0.5f), Space.World);
        ivTf.Rotate(new Vector3(0, 0, 0), Space.World);
        Vector3[] polygon = GetMedicalIVFootprint(ivTf);

        UpdateFootprint(polygon);
    }

    // dynamic footprint based on the location of the medical cart
    private Vector3[] GetMedicalCartFootprint(Transform cartTF) 
    {
        Vector3[] points = GetRectangle(0.74f, 1.18f);      // cart wrt cart
        Vector3[] points_ = new Vector3[points.Length];                 // cart wrt robot
        Vector3[] robotFootprint = GetRobotFootprint();
        Transform robotTF = robot.GetComponent<Transform>();

        for(int i = 0; i < points.Length; i++)
        {
            // Transform Cart Points from Cart to Robot Coordinates
            //      Local(Cart) to World
            //      World to Local(Robot)
            points_[i] = robotTF.InverseTransformPoint(cartTF.TransformPoint(points[i]));
        }
        
        // Assume that the polygon of the robot are in robot local coordinates
        Vector3[] newFootprint = new Vector3[points_.Length + 7];

        // Combine
        Array.Copy(points_, 0, newFootprint, 0, 4);
        Array.Copy(robotFootprint, 1, newFootprint, 4, 7);

        Debug.Log(newFootprint);
        return newFootprint;
    }

    private Vector3[] GetMedicalIVFootprint(Transform ivTF) 
    {   
        // Transform                      
        Transform robotTF = robot.GetComponent<Transform>();
        Vector3 ivPosition = robotTF.InverseTransformPoint(ivTF.position);  // wrt to robot

        // Getting the pair shaped based on the distance
        Vector3[] pearShape = GetPearShape(ivPosition.magnitude, robotFootprintRadius, 0.33f);
        Vector3[] newFootprint = new Vector3[pearShape.Length];

        // Rotating the pear
        Matrix4x4 rotation = Matrix4x4.TRS(Vector3.zero, 
                                           Quaternion.FromToRotation(robotTF.forward, 
                                                                     new Vector3(ivPosition.x,
                                                                                0.00f,
                                                                                ivPosition.z)),
                                           Vector3.one);

        for(int i = 0; i < pearShape.Length; i++)
        {
            newFootprint[i] = rotation.MultiplyPoint3x4(pearShape[i]);
        }

        return newFootprint;



        
    }

    private Vector3[] GetRobotFootprint()
    {
        return GetPolygon(8, robotFootprintRadius);
    }

    // STATIC METHODS

    // Get the Rectangular Polygon
    private static Vector3[] GetRectangle(float width, float length) 
    {
        // Points are organized counter-clockwise
        Vector3[] poly = {
            new Vector3( width/2, 0, -length/2),
            new Vector3( width/2, 0,  length/2),
            new Vector3(-width/2, 0,  length/2),
            new Vector3(-width/2, 0, -length/2)
        };
        return poly;
    }

    private static Vector3[] GetPearShape(float distance, float baseApothem, float objectApothem)
    {
        // Get circles based on each appthem
        
        Vector3[] baseCircle = GetPolygon(8, baseApothem);   // assume this to be the larger size
        Vector3[] objectCircle = GetPolygon(8, objectApothem);
        Vector3[] objectCircle_ = new Vector3[objectCircle.Length];
        Vector3[] pearShapedPolygon = new Vector3[10]; 

        Matrix4x4 translation = Matrix4x4.TRS(new Vector3(0f, 0f, distance),
                                              Quaternion.identity,
                                              Vector3.one);

        // Translate the object circle by distance d forward
        for(int i = 0; i < 8; i++)
        {
            objectCircle_[i] = translation.MultiplyPoint3x4(objectCircle[i]);
        }

        Array.Copy(baseCircle, 2, pearShapedPolygon, 0, 5);
        Array.Copy(objectCircle_, 6, pearShapedPolygon, 5, 2);
        Array.Copy(objectCircle_, 0, pearShapedPolygon, 7, 3);

        return pearShapedPolygon;
    }

    // Get a Regular Polygon using the apothem
    private static Vector3[] GetPolygon(int sides, float apothem)
    {
        // apothem - radius of the incircle of a polygon
        // radius - radius of the circumscribed circle

        Vector3[] points = new Vector3[sides];

        float angle = 2 * Mathf.PI / sides;

        // Gets the radius of the circumscribed circle
        float radius = apothem / Mathf.Cos(angle / 2);

        // All points of a regular polygon lie on the circumscribed circle
        // Points are organized counter-clockwise
        for (int i = 0; i < sides; i++)
        {
            float x = radius * Mathf.Cos((float)(i * angle));
            float z = radius * Mathf.Sin((float)(i * angle));
            points[i] = new Vector3(x, 0, z);
        }

        // Reoriganizing the array
        Vector3[] pointsReorganized = new Vector3[sides];
        Array.Copy(points, 2, pointsReorganized, 0, 6);
        Array.Copy(points, 0, pointsReorganized, 6, 2);


        Debug.Log(pointsReorganized[0]);

        return pointsReorganized;
    }

}
