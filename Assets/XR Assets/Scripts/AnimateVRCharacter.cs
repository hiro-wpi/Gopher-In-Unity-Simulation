using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///    Change animation of the VR character 
///    based on XR controller input and
///    the movement of the VR tracking objects.
/// </summary>
[System.Serializable]
public class AnimationInput
{
    public string animationPropertyName;
    public InputActionProperty action;
}

public class AnimateVRCharacter : MonoBehaviour
{
    // Animator
    [SerializeField] private Animator animator;
    [SerializeField] private bool animateHandsOnly = false;
    
    // Controller input
    [SerializeField] private List<AnimationInput> animationInputs;
    // Transform
    private Vector3 prevPosition;
    private Quaternion prevRotation;
    [SerializeField, ReadOnly] private Vector3 localVelocity;
    [SerializeField, ReadOnly] private Vector3 localAngularVelocity;
    private Vector3 smoothLocalVelocity;
    private Vector3 smoothLocalAngularVelocity;

    void Start()
    {
        prevPosition = transform.position;
        prevRotation = transform.rotation;
    }

    void Update()
    {
        // Read controller input
        foreach (var item in animationInputs)
        {
            float actionValue = item.action.action.ReadValue<float>();
            animator.SetFloat(item.animationPropertyName, actionValue);
        }

        if (animateHandsOnly)
        {
            return;
        }

        // Update local velocity and set animator parameters
        UpdateLocalVelocity();
        animator.SetFloat("Linear X", localVelocity.x);
        animator.SetFloat("Angular Y", localAngularVelocity.y);
        animator.SetFloat("Linear Z", localVelocity.z);
    }

    void UpdateLocalVelocity()
    {
        // Compute local velocity
        Vector3 currLocalVelocity = transform.InverseTransformDirection(
            (transform.position - prevPosition) / Time.deltaTime
        );
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(
            prevRotation
        );
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f)
        {
            angle -= 360f;
        }
        Vector3 currLocalAngularVelocity = transform.InverseTransformDirection(
            (angle * axis * Mathf.Deg2Rad) / Time.deltaTime
        );
        // update
        prevPosition = transform.position;
        prevRotation = transform.rotation;

        // Set animator parameters
        // Apply a simple filter to localVelocity and localAngularVelocity
        if (localVelocity.magnitude < 0.01f)
        {
            localVelocity = Vector3.zero;
        }
        if (localAngularVelocity.magnitude < 0.01f)
        {
            localAngularVelocity = Vector3.zero;
        }
        localVelocity = Vector3.SmoothDamp(
            localVelocity, currLocalVelocity, ref smoothLocalVelocity, 0.1f
        );
        localAngularVelocity = Vector3.SmoothDamp(
            localAngularVelocity, currLocalAngularVelocity, ref smoothLocalAngularVelocity, 0.1f
        );
    }
}
