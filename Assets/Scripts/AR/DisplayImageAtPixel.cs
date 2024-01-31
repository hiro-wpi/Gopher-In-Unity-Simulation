using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayImageAtPixel : MonoBehaviour
{
    public Canvas canvas;  // Reference to the canvas
    // Pixel coordinates where the image should be displayed
    // public Sprite imageSprite;  // Sprite to be displayed

    public GameObject imageGameObject;  // Reference to the GameObject with the Image component

    void Start()
    {
        DisplayImage(new Vector2(200, 100));
    }

    void DisplayImage(Vector2 pixelCoordinates)
    {
        if (canvas != null)
        {
            // Set the position based on pixel coordinates
            RectTransform rectTransform = imageGameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = pixelCoordinates;

            // Optionally, set other properties like size, rotation, etc., as needed
            // rectTransform.sizeDelta = new Vector2(width, height);

            Debug.Log($"Image displayed at pixel coordinates: {pixelCoordinates}");
        }
        else
        {
            Debug.LogError("Canvas or image sprite reference is missing!");
        }
    }
}
