using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Highlight objects tha are selected/hovered by the mouse
/// </summary>
public class HighlightSelection : MonoBehaviour
{
    public Camera cam;
    public Material hightlightMaterial;
    
    private Transform selectedTransform;
    private Material originalMaterial;

    void Start()
    {
    }

    void Update()
    {
        // Get camera pointing ray
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // If pointing at some game object
        if (Physics.Raycast(ray, out hit))
        {   
            // Get object transform
            Transform selection = hit.transform;
            


            // If selection is changed
            if (selectedTransform != null && selectedTransform != selection)
            {
                Renderer selectedRenderer = selectedTransform.GetComponent<Renderer>();
                selectedRenderer.material = originalMaterial;
            }


            // Get and change object render materials
            Renderer selectionRenderer;
            if (selection.TryGetComponent(out selectionRenderer))
            {
                if (selectedTransform == null || selectedTransform != selection)
                {
                    originalMaterial = selectionRenderer.material;
                    selectionRenderer.material = hightlightMaterial;

                    selectedTransform = selection;
                }
            }
            
        }
    }
}
