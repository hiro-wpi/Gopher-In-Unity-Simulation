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

    [SerializeField, ReadOnly] bool ready = false;
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

        ready = cam != null && cameraRect != null;

        // Quick Test
        // Left Click
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log(GetGUIAR(Input.mousePosition));
        }

    }

    public (string, string, string) GetGUIAR(Vector2 pixelPosition)
    {
        if(!ready)
        {
            return ("None", "None", "None");
        }

        (string, string) gui = GetPixelToGui(pixelPosition);
        (bool, string) ar = Get3DAR(pixelPosition);

        return (gui.Item1, gui.Item2, ar.Item2);
    }

    // Get the name of the gameobject related to the pixel coordinate using the GraphicRaycaster
    private (string, string) GetPixelToGui(Vector2 pixelPosition)
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
            // List<GameObject> guiList= GetPathToCanvas(results[0].gameObject, "Canvas");
            List<GameObject> guiList = GetPathToCanvas(results[0].gameObject, "Canvas");
            string guiString = GetPathString(guiList);    


            if(guiList.Contains(cameraRect.gameObject) && results[0].gameObject != cameraRect.gameObject)
            {
                string mainDisplayToCanvas = GetPathString(GetPathToCanvas(cameraRect.gameObject, "Canvas"));
                return (cameraRect.gameObject.name, results[0].gameObject.name);
            }

            return (results[0].gameObject.name, "None");
        }

        return ("None", "None");
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
    private (bool, string) Get3DAR( Vector2 selectedPosition )
    {
        // Check if hitting the proper RectTransform
        if (
            !RectTransformUtility.RectangleContainsScreenPoint(
                cameraRect, selectedPosition
            )
        )
        {
            return (false, "None");
        }

        // Check if we are hitting the ARObject
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(selectedPosition);
        // Find collision and check if it is the floor
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.tag == stringARTag)
            {
                return (true, hit.collider.gameObject.name);
            }
            
        }
        
        return (false, "None");
    }





    
}
