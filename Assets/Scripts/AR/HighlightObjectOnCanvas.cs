using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Highlight an object on the Canvas.
///     By default, the object will be highlighted witha a bounding box.
///     It could also be replaced with other 2D UI elements.
///     
///     Highlight the object with the following functions:
///     Highlight()
///     RemoveHighlight()
/// </summary>
public class HighlightObjectOnCanvas : MonoBehaviour
{
    // Default 2D UI elements
    [SerializeField] private Sprite boundingBoxTexture;
    [SerializeField] private Color defaultColor = Color.red;
    // Store the objects to highlight
    [ReadOnly]
    public Dictionary<GameObject, List<HighlightElement>> Highlights = new ();

    public enum ElementPosition
    {
        Center,
        Left,
        Right,
        Top,
        Bottom
    }

    void Start() {}

    void Update()
    {
        foreach(var entry in Highlights)
        {
            foreach (var highlight in entry.Value)
            {
                highlight.Update();
            }
        }
    }

    public void Highlight(
        GameObject go,
        Camera cam,
        RectTransform displayRect,
        Sprite uiTexture = null,
        Color? color = null,
        bool adjustUIScale = true,
        ElementPosition position = ElementPosition.Center
    )
    {
        // Default value and color
        if (uiTexture == null)
        {
            uiTexture = boundingBoxTexture;
        }
        Color col = color ?? defaultColor;

        // Create a new highlight element
        var highlight = new HighlightElement(
            go, cam, displayRect, uiTexture, col, adjustUIScale, position
        );

        // Update the existing highlight
        if (Highlights.ContainsKey(go))
        {
            Highlights[go].Add(highlight);
        }
        // Add new highlight
        else
        {
            Highlights.Add(go, new List<HighlightElement> { highlight });
        }
    }

    public List<GameObject> GetHighlightGameObject(GameObject go)
    {
        if (Highlights.ContainsKey(go))
        {
            List<GameObject> highlightGameObjects = new List<GameObject>();
            foreach (var highlight in Highlights[go])
            {
                highlightGameObjects.Add(highlight.GetHighlightObject());
            }
            return highlightGameObjects;
        }
        else
        {
            return new List<GameObject>();
        }
    }

    public void RemoveHighlight(GameObject go, int index = -1)
    {
        // if the object is in the list, remove it
        if (Highlights.ContainsKey(go))
        {
            // Destroy everything
            if (index < 0)
            {
                foreach (var highlight in Highlights[go])
                {
                    highlight.DestroyHighlightElement();
                }
                Highlights.Remove(go);
            }

            // Destroy the specific index of the game object
            // if exists
            else
            {
                if (index >= Highlights[go].Count)
                {
                    return;
                }
                Highlights[go][index].DestroyHighlightElement();
                Highlights[go].RemoveAt(index);

                // if this is the last game object
                if (Highlights[go].Count == 0)
                {
                    Highlights.Remove(go);
                }
            }
        }
    }

    public void UpdateDisplay(
        GameObject go,
        Camera cam,
        RectTransform displayRect
    )
    {
        if (Highlights.ContainsKey(go))
        {
            foreach (var highlight in Highlights[go])
            {
                highlight.SetDisplay(cam, displayRect);
            }
        }
    }

    /// <summary>
    ///     Highlight element class
    ///     Highlight given object with an UI element and position
    /// </summary>
    public class HighlightElement
    {
        // Target object to highlight
        private GameObject targetObject;
        // Gameobject that stores highlight element
        private GameObject highlightObject;
        private RectTransform highlightRect;
        private Image highlightImage;

        // Camera and RectTransform to display
        private Camera cam;
        private RectTransform displayRect;
        // UI texture and color used to highlight
        private Sprite uiTexture;
        private Color color;
        private bool adjustUIScale = true;
        private float maxScaleRatio = 2.5f;
        private ElementPosition position = ElementPosition.Center;

        // Visibility check
        private float inViewBuffer = 0.075f;
        private float inViewDistance = 3;
        private bool isVisible = false;
        private float checkInterval = 0.5f;
        private float lastCheckTime = 0;

        public HighlightElement(
            GameObject go,
            Camera cam,
            RectTransform displayRect,
            Sprite uiTexture,
            Color color,
            bool adjustUIScale,
            ElementPosition position
        )
        {
            targetObject = go;
            this.cam = cam;
            this.displayRect = displayRect;
            this.uiTexture = uiTexture;
            this.color = color;
            this.adjustUIScale = adjustUIScale;
            this.position = position;

            InstantiateOnCanvas();
        }

        void InstantiateOnCanvas()
        {
            // Create a UI object
            highlightObject = new GameObject("HighlightObject");
            highlightObject.transform.parent = displayRect.transform;
            highlightObject.tag = "ARObject";

            highlightRect = highlightObject.AddComponent<RectTransform>();
            highlightImage = highlightObject.AddComponent<Image>();

            // Set the texture and color
            highlightImage.sprite = uiTexture;
            highlightImage.color = color;

            Update();
        }

