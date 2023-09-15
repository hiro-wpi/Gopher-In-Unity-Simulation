using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Robot dancing - just for fun
/// </summary>
public class Dancing : MonoBehaviour
{
    public ArticulationBaseController baseController;
    public ArticulationArmController leftArmController;
    public ArticulationArmController rightArmController;
    public ArticulationChestController chestController;
    public ArticulationCameraController cameraController;

    [System.Serializable]
    public struct KeyFrame
    {
        public float time;
        public float linearSpeed;
        public float angularSpeed;
        public Vector3 leftArmLinearSpeed;
        public Vector3 leftArmAngularSpeed;
        public Vector3 rightArmLinearSpeed;
        public Vector3 rightArmAngularSpeed;
        public float chestSpeed;
        public float[] cameraAngles;
    };
    public KeyFrame[] keyFrames;
    private int frameIndex = -1;
    private bool start = false;
    private float startTime;

    void Start()
    {
        // Pick demo
        keyFrames = new KeyFrame[17];
        keyFrames[0] = ToKeyFrame(0f);
        keyFrames[1] = ToKeyFrame(1.8f, linearSpeed:0.75f);
        keyFrames[2] = ToKeyFrame(3.5f, linearSpeed:0.75f);
        keyFrames[3] = ToKeyFrame(3.6f);

        keyFrames[4] = ToKeyFrame(4f, chestSpeed:1f);
        keyFrames[5] = ToKeyFrame(7f, chestSpeed:1f);
        keyFrames[6] = ToKeyFrame(7.2f);

        keyFrames[7] = ToKeyFrame(7.8f, rightArmLinearSpeed:new Vector3(-0.2f, 0, 0));
        keyFrames[8] = ToKeyFrame(8.1f);

        keyFrames[9] = ToKeyFrame(9.2f, rightArmLinearSpeed:new Vector3(0, 1.0f, 0f));
        keyFrames[10] = ToKeyFrame(11f, rightArmLinearSpeed:new Vector3(0, 1.0f, 0f));
        keyFrames[11] = ToKeyFrame(11.1f);

        keyFrames[12] = ToKeyFrame(11.5f, rightArmAngularSpeed:new Vector3(0, 0.8f, 0f));
        keyFrames[13] = ToKeyFrame(15f, rightArmAngularSpeed:new Vector3(0, 0.8f, 0f));
        keyFrames[14] = ToKeyFrame(15.1f);

        keyFrames[15] = ToKeyFrame(17f, rightArmLinearSpeed:new Vector3(0, 0f, 0.8f));
        keyFrames[16] = ToKeyFrame(18f);
    }

    void Update()
    {
        // Start
        if (Input.GetKeyDown(KeyCode.Space))
        {
            start = true;
            startTime = Time.time;
        }
            
        if (!start)
        {
            return;
        }

        // Next frame
        if (frameIndex + 1 < keyFrames.Length && 
            (Time.time - startTime) > keyFrames[frameIndex + 1].time
        )
        {
            frameIndex++;
        }
        if (frameIndex + 1 >= keyFrames.Length || frameIndex == -1)
        {
            return;
        }

        // Execute current motion
        // current progress
        float per = ((Time.time - startTime) - keyFrames[frameIndex].time) / 
                    (keyFrames[frameIndex+1].time - keyFrames[frameIndex].time);
        
        // base
        float linearSpeed = Mathf.Lerp(
            keyFrames[frameIndex].linearSpeed, keyFrames[frameIndex+1].linearSpeed, per
        );
        float angularSpeed = Mathf.Lerp(
            keyFrames[frameIndex].angularSpeed, keyFrames[frameIndex+1].angularSpeed, per
        );
        baseController.SetVelocity(
            new Vector3(0, 0, linearSpeed), 
            new Vector3(0, angularSpeed, 0)
        );

        // left arm
        Vector3 leftArmLinearSpeed = LerpVector3(
            keyFrames[frameIndex].leftArmLinearSpeed, 
            keyFrames[frameIndex+1].leftArmLinearSpeed, 
            per
        );
        Vector3 leftArmAngularSpeed = LerpVector3(
            keyFrames[frameIndex].leftArmAngularSpeed, 
            keyFrames[frameIndex+1].leftArmAngularSpeed, 
            per
        );
        leftArmController.SetLinearVelocity(leftArmLinearSpeed);
        leftArmController.SetAngularVelocity(leftArmAngularSpeed);
        
        // right arm
        Vector3 rightArmLinearSpeed = LerpVector3(
            keyFrames[frameIndex].rightArmLinearSpeed, 
            keyFrames[frameIndex+1].rightArmLinearSpeed, 
            per
        );
        Vector3 rightArmAngularSpeed = LerpVector3(
            keyFrames[frameIndex].rightArmAngularSpeed, 
            keyFrames[frameIndex+1].rightArmAngularSpeed, 
            per
        );
        rightArmController.SetLinearVelocity(rightArmLinearSpeed);
        rightArmController.SetAngularVelocity(rightArmAngularSpeed);

        // chest
        float chestSpeed = Mathf.Lerp(
            keyFrames[frameIndex].chestSpeed, keyFrames[frameIndex+1].chestSpeed, per
        );
        chestController.SetSpeedFraction(chestSpeed);

        // camera
        float[] cameraAngles = LerpArray(
            keyFrames[frameIndex].cameraAngles, keyFrames[frameIndex+1].cameraAngles, per
        );
        cameraController.SetPosition(new Vector3(0f, cameraAngles[0], cameraAngles[1]));
    }

    private KeyFrame ToKeyFrame(
        float time, 
        float linearSpeed = 0f, 
        float angularSpeed = 0f,
        Vector3 leftArmLinearSpeed = default,
        Vector3 leftArmAngularSpee = default,
        Vector3 rightArmLinearSpeed = default,
        Vector3 rightArmAngularSpeed = default,
        float chestSpeed = 0f,
        float[] cameraAngles = null)
    {
        cameraAngles ??= new float[2];

        KeyFrame keyFrame = new()
        {
            time = time,
            linearSpeed = linearSpeed,
            angularSpeed = angularSpeed,
            leftArmLinearSpeed = leftArmLinearSpeed,
            leftArmAngularSpeed = leftArmAngularSpee,
            rightArmLinearSpeed = rightArmLinearSpeed,
            rightArmAngularSpeed = rightArmAngularSpeed,
            chestSpeed = chestSpeed,
            cameraAngles = cameraAngles
        };

        return keyFrame;
    }

    // Utils
    private float[] LerpArray(float[] a1, float[] a2, float percentage)
    {
        float[] res = new float[a1.Length];
        for (int i = 0; i < a1.Length; ++i)
        {
            res[i] = Mathf.Lerp(a1[i], a2[i], percentage);
        }
        return res;
    }

    private Vector3 LerpVector3(Vector3 v1, Vector3 v2, float percentage)
    {
        return new Vector3(
            Mathf.Lerp(v1.x, v2.x, percentage),
            Mathf.Lerp(v1.y, v2.y, percentage),
            Mathf.Lerp(v1.z, v2.z, percentage)
        );
    }
}
