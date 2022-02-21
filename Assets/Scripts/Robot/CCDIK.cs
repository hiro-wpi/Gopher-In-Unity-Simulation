using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Implementation of Cyclic Coordinate Descent (CCD)
///     to provide inverse kinematics (IK) of robotic arms
///     https://www.tandfonline.com/doi/abs/10.1080/2165347X.2013.823362
/// </summary>
public class CCDIK : MonoBehaviour
{
    // Robot parameters
    public ForwardKinematics fK;
    public int numJoint = 7;
    private Vector3[] jPositions;
    private Quaternion[] jRotations;

    // CCD parameters
    public int maxItreration = 20; // total iteration: n * (4*n)
    public float tolerancePosition = 0.01f; // m <==> 1cm
    public float toleranceRotation = 0.0872665f; // rad <==> 5°

    private float[] jointAngles;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool success;

    void Start()
    {
        // Robot parameters
        jointAngles = new float[numJoint];
    }

    void Update()
    {
        // Example //
        /*
        SetJointAngle(new float[] {0f, 0f, 0f, Mathf.PI/2, 0f, Mathf.PI/2, 0f});
        SetTarget(new Vector3(0.1f, 0.2f, 0.5f),
                  new Quaternion(0f, 0f, 1f, 0f));
        (float[] resultJointAngles, bool foundSolution) = CCD();

        string result = "";
        foreach (var a in resultJointAngles)
            result += a.ToString("0.000") + " ";
        Debug.Log(result);
        */
    }

    public void SetJointAngle(float[] currentJointAngles)
    {
        jointAngles = currentJointAngles;
    }
    public void SetTarget(Vector3 newTargetPosition, 
                          Quaternion newTargetRotation,
                          bool fromRUF=true)
    {
        targetPosition = newTargetPosition;
        targetRotation = newTargetRotation;
        if (fromRUF)
        {
            targetPosition = FromRUF(newTargetPosition);
            targetRotation = FromRUF(newTargetRotation);
        }
    }

