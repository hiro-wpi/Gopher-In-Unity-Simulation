using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///    Read the velocity and orientation of the
///    Kat Walk treadmill
/// </summary>
public class TreadmillReader : MonoBehaviour
{
    [SerializeField] 
    private Vector2 velocityScale = new Vector2(1.0f, 1.0f);
    [SerializeField, ReadOnly] 
    private Vector2 currentVelocity = Vector2.zero;
    [SerializeField, ReadOnly] 
    private float rotation = 0.0f;

    private Vector2 smoothVelocity = new Vector2(0.0f, 0.0f);
    private float smoothRotation = 0.0f;

    void Update()
    {
        // Get velocity from the Kat Walk treadmill
        var ws = KATNativeSDK.GetWalkStatus();
        Vector2 targetVelocity = new Vector2(ws.moveSpeed.x, ws.moveSpeed.z);
        float targetRotation = ws.bodyRotationRaw.eulerAngles.y;

        // Apply smoothing
        currentVelocity = Vector2.SmoothDamp(
            currentVelocity, targetVelocity, ref smoothVelocity, 0.1f
        );
        rotation = Mathf.SmoothDamp(
            rotation, targetRotation, ref smoothRotation, 0.1f
        );
    }

    public Vector2 GetVelocity()
    {
        return currentVelocity * velocityScale;
    }

    public float GetRotation()
    {
        return rotation;
    }
}
