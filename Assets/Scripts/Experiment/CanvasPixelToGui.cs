using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/// <summary>
///     Converts a pixel coordinate to a gui element
/// </summary>
public class CanvasPixelToGui : MonoBehaviour
{

    // TODO Consider using GraphicRaycaster.Raycast
    [SerializeField] List<GameObject> guiElements = new List<GameObject>();
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    

    void Update()
    {
        // Quick Test
        // Left Click
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log(GetPixelToGui(Input.mousePosition));
        }
    }

    // Get the name of the gameobject related to the pixel coordinate
    // public string GetPixelToGui(Vector2 pixelPosition)
    // {
    //     // Check each gui element
    //     foreach(GameObject gui in guiElements)
    //     {
    //         Rect rect = gui.GetComponent<RectTransform>().rect;
    //         if(RectContains(gui.GetComponent<RectTransform>(), pixelPosition))
    //         {
    //             return gui.name;
    //         }
    //     }

    //     return "none";
    // }

    // Get the name of the gameobject related to the pixel coordinate using the GraphicRaycaster
    public string GetPixelToGui(Vector2 pixelPosition)
    {
        // Check each gui element

        // Save the pixel position
        PointerEventData pointerEventData = new PointerEventData(null);
        pointerEventData.position = pixelPosition;

        // Results
        List<RaycastResult> results = new List<RaycastResult>();

        graphicRaycaster.Raycast(pointerEventData, results);

        if(results.Count > 0)
        {
            return results[0].gameObject.name;
        }

        return "none";
    }

    private bool RectContains(RectTransform rect, Vector2 pixelPosition)
    {

        // Placeholder for rect corners
        Vector3[] v = new Vector3[4];
        rect.GetWorldCorners(v);

        // Quickly check that the canvas is normalized
        foreach(Vector3 corner in v)
        {
            if(corner.z != 0.0f)
            {
                Debug.LogError("Corner Position.Z is not zero. Canvas is rotated.");
                return false;
            }

        }

        // Breakdown each corner
        float rectXMin = v[0].x;
        float rectXMax = v[2].x;
        float rectYMin = v[0].y;
        float rectYMax = v[2].y;

        Debug.Log("V: " + "" + v[0].ToString() + " " + v[2].ToString());

        return  pixelPosition.x > rectXMin 
                && pixelPosition.x < rectXMax
                && pixelPosition.y > rectYMin
                && pixelPosition.y < rectYMax;
    }

    
}
