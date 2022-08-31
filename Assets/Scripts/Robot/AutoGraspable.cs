using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script denotes an auto graspable object
///     
/// </summary>
public class AutoGraspable : MonoBehaviour
{
    // Mod for different grasp
    public Transform grabPoint;
    private Transform grabTransform; // to return
    private Transform hoverTransform; // to return
    public float hoverHeight = 0.1f;
    public HoverDirection hoverDirection;
    public HoverOrientation hoverOrientation;
    // Two different modes
    // single X/Y/Z offset + 2/4 Orientation
    // Or     X/Y/Z Plane + YorZ/XorZ/XorY as forward
    public enum HoverDirection
    {
        X,
        Y,
        Z,
        XPlane,
        YPlane,
        ZPlane
    }
    public enum HoverOrientation
    {
        OneOrientaion,
        TwoOrientations,
        FourOrientations,
        PlaneForwardX,
        PlaneForwardY,
        PlaneForwardZ
    }

    void Start()
    {}

    
    // Core
    // Get hover point for grasping based on Hover Direction and Orientation
    public (Transform, Transform) GetHoverAndGrapPoint(Vector3 EndEffectorPosition,
                                                       Quaternion EndEffectorRotation)
    {
        // Init
        if (hoverTransform == null)
        {
            hoverTransform = new GameObject("HoverTransform").transform;
            hoverTransform.parent = transform;
        }
        if (grabTransform == null)
        {
            grabTransform = new GameObject("GrabTransform").transform;
            grabTransform.parent = transform;
        }
        grabTransform.position = grabPoint.position;
        grabTransform.rotation = grabPoint.rotation;
        
        // select plane and forward direction
        int selectedDirIndex = (int) hoverDirection;
        int selectedOriIndex = (int) hoverOrientation;
        Vector3 posOffset = Vector3.zero;
        Vector3 rotationDirection = Vector3.zero;


        // TYPE 1 //
        // Single hover point with multiple orientations
        if (selectedDirIndex <= 2)
        {
            // assert proper mode
            Debug.Assert(selectedOriIndex < 3, "Please select proper orientation mode");

            // compute offset and rotation direction
            posOffset = Vector3.zero;
            posOffset[selectedDirIndex] = hoverHeight;
            hoverTransform.position = grabPoint.position + grabPoint.rotation * posOffset;

            // compute potential hover rotations and select from them
            rotationDirection = Vector3.zero;
            rotationDirection[selectedDirIndex] = 1;
            float[] potentialAngles = new float[0];
            switch(hoverOrientation)
            {
                case HoverOrientation.OneOrientaion:
                    potentialAngles = new float[] {0f};
                    break;
                case HoverOrientation.TwoOrientations:
                    potentialAngles = new float[] {0f, 180f};
                    break;
                case HoverOrientation.FourOrientations:
                    potentialAngles = new float[] {0f, 90f, 180f, 270f};
                    break;
            }
            Quaternion rot = GetRotationWithMinimumDiffernce(EndEffectorRotation, grabPoint.rotation,
                                                             rotationDirection, potentialAngles);
            hoverTransform.rotation = rot;
            grabTransform.rotation = rot;
        }

        // TYPE 2 //
        // Plane - multiple hover points on a single plane
        else
        {
            // assert proper mode
            selectedDirIndex = selectedDirIndex - 3;
            selectedOriIndex = selectedOriIndex - 3;
            Debug.Assert(selectedOriIndex >= 0 || selectedDirIndex != selectedOriIndex,
                         "Please select proper orientation mode");

            // compute offset and rotation direction
            posOffset = Vector3.zero;
            posOffset[selectedOriIndex] = hoverHeight;
            rotationDirection = Vector3.zero;
            rotationDirection[selectedDirIndex] = 1;

            // compute multiple hover points with orientation and select from them
            int numHoverPoints = 10;
            var (minHoverPos, minHoverRot) = 
                GetPosAndRotWithMinimumDistance(EndEffectorPosition,
                                                grabPoint.position, grabPoint.rotation,
                                                rotationDirection, posOffset, numHoverPoints);
            hoverTransform.position = minHoverPos;
            hoverTransform.rotation = minHoverRot;
            grabTransform.rotation = minHoverRot;
        }

        return (hoverTransform, grabTransform);
    }

    private Quaternion GetRotationWithMinimumDiffernce(Quaternion EndEffectorRotation, 
                                                       Quaternion grabRot, 
                                                       Vector3 dir, float[] angles)
    {
        float minAngleDifference = 1000f;
        Quaternion minHoverRot = Quaternion.identity;
        for (int i = 0; i < angles.Length; ++i)
        {
            Quaternion hoverRot = grabRot * Quaternion.Euler(dir * angles[i]);
            float angleDifference = GetAngleDifference(EndEffectorRotation, hoverRot);
            if (angleDifference < minAngleDifference)
            {
                minHoverRot = hoverRot;
                minAngleDifference = angleDifference;
            }
        }
        return minHoverRot;
    }
    private float GetAngleDifference(Quaternion r1, Quaternion r2)
    {
        // Angle difference
        Quaternion rotationError = r1 * Quaternion.Inverse(r2);
        Vector3 rotationAxis;
        float rotationAngle;
        rotationError.ToAngleAxis(out rotationAngle, out rotationAxis);
        // Wrap and get magnitude
        rotationAngle = Mathf.Abs(Mathf.DeltaAngle(0f, rotationAngle));
        return rotationAngle;
    }

