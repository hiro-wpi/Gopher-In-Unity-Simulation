using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreadmillReader : MonoBehaviour
{
    [SerializeField] private float smoothTime = 0.2f; // Adjust this value for smoother or quicker dampening
    
    private Vector2 currentVelocity = new Vector2(0.0f, 0.0f); // Store the current velocity for SmoothDamp

    [SerializeField] private Vector2 velocityScale = new Vector2(1.0f, 1.0f);

    [SerializeField] private Vector2 velocityXZ = Vector2.zero;

    [SerializeField] private float rotationY = 0.0f;

    void Update()
    {
        var ws = KATNativeSDK.GetWalkStatus();

        Vector2 targetVelocity = new Vector2(ws.moveSpeed.x, ws.moveSpeed.z);

        // Apply smoothing using Vector2.SmoothDamp()
        velocityXZ = Vector2.SmoothDamp(velocityXZ, targetVelocity, ref currentVelocity, smoothTime);

        // Smooth this
        // rotationY = ws.bodyRotationRaw.eulerAngles.y;
    }

    public Vector2 GetVelocity()
    {
        return velocityXZ * velocityScale;
    }

    public float GetRotation()
    {
        return rotationY;
    }
}
