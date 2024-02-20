using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGazePosition : MonoBehaviour
{
    public EyeTracking eyeTracking;
    public RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 position = eyeTracking.Pixel;
        rect.anchoredPosition = position;
    }
}
