using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHighlightObject2d : MonoBehaviour
{
    // public List<GameObject> objectsToHighlight = new List<GameObject>();
    public ObjectSelector objectSelector;

    // Start is called before the first frame update
    public HighlightObject2D highlightObject2D;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (objectSelector.graspableObjects != null && !highlightObject2D.objectsToHighlight.Contains(objectSelector.graspableObjects))
            {
                highlightObject2D.AddHighlight(objectSelector.graspableObjects);
            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            highlightObject2D.RemoveHighlight(objectSelector.graspableObjects);
        }
        
    }
}
