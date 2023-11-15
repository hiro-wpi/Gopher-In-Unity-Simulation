using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///    Read the velocity input and orientation from the 
///    KatVR treadmill
/// </summary>
public class TreadmillReader : MonoBehaviour
{
    // Scale the velocity input
    [SerializeField] private Vector2 velocityScale;

    // Read state
    [ReadOnly, SerializeField] 
    private Vector2 velocity;
    [ReadOnly, SerializeField]
    private float rotation;

    void Start()
    {
        
    }

    void Update()
    {
        // In case need to filter velocity // Vector2.SmoothDamp()
    }

    public Vector2 GetVelocity()
    {
        return velocity * velocityScale;
    }

    public float GetRotation()
    {
        return rotation;
    }
}