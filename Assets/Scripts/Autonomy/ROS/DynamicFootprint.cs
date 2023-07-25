using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// <summary>
//     Handles creating different robot footprints based on different object.
//     So far we consider the medical cart and medical iv pole.
// </summary>
public class DynamicFootprint : MonoBehaviour
{
    [SerializeField] private Transform robotTF; // robotTF
    [SerializeField] private PolygonPublisher polygonPublisher;
    [SerializeField] private Grasping leftArmGrasping;
    [SerializeField] private Grasping rightArmGrasping;
    private float robotFootprintRadius = 0.45f;

    // Handles Looking at Grasped Objects
    private GameObject graspedObject;
    private Transform graspedObjectTF;

    public float previousPosition = 0.0f;
    public float previousRotation = 0.0f;

    // Filtering For laser

    public Laser laserScanner;

    // Flag for the state of the base
    private bool usingNormalFootprint = true;

    // Public

    public void Start()
    {
        // Init Footprint
        StartCoroutine(InitFootprint());
    }

    IEnumerator InitFootprint()
    {
        yield return new WaitForFixedUpdate();
        SetToNormalFootprint();
        usingNormalFootprint = true;
    }

    public void Update()
    {

        if(leftArmGrasping.IsGrasping | rightArmGrasping.IsGrasping)
        {
            bool changingFootprint = false;

            // Getting the Grasped Object
            if(leftArmGrasping.IsGrasping)
            {
                graspedObject = leftArmGrasping.GetGraspedObject();
            }
            else if(rightArmGrasping.IsGrasping)
            {
                graspedObject = rightArmGrasping.GetGraspedObject();
            }

            // Pass the grasped game object to the laser scanner
            laserScanner.filteredGraspedObject = graspedObject;

            // Gets the pos and rot relative to the robot frame
            (float pos, float rot) = GetGraspedObjectPoseShift(Vector3.zero, Quaternion.identity, graspedObject.transform);
            
            // Figures out if the footprint should be changed
            if(previousPosition == 0.0f | previousRotation == 0.0f)
            {
                (previousPosition, previousRotation) = GetGraspedObjectPoseShift(Vector3.zero, Quaternion.identity, graspedObject.transform);
                changingFootprint = true;
            }
            else if(Math.Abs(pos - previousPosition) > 0.05 | Math.Abs(rot - previousRotation) > 10.0f)
            {
                previousPosition = pos;
                previousRotation = rot;
                changingFootprint =  true;
            }

            // if the footprint should be changed, update accordingly based on the object
            if(changingFootprint)
            {
                graspedObjectTF = graspedObject.GetComponent<Transform>();

                if(GameObjectHasName(graspedObject, "IV Pole Movable"))
                {
                    Vector3[] polygon = GetMedicalIVFootprint(graspedObjectTF);
                    UpdateFootprint(polygon);
                    usingNormalFootprint = false;
                }

                // Change the footprint of the Cart
                if(GameObjectHasName(graspedObject, "Service Cart Movable"))
                {
                    // Debug.Log("Service Cart");
                    Vector3[] polygon = GetMedicalCartFootprint(graspedObjectTF);
                    UpdateFootprint(polygon);
                    usingNormalFootprint = false;
                }
            }
        }
        else
        {
            if(usingNormalFootprint == false)
            {   
                // Sets the footprint to normal
                SetToNormalFootprint();
                usingNormalFootprint = true;

                // Reset the laser scanner
                laserScanner.filteredGraspedObject = null;

                // Makes sure that no object is being
                previousPosition = 0.0f;
                previousRotation = 0.0f;
            }
        }
    }

