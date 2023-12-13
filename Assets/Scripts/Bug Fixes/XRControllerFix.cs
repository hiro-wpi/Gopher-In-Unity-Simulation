using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRControllerFix : MonoBehaviour
{
    [SerializeField] private Transform playerHead; // Reference to the XR headset or head position

    private CharacterController characterController;
    private Vector3 originalHeadPosition;
    [SerializeField] private float leanThreshold = 0.1f; // Adjust this threshold as needed

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        originalHeadPosition = playerHead.position; // Store the initial head position
    }

    void Update()
    {
        // Calculate the distance moved by the player's head
        float distanceMoved = Vector3.Distance(originalHeadPosition, playerHead.position);

        // If the head moves beyond the lean threshold, prevent collider movement
        if (distanceMoved > leanThreshold)
        {
            // Calculate the movement direction based on the head's movement
            Vector3 movementDirection = (playerHead.position - originalHeadPosition).normalized;

            // Adjust the character controller's position without moving forward
            characterController.Move(-movementDirection * (distanceMoved - leanThreshold));

            // Reset the original head position to prevent constant adjustments
            originalHeadPosition = playerHead.position;
        }
    }
}

