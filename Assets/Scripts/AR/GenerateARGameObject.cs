using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Generate AR game object (in the 3D world) 
///     to be used as AR feature
///     
///     Generate AR game object with the following options:
///     Instantiate()
///     - Instantiate a game object with a ARObjectType
///     - Instantiate a game object with a prefab
///     The generated AR game object will be a child of the given game object
/// </summary>
public class GenerateARGameObject : MonoBehaviour
{
    [SerializeField] private Material transparentMaterial;
    [ReadOnly]
    public Dictionary<GameObject, List<GameObject>> Highlights = new ();

    public enum ARObjectType
    {
        Cube,
        Sphere
    }

    void Start() {}

    void Update() {}

    public void Instantiate(
        GameObject gameObject,
        GameObject arObjectPrefab,
        Vector3 positionOffset = default(Vector3),
        Vector3 rotationOffset = default(Vector3),
        Vector3 scale = default(Vector3),
        Color? color = null,
        float transparency = 0.5f
    )
    
    {
        GameObject arObject = GameObject.Instantiate(arObjectPrefab);
        arObject.tag = "ARObject";
        arObject.layer = LayerMask.NameToLayer("ARObject");
        if (Highlights.ContainsKey(gameObject))
        {
            Highlights[gameObject].Add(arObject);
        }
        else
        {
            Highlights.Add(gameObject, new List<GameObject> { arObject });
        }

        // Set the game object
        SetupGameObject(
            arObject,
            gameObject,
            positionOffset,
            rotationOffset,
            scale,
            color,
            transparency
        );
    }

    public void Instantiate(
        GameObject gameObject,
        ARObjectType arObjectType,
        Vector3 positionOffset = default(Vector3),
        Vector3 rotationOffset = default(Vector3),
        Vector3 scale = default(Vector3),
        Color? color = null,
        float transparency = 0.5f
    )
    {
        // Create the game object based on the type
        GameObject arObject;
        if (arObjectType == ARObjectType.Cube)
        {
            arObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
        else if (arObjectType == ARObjectType.Sphere)
        {
            arObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }
        else
        {
            return;
        }
        arObject.tag = "ARObject";
        arObject.layer = LayerMask.NameToLayer("ARObject");

        if (Highlights.ContainsKey(gameObject))
        {
            Highlights[gameObject].Add(arObject);
        }
        else
        {
            Highlights.Add(gameObject, new List<GameObject> { arObject });
        }

        // Set the game object
        SetupGameObject(
            arObject,
            gameObject,
            positionOffset,
            rotationOffset,
            scale,
            color,
            transparency
        );
    }

    private void SetupGameObject(
        GameObject arObject,
        GameObject gameObject,
        Vector3 positionOffset,
        Vector3 rotationOffset,
        Vector3 scale,
        Color? color = null,
        float transparency = 0.5f
    )
    {
        // Set the default values
        if (scale == default(Vector3))
            scale = Vector3.one;

        // Set parent
        arObject.transform.parent = gameObject.transform;

        // Set the position and rotation
        arObject.transform.position = gameObject.transform.position;
        arObject.transform.rotation = gameObject.transform.rotation;
        arObject.transform.localScale = scale;
        // Set the position and rotation offset relative to the parent
        arObject.transform.position += positionOffset;
        arObject.transform.Rotate(rotationOffset, Space.Self);

        // Disable the collider
        var colliders = arObject.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // If no color provided, do not override material
        if (color == null)
        {
            return;
        }

        // Change the color and transparency
        // create a new material
        Material material = new Material(transparentMaterial);
        // set transparency
        Color newColor = (Color)color;
        newColor.a = transparency;
        material.color = newColor;
        // set the color
        var renderer = arObject.GetComponentsInChildren<Renderer>();
        foreach (var r in renderer)
        {
            r.material = material;
        }
    }

    public List<GameObject> GetARGameObject(GameObject gameObject)
    {
        if (Highlights.ContainsKey(gameObject))
        {
            return Highlights[gameObject];
        }
        else
        {
            return new List<GameObject>();
        }
    }

    public void Destroy(GameObject gameObject, int index = -1)
    {
        if (Highlights.ContainsKey(gameObject))
        {
            // Destroy everything
            if (index < 0)
            {
                foreach (var arObject in Highlights[gameObject])
                {
                    GameObject.Destroy(arObject);
                }
                Highlights.Remove(gameObject);
            }
            
            // Destroy the specific index of the game object
            // if exists
            else
            {
                if (index >= Highlights[gameObject].Count)
                {
                    return;
                }
                GameObject.Destroy(Highlights[gameObject][index]);
                Highlights[gameObject].RemoveAt(index);

                // if this is the last game object
                if (Highlights[gameObject].Count == 0)
                {
                    Highlights.Remove(gameObject);
                }
            }
        }
    }
}