        public void Update()
        {
            // Visibility Check
            if (Time.time - lastCheckTime > checkInterval)
            {
                CheckIfObjectInView();
                lastCheckTime = Time.time;
            }
            if (!isVisible)
            {
                return;
            }

            // Get the bounds of the target object in world space
            var (minBound, maxBound) = GetWorldBounds();
            // Convert the bounds to screen coordinates
            Vector3 minScreenPoint = cam.WorldToViewportPoint(minBound);
            Vector3 maxScreenPoint = cam.WorldToViewportPoint(maxBound);

            // Calculate size of the bounding box in canvas space
            minScreenPoint.x = minScreenPoint.x * displayRect.rect.width;
            minScreenPoint.y = minScreenPoint.y * displayRect.rect.height;
            maxScreenPoint.x = maxScreenPoint.x * displayRect.rect.width;
            maxScreenPoint.y = maxScreenPoint.y * displayRect.rect.height;
            Vector2 centerPosition = new Vector2(
                (minScreenPoint.x + maxScreenPoint.x) / 2,
                (minScreenPoint.y + maxScreenPoint.y) / 2
            );
            // Move to given position
            if (position == ElementPosition.Left)
            {
                centerPosition.x -= 2 * (maxScreenPoint.x - minScreenPoint.x);
            }
            else if (position == ElementPosition.Right)
            {
                centerPosition.x += 2 * (maxScreenPoint.x - minScreenPoint.x);
            }
            else if (position == ElementPosition.Top)
            {
                centerPosition.y += 2 * (maxScreenPoint.y - minScreenPoint.y);
            }
            else if (position == ElementPosition.Bottom)
            {
                centerPosition.y -= 2 * (maxScreenPoint.y - minScreenPoint.y);
            }

            // Adjust position
            highlightRect.anchoredPosition = new Vector2(
                centerPosition.x - displayRect.rect.width / 2,
                centerPosition.y - displayRect.rect.height / 2
            );

            // Adjust the size of the bounding box
            // only if adjustUIScale is true
            if (!adjustUIScale)
            {
                return;
            }

            Vector2 sizeInViewport = new Vector2(
                Mathf.Abs(maxScreenPoint.x - minScreenPoint.x), 
                Mathf.Abs(maxScreenPoint.y - minScreenPoint.y)
            ) * 2;

            // Prevent from extreme scaling ratio
            if ((sizeInViewport.x / sizeInViewport.y) > maxScaleRatio)
            {
                sizeInViewport.x = sizeInViewport.y * maxScaleRatio;
            }
            if ((sizeInViewport.y / sizeInViewport.x) > maxScaleRatio)
            {
                sizeInViewport.y = sizeInViewport.x * maxScaleRatio;
            }

            highlightRect.sizeDelta = sizeInViewport;
        }

        private void CheckIfObjectInView()
        {
            // Get the viewport position of the object
            Vector3 viewportPosition = cam.WorldToViewportPoint(
                targetObject.transform.position
            );

            // Check if the object is within the camera's view
            bool objectInView = (
                viewportPosition.x >= inViewBuffer
                && viewportPosition.x <= 1 - inViewBuffer
                && viewportPosition.y >= inViewBuffer
                && viewportPosition.y <= 1 - inViewBuffer
                && viewportPosition.z > 0
                && viewportPosition.z < inViewDistance
            );

            highlightObject.SetActive(objectInView);
            isVisible = objectInView;
        }

        private (Vector3, Vector3) GetWorldBounds()
        {
            // Get all rendereres
            var renderers = targetObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return (Vector3.zero, Vector3.zero);
            }
            else if (renderers.Length == 1)
            {
                if (renderers[0].gameObject.tag == "ARObject")
                {
                    return (Vector3.zero, Vector3.zero);
                }
                return (renderers[0].bounds.min, renderers[0].bounds.max);
            }

            // Need to build an overall bounds
            // Cannot simply initialize bounds with new Bounds()
            // need some workaround for this
            // find the first valid bound
            Bounds bounds = renderers[0].bounds;
            int i = 0;
            for (i = 0; i < renderers.Length; i++)
            {
                if (HasValidBound(renderers[i]))
                {
                    bounds = renderers[i].bounds;
                    break;
                }
            }
            // no valid bound found
            if (i == renderers.Length)
            {
                return (Vector3.zero, Vector3.zero);
            }
            // encapsulate the rest of the bounds
            for (i = i + 1; i < renderers.Length; i++)
            {
                if (HasValidBound(renderers[i]))
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }
            return (bounds.min, bounds.max);
        }

        private bool HasValidBound(Renderer renderer)
        {
            return renderer.gameObject.tag != "ARObject";
        }

        public GameObject GetHighlightObject()
        {
            return highlightObject;
        }

        public void SetDisplay(Camera cam, RectTransform displayRect)
        {
            this.cam = cam;
            this.displayRect = displayRect;
        }

        public void DestroyHighlightElement()
        {
            // Destroy the game object
            Destroy(highlightObject);
        }
    }
}
