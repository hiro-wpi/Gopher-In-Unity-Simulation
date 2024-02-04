using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectSelector : MonoBehaviour, 
    IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    // Camera and Display
    [SerializeField] private Camera cam;
    [SerializeField] private RectTransform cameraRect;
    private bool ready;

    // AR feature
    [SerializeField] private Sprite selectionRectangleTexture;
    private GameObject rectangle;
    private RectTransform rectangleTransform;

    // Click and Drag Flags
    [SerializeField] private string[] selectableObjectTags;
    private Vector2 dragStartPosition;
    private bool dragged = false;

    // Result
    private GameObject selectedObject;
    private Vector3 selectPosition;

    // Event to send results
    public delegate void ObjectSelected(
        GameObject gameObject, Vector3 position
    );
    public event ObjectSelected OnObjectSelected;

    void Start()
    {
        InitializeSelectionRectangle();
    }

    void Update()
    {
        ready = cam != null && cameraRect != null;
    }

    void InitializeSelectionRectangle()
    {
        // Create a RawImage component for selection rectangle
        rectangle = new GameObject("SelectionRectangle");
        rectangle.transform.SetParent(transform);

        // Set a transparent color
        rectangleTransform = rectangle.AddComponent<RectTransform>();
        Image rectangleImage = rectangle.AddComponent<Image>();
        Color color = Color.cyan;
        color.a = 0.1f;
        rectangleImage.color = color;
        rectangleImage.sprite = selectionRectangleTexture;

        rectangle.SetActive(false);
    }

    public void SetCameraAndDisplay(Camera cam, RectTransform cameraRect)
    {
        this.cam = cam;
        this.cameraRect = cameraRect;
    }

    public void SetDesiredTags(string[] tags)
    {
        selectableObjectTags = tags;
    }

    // Event Handlers
    public void OnPointerDown(PointerEventData eventData)
    {
        if(!ready)
        {
            return;
        }
        dragged = false;

        dragStartPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!ready)
        {
            return;
        }
        dragged = true;
        rectangle.SetActive(true);

        // Update the current rect transform position and size
        Vector2 size = eventData.position - dragStartPosition;
        rectangleTransform.position = dragStartPosition + size / 2f;
        rectangleTransform.sizeDelta = new Vector2(
            Mathf.Abs(size.x), Mathf.Abs(size.y)
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(!ready)
        {
            return;
        }
        rectangle.SetActive(false);

        // If not dragged, rotation is not specified
        Vector2[] selectedPositions;
        if(!dragged)
        {
            // only track the center
            selectedPositions = new Vector2[] { dragStartPosition };
        }
        else
        {
            // instead of checking every point, check the center and 4 sides
            Vector2 rectCenter = rectangleTransform.position;
            Vector2 rectSize = rectangleTransform.sizeDelta;
            selectedPositions = new Vector2[] {
                rectangleTransform.position,
                new Vector2(rectCenter.x - rectSize.x * 0.25f, rectCenter.y),
                new Vector2(rectCenter.x + rectSize.x * 0.25f, rectCenter.y),
                new Vector2(rectCenter.x, rectCenter.y - rectSize.y * 0.25f),
                new Vector2(rectCenter.x, rectCenter.y + rectSize.y * 0.25f)
            };
        }

        // If selectable object is selected, send the event
        foreach (var selectedPosition in selectedPositions)
        {
            var (hit, go, position) = GetGameObject(selectedPosition);
            if (hit)
            {
                OnObjectSelected?.Invoke(go, position);
                selectedObject = go;
                selectPosition = position;
                return;
            }
        }
    }

    // Get if a gameobject is selected given a 2D position
    private (bool, GameObject, Vector3) GetGameObject(
        Vector2 selectedPosition,
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
            return (false, null, Vector3.zero);
        }

        // Check if hitting the floor
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(selectedPosition);
        // Find collision and check if it is the floor
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (
                Array.IndexOf(
                    selectableObjectTags, hit.collider.gameObject.tag
                ) >= 0
            )
            {
                return (true, hit.collider.gameObject, hit.point);
            }
            else
            {
                return (false, null, Vector3.zero);
            }
        }
        else
        {
            return (false, null, Vector3.zero);
        }
    }
}