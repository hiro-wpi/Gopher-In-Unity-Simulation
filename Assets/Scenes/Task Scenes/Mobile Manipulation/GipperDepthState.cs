using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GipperDepthState : MonoBehaviour
{
    [SerializeField] private Renderer rend;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material triggerMaterial;

    void Start() { }

    void Update() { }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Robot"))
        {
            rend.material = triggerMaterial;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Robot"))
        {
            rend.material = defaultMaterial;
        }
    }
}
