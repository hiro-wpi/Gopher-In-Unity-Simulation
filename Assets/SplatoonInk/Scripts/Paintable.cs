using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paintable : MonoBehaviour 
{
    // convert to texture size
    private int TEXTURE_SIZE = 128;
    public float extendsIslandOffset = 1f;

    RenderTexture extendIslandsRenderTexture;
    RenderTexture uvIslandsRenderTexture;
    RenderTexture maskRenderTexture;
    RenderTexture supportTexture;
    Renderer rend;

    int maskTextureID = Shader.PropertyToID("_MaskTexture");

    public RenderTexture getMask() => maskRenderTexture;
    public RenderTexture getUVIslands() => uvIslandsRenderTexture;
    public RenderTexture getExtend() => extendIslandsRenderTexture;
    public RenderTexture getSupport() => supportTexture;
    public Renderer getRenderer() => rend;

    // Check painted area
    public bool coverageCheck = false;
    public float checkPeriod = 0.5f; // check every second
    private float prevTime;
    private float coverage;

    public float GetCoverage() => coverage;


    void Start() 
    {
        // Paintable
        maskRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        maskRenderTexture.filterMode = FilterMode.Bilinear;

        extendIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        extendIslandsRenderTexture.filterMode = FilterMode.Bilinear;

        uvIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        uvIslandsRenderTexture.filterMode = FilterMode.Bilinear;

        supportTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        supportTexture.filterMode = FilterMode.Bilinear;

        rend = GetComponent<Renderer>();
        rend.material.SetTexture(maskTextureID, extendIslandsRenderTexture);

        PaintManager.instance.initTextures(this);

        // Painted area check
        if (coverageCheck)
            InvokeRepeating("CheckCoverage", 0f, checkPeriod);
    }

    void OnDisable()
    {
        maskRenderTexture.Release();
        uvIslandsRenderTexture.Release();
        extendIslandsRenderTexture.Release();
        supportTexture.Release();
    }
    

    private void CheckCoverage()
    {
        RenderTexture current = RenderTexture.active;

        RenderTexture.active = maskRenderTexture;
        Texture2D maskTexture2D = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, 
                                                TextureFormat.RGBA32, false, true);
        maskTexture2D.ReadPixels(new Rect(0, 0, TEXTURE_SIZE, TEXTURE_SIZE), 0, 0);
        maskTexture2D.Apply();
        var data = maskTexture2D.GetPixels();

        // Find area that is not black / is painted
        int blackCount = 0;
        for (int i = 0; i < data.Length; i++)
            if (data[i].grayscale == 0)
                blackCount++;
        coverage = 1 - (float)blackCount / (TEXTURE_SIZE * TEXTURE_SIZE);

        RenderTexture.active = current;
    }
}