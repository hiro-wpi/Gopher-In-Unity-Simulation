using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

/// <summary>
///     This is a script to select a point on the floor
///     and visualize the selected point with an arrow.
///     
///     The resulting postion and rotation will be sent
///     to the subscribers of the event OnFloorSelected.
/// </summary>
public class FloorSelector : MonoBehaviour, 
    IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    // Camera and Display
    [SerializeField] private Camera cam;
    [SerializeField] private RectTransform cameraRect;
    private bool ready;

    // AR feature
    [SerializeField] private LayerMask floorLayerMask;
    [SerializeField] private GameObject arrowPrefab;
    private GameObject arrow;

    // Click and Drag Flags
    private Vector3 dragStartPosition;
    private Vector3 dragEndPosition;
    private Vector3 arrowOffset = new Vector3(0, 0.1f, 0);
    private bool validFloorHit = false;
    private bool dragged = false;

    // Event to send results
    public delegate void FloorSelected(Vector3 position, Quaternion rotation);
    public event FloorSelected OnFloorSelected;

    void Start()
    {
        InitializeArrow();
    }

    void Update()
    {
        ready = cam != null && cameraRect != null;
    }

    private void InitializeArrow()
    {
        arrow = Instantiate(arrowPrefab);
        arrow.transform.SetParent(transform);
        arrow.transform.localPosition = Vector3.zero;
        arrow.transform.localRotation = Quaternion.identity;

        arrow.SetActive(false);
    }

    public void SetCameraAndDisplay(Camera cam, RectTransform cameraRect)
    {
        this.cam = cam;
        this.cameraRect = cameraRect;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!ready)
        {
            return;
        }
        dragged = false;

        // Check if the mouse is clicking the floor
        (validFloorHit, dragStartPosition) = GetPointOnFloor(
            eventData.position, true
        );
        if (validFloorHit)
        {
            arrow.transform.position = dragStartPosition + arrowOffset;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!ready || !validFloorHit)
        {
            return;
        }
        dragged = true;
        arrow.SetActive(true);

        // Get the current mouse position (allowed to be occuluded)
        var (hit, currPosition) = GetPointOnFloor(
            eventData.position, true
        );
        if (hit)
        {
            dragEndPosition = currPosition;
            arrow.transform.rotation = Quaternion.LookRotation(
                dragEndPosition - dragStartPosition
            );
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(!ready)
        {
            return;
        }
        arrow.SetActive(false);

        // If not dragged, rotation is not specified
        if(!dragged)
        {
            if (validFloorHit)
            {
                OnFloorSelected?.Invoke(dragStartPosition, new Quaternion());
            }
        }
        else
        {
            Quaternion rotation = Quaternion.LookRotation(
                dragEndPosition - dragStartPosition
            );
            OnFloorSelected?.Invoke(dragStartPosition, rotation);
        }
    }

    // Get the point on the floor, could ignore any other objects
    private (bool, Vector3) GetPointOnFloor(
        Vector2 selectedPosition,
        bool ignoreOtherObjects = false,
        float maxDistance = 5
    )
    {
        // Check if hitting the proper RectTransform
        if (
            !RectTransformUtility.RectangleContainsScreenPoint(
                cameraRect, selectedPosition
            )
        )
        {
            return (false, Vector3.zero);
        }

        // Check if hitting the floor
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(selectedPosition);
        // Find collision and check if it is the floor
        if (
            !ignoreOtherObjects 
            && Physics.Raycast(ray, out hit, maxDistance)
        )
        {
            if ((floorLayerMask & (1 << hit.collider.gameObject.layer)) > 0)
            {
                return (true, hit.point);
            }
            else
            {
                return (false, Vector3.zero);
            }
        }

        // Only find collision with the floor
        else if (
            ignoreOtherObjects 
            && Physics.Raycast(
                ray, out hit, maxDistance, floorLayerMask
            )
        )
        {
            return (true, hit.point);
        }

        // Floor not found
        else
        {
            return (false, Vector3.zero);
        }
    }
}
