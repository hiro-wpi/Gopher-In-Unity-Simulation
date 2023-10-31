using UnityEngine;

public class IKTargetFollowVRRigAnimation : MonoBehaviour
{
    [Range(0,1)]
    public float turnSmoothness = 0.1f;
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Vector3 headBodyPositionOffset;
    public float headBodyYawOffset;

    void LateUpdate()
    {
        // Update current object's transform
        // The whole body should move according to headset's position and rotation
        transform.position = head.ikTarget.position + headBodyPositionOffset;
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
