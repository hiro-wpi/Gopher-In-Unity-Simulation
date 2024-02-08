using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAR : MonoBehaviour
{
    // Scripts
    [SerializeField] private HighlightObjectOnCanvas highlightObject;
    [SerializeField] private GenerateARGameObject arGenerator;
    [SerializeField] private FloorSelector floorSelector;
    [SerializeField] private ObjectSelector objectSelector;

    // Test setup
    [SerializeField] private GraphicalInterface graphicalInterface;
    [SerializeField] private RectTransform displayRect;
    private Camera cam;

    // HighlightObjectOnCanvas
    [SerializeField] private Sprite icon;
    // GenerateARGameObject
    [SerializeField] private GameObject arObjectPrefab;
    // ObjectSelector
    [SerializeField, ReadOnly] private GameObject selectedObject;

    void Start() {}

    //////////////////////////////////////////
    void OnEnable()
    {
        // Subscribe to the event
        floorSelector.OnFloorSelected += OnFloorSelected;
        objectSelector.OnObjectSelected += OnObjectSelected;
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up
        floorSelector.OnFloorSelected -= OnFloorSelected;
        objectSelector.OnObjectSelected -= OnObjectSelected;
    }
    //////////////////////////////////////////

    //////////////////////////////////////////
    private void OnFloorSelected(Vector3 position, Quaternion rotation)
    {
        Debug.Log(
            "Floor selected at: " 
            + position 
            + " with rotation: " 
            + rotation.eulerAngles
        );
    }

    private void OnObjectSelected(GameObject obj, Vector3 position)
    {
        Debug.Log("Object selected: " + obj.name);
        Debug.Log("Hit position: " + position);

        highlightObject.RemoveHighlight(selectedObject);

        selectedObject = obj;

        highlightObject.Highlight(
            selectedObject,
            cam,
            displayRect
        );
    }
    //////////////////////////////////////////

    void Update()
    {
        // Get the main camera if it is not already set
        if (cam == null)
        {
            // Get all the active cameras referanced in the graphical interface
            Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();
            if (cameras.Length > 0)
            {
                cam = cameras[0];

                //////////////////////////////////////////
                floorSelector.SetCameraAndDisplay(cam, displayRect);
                objectSelector.SetCameraAndDisplay(cam, displayRect);
                //////////////////////////////////////////
            }
        }

        //////////////////////////////////////////
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (selectedObject == null)
            {
                return;
            }

            highlightObject.Highlight(
                selectedObject,
                cam,
                displayRect
            );

            var top = HighlightObjectOnCanvas.ElementPosition.Top;
            highlightObject.Highlight(
                selectedObject,
                cam,
                displayRect,
                icon,
                Color.green,
                adjustUIScale: false,
                position: top
            );
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            highlightObject.RemoveHighlight(selectedObject);
        }
        //////////////////////////////////////////
        
        //////////////////////////////////////////
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (selectedObject == null)
            {
                return;
            }

            var type = GenerateARGameObject.ARObjectType.Cube;
            arGenerator.Instantiate(
                selectedObject,
                type,
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(0.1f, 0.1f, 0.1f),
                Color.green,
                0.5f
            );

            arGenerator.Instantiate(
                selectedObject,
                arObjectPrefab,
                new Vector3(0, 1, 0),
                new Vector3(0, 90, 0),
                new Vector3(1f, 1f, 1f),
                Color.red,
                0.25f
            );
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            arGenerator.Destroy(selectedObject, 0);
            arGenerator.Destroy(selectedObject, 0);
        }
        //////////////////////////////////////////
    }
}
