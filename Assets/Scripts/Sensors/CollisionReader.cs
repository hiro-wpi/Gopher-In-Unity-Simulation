using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script is used to assign ArticulationCollisionDetection to
///     each articulation body in the game object,
///     and read collision if happened.
///     Collision is saved in two array of size 5:
///     collisionSelfNames and collisionOtherNames.
/// </summary>
public class CollisionReader : MonoBehaviour
{
    public GameObject robotRoot;
    public GameObject[] extraObjects;
    private ArticulationBody[] articulationBodyChain;

    public AudioClip collisionAudioClip;
    private AudioSource collisionAudio;

    public int storageIndex = -1;
    public int storageLength = 3;
    public string[] collisionSelfNames;
    public string[] collisionOtherNames;
    public float[] collisionRelativeSpeed;

    void Awake()
    {
        // Audio effect
        collisionAudio = gameObject.AddComponent<AudioSource>();
        collisionAudio.clip = collisionAudioClip;
        collisionAudio.volume = 0.5f;

        // Get collision detections
        // robot
        articulationBodyChain = robotRoot.GetComponentsInChildren<ArticulationBody>();
        foreach (ArticulationBody articulationBody in articulationBodyChain)
        {
            GameObject parent = articulationBody.gameObject;
            ArticulationCollisionDetection collisionDetection =
                                           parent.AddComponent<ArticulationCollisionDetection>();
            collisionDetection.setParent(robotRoot);
        }
        foreach (GameObject extraObject in extraObjects)
        {
            ArticulationCollisionDetection collisionDetection = 
                                           extraObject.AddComponent<ArticulationCollisionDetection>();
            collisionDetection.setParent(robotRoot);
        }

        // To store collision information
        collisionSelfNames = new string[storageLength];
        collisionOtherNames = new string[storageLength];
        collisionRelativeSpeed = new float[storageLength];
    }

    void Update()
    {
    }

    public void OnCollision(string self, string other, float relativeSpeed)
    {
        // Prevent too frequent collision detection
        if (!collisionAudio.isPlaying)
        {
            collisionAudio.volume = relativeSpeed*0.3f;
            collisionAudio.Play();

            // Store collision self, other and relative speed
            storageIndex = (storageIndex + 1) % storageLength;
            collisionSelfNames[storageIndex] = self;
            collisionOtherNames[storageIndex] = other;
            collisionRelativeSpeed[storageIndex] = relativeSpeed;
        }
    }
}