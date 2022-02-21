using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Provide util functions to compute forward kinematics
///     for Kinova Gen3 7-DOF robotic arm
/// </summary>
public class ForwardKinematics : MonoBehaviour
{
    // DH parameters
    // params has length of numJoint+1
    // world -> joint 1 -> ... -> end effector
    public int numJoint = 7;
    public float[] alpha = new float[] {3.1415927f, 1.5707963f, 1.5707963f, 1.5707963f,  
                                        1.5707963f, 1.5707963f, 1.5707963f, 3.1415927f};
    public float[] a = new float[] {0, 0, 0, 0, 0, 0, 0, 0};
    public float[] d = new float[] {0, -0.2848f, -0.0118f, -0.4208f, 
                                    -0.0128f, -0.3143f, 0, -0.2874f};
    public float[] initialTheta = new float[] {0, 0, 3.1415927f, 3.1415927f, 3.1415927f, 
                                               3.1415927f, 3.1415927f, 3.1415927f};
    private float[] theta;
    public float[] angleLowerLimits = new float[] {0, -2.41f, 0, -2.66f, 0, -2.23f, 0};
    public float[] angleUpperLimits = new float[] {0,  2.41f, 0,  2.66f, 0,  2.23f, 0};

    // Homography Matrix
    private Matrix4x4[] initH;
    private Matrix4x4[] H;

    // Joint position and rotations
    private Vector3[] positions;
    private Quaternion[] rotations;

    void Start()
    {
        theta = (float[])initialTheta.Clone();

        // Initialize homography matrices
        initH = new Matrix4x4[numJoint+1];
        for (int i = 0; i < numJoint+1; ++i)
        {
            float ca = Mathf.Cos(alpha[i]);
            float sa = Mathf.Sin(alpha[i]);

            initH[i] = Matrix4x4.identity;
            initH[i].SetRow( 0, new Vector4 (1, -ca,  sa, a[i]) );
            initH[i].SetRow( 1, new Vector4 (1,  ca, -sa, a[i]) );
            initH[i].SetRow( 2, new Vector4 (0,  sa,  ca, d[i]) );
            initH[i].SetRow( 3, new Vector4 (0,  0,   0,  1   ) );
        }
        H = (Matrix4x4[])initH.Clone();

        // Initialize joint positions and rotations
        positions = new Vector3[numJoint+1];
        rotations = new Quaternion[numJoint+1];
        UpdateAllH(new float[] {0f, 0f, 0f, 0f, 0f, 0f, 0f});
    }

    void Update()
    {
        // Example //
        /* 
        UpdateAllH(new float[] {0f, 0f, 0f, Mathf.PI/2, 0f, Mathf.PI/2, 0f});
        (Vector3 position, Quaternion rotation) = GetPose(7) // end effector
        Debug.Log(position.ToString("0.000"));
        */
    }

    public void UpdateH(int i, float rotateTheta)
    {
        /* Compute homography transformation matrix
           from joint i-1 to joint i 
        */
        // Update joint angle
        theta[i] = initialTheta[i] + rotateTheta;
        // For computation
        float ct = Mathf.Cos(theta[i]);
        float st = Mathf.Sin(theta[i]);

        // Update H
        H[i] = initH[i];
        H[i][0, 0] *= ct;
        H[i][0, 1] *= st;
        H[i][0, 2] *= st;
        H[i][0, 3] *= ct;
        H[i][1, 0] *= st;
        H[i][1, 1] *= ct;
        H[i][1, 2] *= ct;
        H[i][1, 3] *= st;
        
        // Update joint positions and rotations
        UpdateAllPose();
    }
    public void UpdateAllH(float[] jointAngles)
    {
        /* Compute homography transformation matrices
           from joint i-1 to joint i for all i
        */
        UpdateH(0, 0);
        for (int i = 0; i < numJoint; ++i)
        {
            UpdateH(i+1, jointAngles[i]);
        }

        // Update joint positions and rotations
        UpdateAllPose();
    }

    public void UpdateAllPose()
    {
        // Compute H from base to end effector
        Matrix4x4 HEnd = Matrix4x4.identity;
        for (int i = 0; i < numJoint+1; ++i)
        {            
            HEnd = HEnd * H[i];
            positions[i] = new Vector3(HEnd[0, 3], HEnd[1, 3], HEnd[2, 3]);
            rotations[i] = HEnd.rotation;
        }
    }

    public (Vector3, Quaternion) GetPose(int i, bool toRUF=false)
    {
        // Unity coordinate
        if (toRUF)
            return (ToRUF(positions[i]), ToRUF(rotations[i]));
        else
            return (positions[i], rotations[i]);
    }
    public (Vector3[], Quaternion[]) GetAllPose(bool toRUF=false)
    {
        // Unity coordinate
        if (toRUF)
        {
            Vector3[] positionsRUF = new Vector3[numJoint+1];
            Quaternion[] rotationsRUF = new Quaternion[numJoint+1];
            for (int i = 0; i < numJoint+1; ++i)
            {
                positionsRUF[i] = ToRUF(positions[i]);
                rotationsRUF[i] = ToRUF(rotations[i]);
            }
            return (positionsRUF, rotationsRUF);
        }
        else
            return (positions, rotations);
    }

    // Convert to Unity coordinate
    private Vector3 ToRUF(Vector3 p)
    {
        return new Vector3(-p.y, p.z, p.x);
    }
    private Quaternion ToRUF(Quaternion q)
    {
        return new Quaternion(-q.y, q.z, q.x, -q.w);
    }
}
