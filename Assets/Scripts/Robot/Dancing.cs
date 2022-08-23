using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Robot dancing - just for fun
/// </summary>
public class Dancing : MonoBehaviour
{
    public ArticulationWheelController wheelController;
    public ArticulationJointController leftArmController;
    public ArticulationJointController rightArmController;
    public ArticulationCameraController cameraController;

    [System.Serializable]
    public struct KeyFrame
    {
        public float time;
        public float linearSpeed;
        public float angularSpeed;
        public float[] leftArmAngles;
        public float[] rightArmAngles;
        public float[] cameraAngles;
    };
    public KeyFrame[] keyFrames;
    private int frameIndex = -1;
    private bool start = false;
    private float startTime;

    void Start()
    {
        // Ballet
        /*
        keyFrames = new KeyFrame[7];
        keyFrames[0] = ToKeyFrame(0f);
        keyFrames[1] = ToKeyFrame(1f, 
                                  0f, 0f, 
                                  new float[] {0f, 0f, 0f, 0f, 0f, 0f, 0f},
                                  new float[] {0f, 1f, 0f, 2f, 0f, 1f, 0f});
        keyFrames[2] = ToKeyFrame(2f, 
                                  0f, -4f, 
                                  new float[] {0f, 0f, 0f, 0f, 0f, 0f, 0f},
                                  new float[] {0f, 0f, 0f, 0f, 0f, 0f, 0f});
        keyFrames[3] = ToKeyFrame(5f, 
                                  0f, -8f, 
                                  new float[] {-0.5f, -1f, 0f, -1.5f, 0f, -0.5f, 0f},
                                  new float[] {0.5f, 1f, 0f, 1.5f, 0f, 0.5f, 0f});
        keyFrames[4] = ToKeyFrame(8f, 
                                  0f, -8f, 
                                  new float[] {-0.5f, -1f, 0f, -1.5f, 0f, -0.5f, 0f},
                                  new float[] {0.5f, 1f, 0f, 1.5f, 0f, 0.5f, 0f});
        keyFrames[5] = ToKeyFrame(9.35f,
                                  0f, 0f, 
                                  new float[] {-0.5f, -1f, 0f, -1.5f, 0f, -0.5f, 0f},
                                  new float[] {0.5f, 1f, 0f, 1.5f, 0f, 0.5f, 0f});
        keyFrames[6] = ToKeyFrame(11f, 
                                  0f, 0f, 
                                  new float[] {0f, 0f, 0f, 0f, 0f, 0f, 0f},
                                  new float[] {-1f, 0.5f, 0f, 0.3f, 0f, 0f, 0f},
                                  new float[] {-1.0f, 0.3f});
        */
        // Fancy Spin
        keyFrames = new KeyFrame[4];
        keyFrames[0] = ToKeyFrame(0f);
        keyFrames[1] = ToKeyFrame(2f, 
                                  0f, -2f, 
                                  new float[] {1.57f, -1.57f, 0f, 0f, 0f, 0f, 0f},
                                  new float[] {-1.57f, 1.57f, 0f, 0f, 0f, 0f, 0f});
        keyFrames[2] = ToKeyFrame(7f, 
                                  0f, -4f, 
                                  new float[] {1.57f, -1.57f, 0f, 0f, 0f, 0f, 0f},
                                  new float[] {-1.57f, 1.57f, 0f, 0f, 0f, 0f, 0f});
        keyFrames[3] = ToKeyFrame(9f);
    }

    void Update()
    {
        // Start
        if (Input.GetMouseButtonDown(1))
        {
            start = true;
            startTime = Time.time;
        }
            
        if (!start)
            return;

        // Next frame
        if (frameIndex + 1 < keyFrames.Length && 
            (Time.time - startTime) > keyFrames[frameIndex + 1].time)
            frameIndex++;
        if (frameIndex + 1 >= keyFrames.Length || frameIndex == -1)
            return;

        // Execute current motion
        // current progress
        float per = ((Time.time - startTime) - keyFrames[frameIndex].time) / 
                    (keyFrames[frameIndex+1].time - keyFrames[frameIndex].time);
        // base
        float linearSpeed = Mathf.Lerp(keyFrames[frameIndex].linearSpeed,
                                       keyFrames[frameIndex+1].linearSpeed, per);
        float angularSpeed = Mathf.Lerp(keyFrames[frameIndex].angularSpeed,
                                        keyFrames[frameIndex+1].angularSpeed, per);
        wheelController.SetRobotVelocity(linearSpeed, angularSpeed);
        // left arm
        float[] leftArmAngles = LerpArray(keyFrames[frameIndex].leftArmAngles, 
                                          keyFrames[frameIndex+1].leftArmAngles, per);
        leftArmController.SetJointTargets(leftArmAngles);
        // right arm
        float[] rightArmAngles = LerpArray(keyFrames[frameIndex].rightArmAngles, 
                                           keyFrames[frameIndex+1].rightArmAngles, per);
        rightArmController.SetJointTargets(rightArmAngles);
        // camera
        float[] cameraAngles = LerpArray(keyFrames[frameIndex].cameraAngles, 
                                         keyFrames[frameIndex+1].cameraAngles, per);
        cameraController.SetCameraJoints(cameraAngles[0], cameraAngles[1]);
    }

    private KeyFrame ToKeyFrame(float time, 
                                float linearSpeed = 0f, float angularSpeed = 0f,
                                float[] leftArmAngles = null, 
                                float[] rightArmAngles = null,
                                float[] cameraAngles = null)
    {
        if (leftArmAngles == null)
            leftArmAngles = new float[7];
        if (rightArmAngles == null)
            rightArmAngles = new float[7];
        if (cameraAngles == null)
            cameraAngles = new float[2];

        KeyFrame keyFrame = new KeyFrame();
        keyFrame.time = time;
        keyFrame.linearSpeed = linearSpeed;
        keyFrame.angularSpeed = angularSpeed;
        keyFrame.leftArmAngles = leftArmAngles;
        keyFrame.rightArmAngles = rightArmAngles;
        keyFrame.cameraAngles = cameraAngles;

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
}