    private (Vector3, Quaternion) GetPosAndRotWithMinimumDistance(Vector3 EndEffectorPosition, 
                                                                  Vector3 grabPos, Quaternion grabRot,
                                                                  Vector3 Rotationdir, Vector3 forwardDir, 
                                                                  int numHoverPoints)
    {
        Vector3 minHoverPos = Vector3.zero;
        Quaternion minHoverRot = Quaternion.identity;
        float minDistance = 100f;
        for (int i = 0; i < numHoverPoints; ++i)
        {
            Quaternion hoverRot = grabRot * Quaternion.Euler(Rotationdir * (i * 360f/numHoverPoints));
            Vector3 hoverPos = grabPos + hoverRot * forwardDir;
            float distance = (hoverPos - EndEffectorPosition).magnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                minHoverPos = hoverPos;
                minHoverRot = hoverRot;
            }
        }
        return (minHoverPos, minHoverRot);
    }


    // Editor Visualization
    public void OnDrawGizmos()
    {
        if (grabPoint == null)
        {
            Debug.LogWarning("No Grab Point is given.");
            return;
        }

        // Draw grab point
        DrawSphere(grabPoint.position, 0.04f);
        DrawAxes(grabPoint.position, grabPoint.rotation);

        // Draw hover point
        // select plane and forward direction
        int selectedDirIndex = (int) hoverDirection;
        int selectedOriIndex = (int) hoverOrientation;
        Vector3 posOffset = Vector3.zero;
        Vector3 rotationDirection = Vector3.zero;

        // TYPE 1 //
        // single hover location with different orientations
        if (selectedDirIndex <= 2)
        {
            // assert proper mode
            if (selectedOriIndex >= 3)
            {
                Debug.LogWarning("Please select proper orientation mode");
                return;
            }

            // compute offset and rotation direction
            posOffset = Vector3.zero;
            posOffset[selectedDirIndex] = hoverHeight;
            Vector3 gizmosHoverPosition = grabPoint.position + grabPoint.rotation * posOffset;
            DrawSphere(gizmosHoverPosition, 0.04f);
            // draw different orientations
            rotationDirection = Vector3.zero;
            rotationDirection[selectedDirIndex] = 1;
            switch(hoverOrientation)
            {
                case HoverOrientation.OneOrientaion:
                    DrawAxes(gizmosHoverPosition, grabPoint.rotation, 0.10f);
                    break;
                case HoverOrientation.TwoOrientations:
                    DrawAxes(gizmosHoverPosition, grabPoint.rotation, 0.15f);
                    DrawAxes(gizmosHoverPosition, 
                             grabPoint.rotation * Quaternion.Euler(rotationDirection * 180), 0.10f);
                    break;
                case HoverOrientation.FourOrientations:
                    DrawAxes(gizmosHoverPosition, grabPoint.rotation, 0.25f);
                    DrawAxes(gizmosHoverPosition, 
                             grabPoint.rotation * Quaternion.Euler(rotationDirection * 90), 0.20f);
                    DrawAxes(gizmosHoverPosition, 
                             grabPoint.rotation * Quaternion.Euler(rotationDirection * 180), 0.15f);
                    DrawAxes(gizmosHoverPosition, 
                             grabPoint.rotation * Quaternion.Euler(rotationDirection * 270), 0.10f);
                    break;
            }
        }

        // TYPE 2 //
        // multiple hover points on a single plane
        else
        {
            // assert proper mode
            selectedDirIndex = selectedDirIndex - 3;
            selectedOriIndex = selectedOriIndex - 3;
            if (selectedOriIndex < 0 || selectedDirIndex == selectedOriIndex)
            {
                Debug.LogWarning("Please select proper orientation mode");
                return;
            }

            // compute offset and rotation direction
            posOffset = Vector3.zero;
            posOffset[selectedOriIndex] = hoverHeight;
            rotationDirection = Vector3.zero;
            rotationDirection[selectedDirIndex] = 1;
            // draw multiple hover points with orientation
            for (int i = 0; i < 8; ++i)
            {
                Quaternion hoverRot = grabPoint.rotation * 
                                      Quaternion.Euler(rotationDirection * (i * 360f/8f));
                Vector3 hoverPos = grabPoint.position + hoverRot * posOffset;
                DrawSphere(hoverPos, 0.04f);
                DrawAxes(hoverPos, hoverRot, 0.10f);
            }
        }
    }
    private void DrawSphere(Vector3 pos, float radius = 0.04f)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pos, radius);
    }
    private void DrawAxes(Vector3 pos, Quaternion rot, float length = 0.1f)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + rot * Vector3.forward * length);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + rot * Vector3.up * length);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + rot * Vector3.right * length);
    }
}
