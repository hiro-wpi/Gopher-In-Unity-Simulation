using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicalInterface : MonoBehaviour
{
    public GameObject recordIcon;

    private int FPS;
    private float FPSSum;
    private int FPSCount;

    // UI
    private GameObject[] UIs;
    private int UIIndex;
    
    // camera
    public GameObject cameraDisplay;
    private RectTransform cameraDisplayRect;
    private RenderTexture cameraRendertexture;

    private Vector2 resolution;


    void Start() 
    {
        // FPS
        FPSCount = 0;
        FPSSum = 0;
        InvokeRepeating("UpdateFPS", 1.0f, 0.5f);

        resolution = new Vector2 (1600, 900);

        // Temp
        cameraDisplay = GameObject.Find("CameraDisplay");
        cameraDisplayRect = cameraDisplay.GetComponent<RectTransform>();
        cameraDisplayRect.sizeDelta = resolution;

        cameraRendertexture = new RenderTexture((int)resolution.x, (int)resolution.y, 24);
        cameraDisplay.GetComponent<RawImage>().texture = cameraRendertexture;
    }

    void Update()
    {
        // FPS
        FPSCount += 1;
        FPSSum += 1.0f/Time.deltaTime;

        // Hotkeys
        if (Input.GetKeyDown(KeyCode.H)) 
            if (UIIndex != 0 && UIIndex != 1 && UIIndex != 2)
                return;
                //ChangeHelpDisplay();
    }

    public void SetCamera(Camera camera)
    {
        camera.targetTexture = cameraRendertexture;
    }

    public void SetRecordIconActive(bool active)
    {
        // TODO 
        // recordIcon.SetActive(active);
    }

    private void UpdateFPS()
    {
        FPS = (int)(FPSSum / FPSCount);
        FPSCount = 0;
        FPSSum = 0;
    }
}