    public void OnSetNormalFootprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SetToNormalFootprint();
        }
    }

    public void OnSetCartFootprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SetToBaseWithCartFootprint();
        }   
    }

    public void OnSetIVFootprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SetToBaseWithIVFootprint();
        }   
    }

    public void UpdateFootprint(Vector3[] poly)
    {
        polygonPublisher.PublishPolygon(poly);
    }

    // Test Functions 
    public void SetToNormalFootprint()
    {   
        Vector3[] polygon = GetRobotFootprint();
        // Debug.Log(polygon);
        UpdateFootprint(polygon);
    }

    public void SetToBaseWithCartFootprint()
    {   
        // Test
        GameObject cartGameObject = new GameObject("cartTest");
        Transform cartTF = cartGameObject.GetComponent<Transform>();
        cartTF.SetParent(robotTF, false);
        cartTF.Translate(new Vector3(0f, 0.0f, 0.7f));
        cartTF.Rotate(new Vector3(0, 0, 0));

        Vector3[] polygon = GetMedicalCartFootprint(cartTF);

        UpdateFootprint(polygon);
    }

    public void SetToBaseWithIVFootprint()
    {   
        // Test
        GameObject ivGameObject = new GameObject("ivTest");
        Transform ivTF = ivGameObject.GetComponent<Transform>();
        ivTF.SetParent(robotTF, false);
        ivTF.Translate(new Vector3(0.2f, 0.0f, 0.1f));
        ivTF.Rotate(new Vector3(0, 0, 0));

        Vector3[] polygon = GetMedicalIVFootprint(ivTF);

        UpdateFootprint(polygon);
    }

    // Private 

    // Checks to see if the pose of the object shifted
    private (float, float) GetGraspedObjectPoseShift(Vector3 prevPosition, Quaternion prevRotation, Transform tf)
    {
        // Gets the difference in the position
        Vector3 position = robotTF.InverseTransformPoint(tf.position);
        float distance = Vector3.Distance(prevPosition, position); 

        // Get the difference in the rotation from a to b
        Quaternion rotation = (Quaternion.Inverse(robotTF.rotation) * tf.rotation);
        float diffAngle = Math.Abs(rotation.eulerAngles.y - prevRotation.eulerAngles.y);

        return (distance, diffAngle);
    }

    private Vector3[] GetMedicalCartFootprint(Transform cartTF) 
    {
        Vector3[] points = GetRectangle(0.74f, 1.18f);      // cart wrt cart
        Vector3[] points_ = new Vector3[points.Length];     // cart wrt robot
        Vector3[] robotFootprint = GetRobotFootprint();
        

        for(int i = 0; i < points.Length; i++)
        {
            // Transform Cart Points from Cart to Robot Coordinates
            points_[i] = robotTF.InverseTransformPoint(cartTF.TransformPoint(points[i]));
        }
        
        // Assume that the polygon of the robot are in robot local coordinates
        Vector3[] newFootprint = new Vector3[points_.Length + 7];

        // Combine
        Array.Copy(points_, 0, newFootprint, 0, 4);
        Array.Copy(robotFootprint, 1, newFootprint, 4, 7);

        return newFootprint;
    }

    private Vector3[] GetMedicalIVFootprint(Transform ivTF) 
    {   
        // Create a temp game object
        GameObject iv_ = new GameObject();
        Transform ivTF_ = iv_.GetComponent<Transform>();
        ivTF_.Translate(new Vector3(ivTF.transform.position.x, 0.0f, ivTF.transform.position.z));
        ivTF_.Rotate(new Vector3(0.0f, ivTF.transform.rotation.y, 0.0f));

        // Transform                      
        // Transform robotTF = robot.GetComponent<Transform>();
        Vector3 ivPosition = robotTF.InverseTransformPoint(ivTF_.position);  // wrt to robot

        // Getting the pair shaped based on the distance
        Vector3[] pearShape = GetPearShape(ivPosition.magnitude, robotFootprintRadius, 0.33f);
        Vector3[] newFootprint = new Vector3[pearShape.Length];

        // Quaternion.FromToRotation(robotTF.forward, 
        //                         new Vector3(ivPosition.x,
        //                                 0.00f,
        //                                 ivPosition.z);

        // Rotating the pear
        Matrix4x4 rotation = Matrix4x4.TRS(Vector3.zero, 
                                           ivTF_.localRotation,
                                           Vector3.one);

        for(int i = 0; i < pearShape.Length; i++)
        {
            newFootprint[i] = rotation.MultiplyPoint3x4(pearShape[i]);
        }

        // Delete the temp object
        Destroy(iv_);

        return newFootprint;



        
    }

    private Vector3[] GetRobotFootprint()
    {
        return GetPolygon(8, robotFootprintRadius);
    }
    // Static 

        // Get Polygon
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


        // Debug.Log(pointsReorganized[0]);

        return pointsReorganized;
    }
        
        // Get PearShapedPolygon
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
        // Get Rectangle

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


    private static bool GameObjectHasName(GameObject gameobject,  string name)
    {
        string objectName = gameobject.name;
        if(objectName.IndexOf(name) > -1)
        {
            return true;
        }
        return false;


    }
}
