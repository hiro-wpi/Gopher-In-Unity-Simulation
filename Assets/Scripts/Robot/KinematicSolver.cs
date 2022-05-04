using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static JacobianTools;

/// <summary>
///     Provide util functions to compute forward kinematics
///     for Kinova Gen3 7-DOF robotic arm
/// </summary>
public class KinematicSolver : MonoBehaviour
{
    // DH parameters
    // params has length of numJoint+1
    // world -> joint 1 -> ... -> end effector
    public int numJoint = 7;
    public float[] alpha = new float[] {3.1415927f, 1.5707963f, 1.5707963f, 1.5707963f,
                                        1.5707963f, 1.5707963f, 1.5707963f, 3.1415927f};
    public float[] a = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
    public float[] d = new float[] {0, -0.2848f, -0.0118f, -0.4208f,
                                    -0.0128f, -0.3143f, 0, -0.2874f};
    public float[] thetaOffset = new float[] {0, 0, 3.1415927f, 3.1415927f, 3.1415927f,
                3.1415927f, 3.1415927f, 3.1415927f};
    public Transform baseTransform;
    private float[] angles = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };

    // Define joint limits
    public float[] angleLowerLimits = new float[] { 0, 0, -2.41f, 0, -2.66f, 0, -2.23f, 0 };
    public float[] angleUpperLimits = new float[] { 0, 0, 2.41f, 0, 2.66f, 0, 2.23f, 0 };

    // Homography Matrix
    private Matrix4x4[] T;

    // Joint position and rotations
    private Vector3[] positions;
    private Quaternion[] rotations;
    private Vector3[] axis;

    void Start()
    {
        // Initialize joint positions and rotations
        positions = new Vector3[numJoint + 1];
        rotations = new Quaternion[numJoint + 1];
        axis = new Vector3[numJoint + 1];

        T = new Matrix4x4[numJoint + 1];
    }

    public Matrix4x4 CalculateT(int i)
    {
        float ca = Mathf.Cos(alpha[i]);
        float sa = Mathf.Sin(alpha[i]);

        float ct = Mathf.Cos(angles[i] + thetaOffset[i]);
        float st = Mathf.Sin(angles[i] + thetaOffset[i]);

        Matrix4x4 T = Matrix4x4.zero;

        T.SetRow(0, new Vector4(ct, -ca * st, sa * st, a[i] * ct));
        T.SetRow(1, new Vector4(st, ca * ct, -sa * ct, a[i] * st));
        T.SetRow(2, new Vector4(0, sa, ca, d[i]));
        T.SetRow(3, new Vector4(0, 0, 0, 1));

        return T;
    }

    public void UpdateAngles(float[] newAngles)
    {
        for (int i = 0; i < numJoint; i++)
        {
            // Limit the joint angles, skipping first joint
            int j = i + 1;
            if (angleLowerLimits[j] != 0 && angleUpperLimits[j] != 0)
            {
                angles[j] = Mathf.Clamp(newAngles[i], angleLowerLimits[j], angleUpperLimits[j]);
            }
            else
            {
                angles[j] = newAngles[i];
            }
        }
    }

    public void CalculateAllT()
    {
        for (int i = 0; i < numJoint + 1; i++)
        {
            T[i] = CalculateT(i);
        }
    }

    public Matrix4x4[] GetMultTs()
    {
        Matrix4x4[] Ts = new Matrix4x4[numJoint + 1];
        // first T is equal to baseTransform
        Vector3 pos = FromRUF(baseTransform.position);
        Quaternion rot = FromRUF(baseTransform.rotation);
        Matrix4x4 TTotal = Matrix4x4.TRS(pos, rot, Vector3.one);
        for (int i = 0; i < numJoint + 1; i++)
        {
            if (i > 0)
            {
                TTotal *= T[i];
            }
            Ts[i] = TTotal;
        }
        return Ts;
    }

    // public Quaternion QuaternionFromMatrix(Matrix4x4 m) { return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); }

    public void UpdateAllPose()
    {
        // Compute H from base to end effector
        Matrix4x4[] Ts = GetMultTs();
        for (int i = 0; i < numJoint + 1; i++)
        {
            Matrix4x4 T = Ts[i];
            axis[i] = ToRUF(T.GetColumn(2));
            positions[i] = ToRUF(T.GetColumn(3));
            try
            {
                rotations[i] = ToRUF(T.rotation);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (positions == null || positions.Length == 0)
        {
            return;
        }
        for (int i = 0; i < numJoint + 1; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(positions[i], positions[i] + axis[i] * 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(positions[i], 0.025f);
            // draw between joints
            if (i > 0)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(positions[i], positions[i - 1]);
            }
        }
    }

    // Need conversion utilities between FLU (forward, left, up) and Unity's RUF (right, up, forward)

    public Vector3 ToRUF(Vector3 v)
    {
        return new Vector3(v.y, -v.z, v.x);
    }

    public Vector3 FromRUF(Vector3 v)
    {
        return new Vector3(v.z, v.x, -v.y);
    }

    public Quaternion ToRUF(Quaternion q)
    {
        return new Quaternion(q.y, -q.z, q.x, q.w);
    }

    public Quaternion FromRUF(Quaternion q)
    {
        return new Quaternion(-q.z, -q.x, q.y, q.w);
    }

    // public Quaternion ToRUF(Quaternion q)
    // {
    //     q = new Quaternion(-q.y, q.z, q.x, -q.w);
    //     return q * Quaternion.Euler(0, 90, 0);
    // }

    // public Quaternion FromRUF(Quaternion q)
    // {
    //     q = q * Quaternion.Euler(0, -90, 0);
    //     return new Quaternion(q.z, -q.x, q.y, -q.w);
    // }

    public (Vector3, Quaternion) GetPose(int i)
    {
        return (positions[i], rotations[i]);
    }

    public (Vector3[], Quaternion[]) GetAllPose()
    {
        return (positions, rotations);
    }

    public ArticulationJacobian ComputeJacobian()
    {
        // Create a new ArticulationJacobian
        // px, py, pz, rx, ry, rz
        ArticulationJacobian jacobian = new ArticulationJacobian(6, numJoint);

        Vector3[] zs = axis;
        Vector3[] ts = positions;

        for (int i = 0; i < numJoint; i++)
        {
            Vector3 zT = Vector3.Cross(zs[i], ts[numJoint] - ts[i]);

            // zB will be the scaled rotation representation of the joint
            // So it's just the axis, with default length 1 (meaning a rotation of 1 radian around that axis)
            Vector3 zB = zs[i];

            JacobianTools.Set(jacobian, i, zT, zB);
        }

        return jacobian;
    }
}
