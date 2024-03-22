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
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    [SerializeField] private GraphicalInterface graphicalInterface;
    private Camera cam;
    [SerializeField] RectTransform cameraRect;

    // Quick note, ARObject is both a tag and a layer
    private string stringARTag = "ARObject";
    void Update()
    {

        if (cam == null)
        {
            // Get all the active cameras referanced in the graphical interface
            Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();
            if (cameras.Length > 0)
            {
                cam = cameras[0];

            }
        }

        // Quick Test
        // Left Click
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log(GetPixelToGui(Input.mousePosition));
            Debug.Log(GetARGameObject(Input.mousePosition));
            Debug.Log(Input.mousePosition);
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

    // Get if a gameobject is selected given a 2D position
    private (bool, GameObject) GetARGameObject( Vector2 selectedPosition )
    {
        // Check if hitting the proper RectTransform
        if (
            !RectTransformUtility.RectangleContainsScreenPoint(
                cameraRect, selectedPosition
            )
        )
        {
            return (false, null);
        }

        // Check if we are hitting the ARObject
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(selectedPosition);
        // Find collision and check if it is the floor
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.tag == stringARTag)
            {
                return (true, hit.collider.gameObject);
            }
            
        }
        
        return (false, null);
    }





    
}
