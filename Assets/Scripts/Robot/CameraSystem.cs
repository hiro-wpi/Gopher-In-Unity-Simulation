using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    public Camera[] cameras;
    public string[] cameraNames;
    public int frameRate;

    void Awake()
    {
        Debug.Assert(cameras.Length == cameraNames.Length);

        foreach (Camera cam in cameras)
        {
            CameraFrameRate fr = cam.gameObject.AddComponent<CameraFrameRate>();
            fr.cam = cam;
            fr.targetFrameRate = frameRate;
        }
    }

    void Update()
    {
    }


    // Set target frame rate
    public void SetTargetFrameRate(string name, int frameRate)
    {
        SetTargetFrameRate(GetIndex(name), frameRate);
    }
    public void SetTargetFrameRate(int index, int frameRate)
    {
        cameras[index].GetComponent<CameraFrameRate>().SetFrameRate(frameRate);
    }

    // Set target RenderTexture
    public void SetTargetRenderTexture(string name, RenderTexture renderTexture)
    {
        SetTargetRenderTexture(GetIndex(name), renderTexture);
    }
    public void SetTargetRenderTexture(int index, RenderTexture renderTexture)
    {
        cameras[index].targetTexture = renderTexture;
    }


    // Enable and Disable
    public void EnableCamera(string name)
    {
        EnableCamera(GetIndex(name));
    }
    public void EnableCamera(int index)
    {
        cameras[index].GetComponent<CameraFrameRate>().rendering = true;
    }
    public void DisableCamera(string name)
    {
        DisableCamera(GetIndex(name));
    }
    public void DisableCamera(int index)
    {
        cameras[index].GetComponent<CameraFrameRate>().rendering = false;
    }
    public void DisableAllCameras()
    {
        foreach (Camera cam in cameras)
        {
            cam.GetComponent<CameraFrameRate>().rendering = false;
        }
    }


    // Utils
    public int GetIndex(string name)
    {
        for (int i = 0; i < cameraNames.Length; ++i)
        {
            if (cameraNames[i].ToUpper() == name.ToUpper())
                return i;
        }
        Debug.Log("No camera named " + name);
        return -1;
    }
    
    public string GetName(int i)
    {
        if (i < 0 || i >= cameraNames.Length)
        {
            return "";
        }
        return cameraNames[i];
    }
}
