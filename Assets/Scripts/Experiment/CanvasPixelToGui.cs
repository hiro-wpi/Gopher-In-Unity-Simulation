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
            // iterate throught the results to get a string connecting the parent gameobject "Canvas" to the hit object
            return GetPathString(GetPathToCanvas(results[0].gameObject, "Canvas"));
            // return results[0].gameObject.name;
        }

        return "none";
    }

    private List<GameObject> GetPathToCanvas(GameObject obj, string parentName)
    {
        List<GameObject> path = new List<GameObject>();
        GameObject current = obj;

        while(current.name != parentName)
        {
            path.Add(current);
            current = current.transform.parent.gameObject;
        }

        return path;
    }

    private string GetPathString(List<GameObject> path)
    {
        string pathString = "";
        foreach(GameObject obj in path)
        {
            pathString = obj.name + "/" + pathString;
        }

        return pathString;
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
