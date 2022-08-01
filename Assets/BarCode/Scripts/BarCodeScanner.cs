using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ZXing;


public class BarCodeScanner : MonoBehaviour
{
    public Camera cam;
    private Texture2D cameraTexture;
    private int TEXTURE_SIZE = 1080;

    void Start()
    {
    }

    void Update()
    {
    }


    public string Scan()
    {
        if (cam == null)
        {
            Debug.Log("Camera not set up.");
            return "";
        }
            
        cameraTexture = GetCameraTexture();
        // Scan the camera texture
        IBarcodeReader barcodeReader = new BarcodeReader();
        Result result = barcodeReader.Decode(cameraTexture.GetPixels32(), 
                                             cameraTexture.width, cameraTexture.height);
        if (result != null)
            return result.Text;
        else
            return "N/A";
    }

    private Texture2D GetCameraTexture()
    {
        RenderTexture current = RenderTexture.active;
        RenderTexture camCurrent = cam.targetTexture;
        // Camera texture

        RenderTexture renderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 24);
        RenderTexture.active = renderTexture;
        cam.targetTexture = renderTexture;
        cam.Render();
        // Read pixel from camera texture
        cameraTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE,
                                      TextureFormat.RGBA32, false, true);
        cameraTexture.ReadPixels(new Rect(0, 0, TEXTURE_SIZE, TEXTURE_SIZE), 0, 0);
        cameraTexture.Apply();

        RenderTexture.active = current;
        cam.targetTexture = camCurrent;
        return cameraTexture;
    }
}
