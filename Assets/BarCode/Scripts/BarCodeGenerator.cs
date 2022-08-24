using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ZXing;
using ZXing.QrCode;


public class BarCodeGenerator : MonoBehaviour
{
    public GameObject displayObject;

    private Texture2D storedEncodedQRTexture;
    private Texture2D storedEncodedPACETexture;
    private int TEXTURE_SIZE = 256;

    void Start()
    {
        // Example
        // GenerateUPCECode(displayObject, "0100200");
    }

    void Update()
    {}

    private Color32[] Encode(string text, ZXing.BarcodeFormat format,
                             int width, int height)
    {
        BarcodeWriter writer = new BarcodeWriter
        {
            Format = format,
            Options = new ZXing.Common.EncodingOptions
            {
                Width = width,
                Height = height
            }
        };
        return writer.Write(text);
    }

    public void GenerateQRCode(GameObject displayObject, string text)
    {
        Color32[] convertPixelToTexture = Encode(text, BarcodeFormat.QR_CODE, 
                                                 TEXTURE_SIZE, TEXTURE_SIZE);
        
        storedEncodedQRTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE);
        storedEncodedQRTexture.SetPixels32(convertPixelToTexture);
        storedEncodedQRTexture.Apply();

        Material material = displayObject.GetComponent<Renderer>().material;
        material.mainTexture = storedEncodedQRTexture;
    }

    public void GenerateUPCECode(GameObject displayObject, string text)
    {
        Color32[] convertPixelToTexture = Encode(text, BarcodeFormat.UPC_E, 
                                                 TEXTURE_SIZE, TEXTURE_SIZE);
        
        storedEncodedPACETexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE);
        storedEncodedPACETexture.SetPixels32(convertPixelToTexture);
        storedEncodedPACETexture.Apply();

        /*
        // Save locally
        byte[] bytes = storedEncodedPACETexture.EncodeToPNG();
        var dirPath = Application.dataPath + "/../Images/";
        if(!Directory.Exists(dirPath)) {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "barcode" + ".png", bytes);
        */
        
        Material material = displayObject.GetComponent<Renderer>().material;
        material.mainTexture = storedEncodedPACETexture;
    }
}
