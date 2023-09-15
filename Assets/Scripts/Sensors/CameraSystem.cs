using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A camera system that can be used to control multiple Cameras
/// </summary>
public class CameraSystem : MonoBehaviour
{
    [field:SerializeField] public Camera[] Cameras { get; private set; }
    [SerializeField] private string[] cameraNames;
    [SerializeField] private int frameRate;
    private CameraFrameRate[] cameraFrameRates;

    void Awake()
    {
        Debug.Assert(Cameras.Length == cameraNames.Length);

        // Attach a camera frame rate to each camera if not already attached
        cameraFrameRates = new CameraFrameRate[cameraNames.Length];
        for(int i = 0; i < Cameras.Length; ++i)
        {
            Camera cam = Cameras[i];

            // get and add camera frame rate
            CameraFrameRate fr = cam.gameObject.GetComponent<CameraFrameRate>();
            if (fr == null)
            {
                fr = cam.gameObject.AddComponent<CameraFrameRate>();
            }
            cameraFrameRates[i] = fr;

            // set up camera
            fr.SetCamera(cam);
            fr.SetFrameRate(frameRate);
        }
    }

    void Start() {}

    void Update() {}

    // Set target frame rate
    public void SetTargetFrameRate(string name, int frameRate)
    {
        SetTargetFrameRate(GetIndex(name), frameRate);
    }

    public void SetTargetFrameRate(int index, int frameRate)
    {
        cameraFrameRates[index].SetFrameRate(frameRate);
    }

    // Set target RenderTexture
    public void SetTargetRenderTexture(string name, RenderTexture renderTexture)
    {
        SetTargetRenderTexture(GetIndex(name), renderTexture);
    }

    public void SetTargetRenderTexture(int index, RenderTexture renderTexture)
    {
        Cameras[index].targetTexture = renderTexture;
    }

    // Enable and Disable
    public void EnableCamera(string name)
    {
        EnableCamera(GetIndex(name));
    }

    public void EnableCamera(int index)
    {
        cameraFrameRates[index].Rendering = true;
    }

    public void DisableCamera(string name)
    {
        DisableCamera(GetIndex(name));
    }

    public void DisableCamera(int index)
    {
        cameraFrameRates[index].Rendering = false;
    }

    public void DisableAllCameras()
    {
        for (int i = 0; i < Cameras.Length; ++i)
        {
            DisableCamera(i);
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
