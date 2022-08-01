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

    public int storageIndex = 0;
    public int storageLength = 5;
    public string[] collisionSelfNames;
    public string[] collisionOtherNames;

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
    }

    void Update()
    {
    }

    public void OnCollision(string self, string other, float relativeSpeed)
    {
        if (!collisionAudio.isPlaying)
        {
            collisionAudio.volume = relativeSpeed*0.3f + 0.1f;
            collisionAudio.Play();

            // Temporary
            collisionSelfNames[storageIndex] = self;
            collisionOtherNames[storageIndex] = other;
            storageIndex = (storageIndex + 1) % storageLength;
        }
    }
}