    public (float[], bool) CCD(Vector3 newTargetPosition, 
                               Quaternion newTargetRotation,
                               float[] currentJointAngles=null,
                               bool fromRUF=true)
    {
        // Set current joint positions
        if (currentJointAngles != null)
            SetJointAngle(currentJointAngles);
        // Set Target
        SetTarget(newTargetPosition, newTargetRotation, fromRUF);
        // Run CCD
        return CCD();
    }
    public (float[], bool) CCD()
    {
        fK.UpdateAllH(jointAngles);
        success = false;
        // CCD Iteration
        for (int i = 0; i < maxItreration; ++i)
        {
            // Check convergence
            var (endPosition, endRotation) = fK.GetPose(numJoint);
            if (IsPositionConverged(endPosition) && IsRotationConverged(endRotation))
            {
                success = true;
                break;
            }

            // Check position convergence
            (endPosition, endRotation) = fK.GetPose(numJoint);
            if (!IsPositionConverged(endPosition))
            {
                // Minimize position error - backwards
                for (int iJoint = numJoint-1; iJoint >= 0; --iJoint)
                {   
                    (jPositions, jRotations) = fK.GetAllPose();
                    UpdateJointAngle(iJoint, true);
                    fK.UpdateH(iJoint+1, jointAngles[iJoint]);
                }
            }
            
            // Check rotation convergence
            (endPosition, endRotation) = fK.GetPose(numJoint);
            if (!IsRotationConverged(endRotation))
            {
                // Minimize rotation error - backwards
                for (int iJoint = numJoint-1; iJoint >= 0; --iJoint)
                {
                    (jPositions, jRotations) = fK.GetAllPose();
                    UpdateJointAngle(iJoint, false);
                    fK.UpdateH(iJoint+1, jointAngles[iJoint]);  
                }
            }
            
            // Check position convergence again
            (endPosition, endRotation) = fK.GetPose(numJoint);
            if (!IsPositionConverged(endPosition))
            {
                // Minimize position error - forwards
                for (int iJoint = 0; iJoint < numJoint-1; ++iJoint)
                {   
                    (jPositions, jRotations) = fK.GetAllPose();
                    UpdateJointAngle(iJoint, true);
                    fK.UpdateH(iJoint+1, jointAngles[iJoint]);
                }
            }
            
            /*
            // Check rotation convergence
            (endPosition, endRotation) = fK.GetPose(numJoint);
            if (IsRotationConverged(endRotation))
            {
                // Minimize rotation error - forwards
                for (int iJoint = 0; iJoint < numJoint-1; ++iJoint)
                {
                    (jPositions, jRotations) = fK.GetAllPose();
                    UpdateJointAngle(iJoint, false);
                    fK.UpdateH(iJoint+1, jointAngles[iJoint]);  
                }
            }
            */
        }
        return (jointAngles, success);
    }
    private void UpdateJointAngle(int iJoint, bool forPosition=true)
    {
        // Update position 
        Matrix4x4 H = Matrix4x4.TRS(Vector3.zero, jRotations[iJoint], Vector3.one);          
        Vector3 jointZDirection = new Vector3(H[0, 2], H[1, 2], H[2, 2]);

        // Get rotation angle of next step
        float theta;
        if (forPosition)
        {
            Vector3 endPosition = jPositions[numJoint];
            Vector3 jointPosition = jPositions[iJoint];

            // Unit vector from current joint to end effector
            Vector3 endVector = (endPosition - jointPosition).normalized; 
            // Unit vector from current joint to target
            Vector3 targetVector = (targetPosition - jointPosition).normalized; 

            // Rotate current joint to match end effector vector to target vector
            float vectorAngle = Mathf.Clamp( Vector3.Dot(endVector, targetVector), -1, 1 );
            theta = Mathf.Abs( Mathf.Acos(vectorAngle) );
            Vector3 direction = Vector3.Cross(endVector, targetVector);

            // Map the desired angle to the angle of z aixs (for position)
            // ? Project normal axis to joint z axis ?
            theta = theta * Vector3.Dot(direction.normalized, jointZDirection.normalized); 
        }
        else
        {
            Quaternion endRotation = jRotations[numJoint];
            // Rotate current joint to match end effector rotation to target rotation
            float errRotation = error(targetRotation, endRotation);

            if (Mathf.Abs(Mathf.PI - errRotation) < 0.02f)
                theta = 0.2f;
            else if (Mathf.Abs(errRotation) < 0.02f)
                theta = 0;
            else
            {
                // Rotate current joint to match end effector rotation to target rotation
                Quaternion q = targetRotation * Quaternion.Inverse(endRotation);
                Vector3 direction = new Vector3(q.x, q.y, q.z) * q.w * 2 / Mathf.Sin(errRotation);

                // Map the desired angle to the angle of z aixs (for rotation)
                // ? Project normal axis to joint z axis ?
                theta = errRotation * Vector3.Dot(direction.normalized, jointZDirection.normalized);
            }
        }
        jointAngles[iJoint] += theta;
        jointAngles[iJoint] = JointLimit(iJoint, jointAngles[iJoint]);
    }

    private float error(Vector3 p1, Vector3 p2)
    {
        return (p1 - p2).magnitude;
    }
    private float error(Quaternion q1, Quaternion q2)
    {
        Quaternion q = q1 * Quaternion.Inverse(q2);
        float theta = Mathf.Clamp( Mathf.Abs(q.w), -1, 1 ); // avoid overflow
        float errRotation = 2 * Mathf.Acos(theta);
        return errRotation;
    }
    private bool IsPositionConverged(Vector3 endPosition)
    {
        float errPosition = error(targetPosition, endPosition);
        return errPosition < tolerancePosition;
    }
    private bool IsRotationConverged(Quaternion endRotation)
    {
        float errRotation = error(targetRotation, endRotation);
        return errRotation < toleranceRotation;
    }

    private float JointLimit(int iJoint, float angle)
    {
        // Apply joint limits
        float minAngle = fK.angleLowerLimits[iJoint];
        float maxAngle = fK.angleUpperLimits[iJoint];
        // If given joint limit
        if (minAngle != maxAngle)
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
        // If no joint limit
        else
            angle = WrapToPi(angle);
        
        return angle;
    }
    private float WrapToPi(float angle)
    {
        // Wrap angle to [-pi, pi]
        float pi = Mathf.PI;
        angle = angle - 2*pi * Mathf.Floor( (angle+pi) / (2*pi) ); 
        return angle;
    }

    // Convert from Unity coordinate
    private Vector3 FromRUF(Vector3 p)
    {
        return new Vector3(p.z, -p.x, p.y);
    }
    private Quaternion FromRUF(Quaternion q)
    {
        return new Quaternion(q.z, -q.x, q.y, -q.w);
    }
}
