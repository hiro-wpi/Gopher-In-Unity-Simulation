using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///    Move the IK target for animation rigging according to the VR rig
/// </summary>
[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
    public void Map()
    {
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(
            trackingRotationOffset
        );
    }
}

public class IKTargetFollowVRRig : MonoBehaviour
{
    [Range(0,1)]
    [SerializeField] private  float turnSmoothness = 0.1f;
    [SerializeField] private  VRMap head;
    [SerializeField] private  VRMap leftHand;
    [SerializeField] private  VRMap rightHand;

    // Apply this offset so that the head target matches the head bone
    [SerializeField] private Vector3 headPositionOffset;

    void Start() {}

    void LateUpdate()
    {
        // Update current object's transform
        // The whole body should move according to headset's position and rotation
        transform.position = head.ikTarget.position + headPositionOffset;
        float yaw = head.vrTarget.eulerAngles.y;
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z),
            turnSmoothness
        );

        // Update IK targets
        head.Map();
        leftHand.Map();
        rightHand.Map();
    }
}
