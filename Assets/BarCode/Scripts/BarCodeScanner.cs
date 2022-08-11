using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using UnityEngine.UI;

using ZXing;


public class BarCodeScanner : MonoBehaviour
{
    public Camera cam;
    private Texture2D cameraTexture;
    private RenderTexture renderTexture;
    public int textureSize = 1440;

    void Start()
    {
    }

    void Update()
    {
    }


    public string Scan(float cropRatio = 1.0f)
    {
        if (cam == null)
        {
            Debug.Log("Camera not set up.");
            return "N/A";
        }
            
        cameraTexture = GetCameraTexture(cropRatio);
        // Scan the camera texture
        IBarcodeReader barcodeReader = new BarcodeReader();
        Result result = barcodeReader.Decode(cameraTexture.GetPixels32(), 
                                             cameraTexture.width, cameraTexture.height);
        if (result == null)
            return "N/A";
        else
            return result.Text;
    }

    private Texture2D GetCameraTexture(float cropRatio = 1.0f)
    {
        RenderTexture current = RenderTexture.active;
        RenderTexture camCurrent = cam.targetTexture;
        int croppedTextureSize = (int) (cropRatio * textureSize);
        int croppedStartSize = (int) ((1 - cropRatio) * textureSize / 2);

        // Camera texture
        renderTexture = new RenderTexture(textureSize, textureSize, 24);
        RenderTexture.active = renderTexture;
        cam.targetTexture = renderTexture;
        cam.Render();
        // Read pixel from camera texture
        cameraTexture = new Texture2D(croppedTextureSize, croppedTextureSize,
                                      TextureFormat.RGBA32, false, true);
        cameraTexture.ReadPixels(new Rect(croppedStartSize, croppedStartSize, 
                                          croppedTextureSize, croppedTextureSize), 0, 0);

        byte[] bytes = cameraTexture.EncodeToJPG();
        // Write the returned byte array to a file in the project folder
        File.WriteAllBytes(Application.dataPath + "/Data/SavedScreen.jpg", bytes);

        cameraTexture.Apply();

        RenderTexture.active = current;
        cam.targetTexture = camCurrent;
        return cameraTexture;
    }
}
