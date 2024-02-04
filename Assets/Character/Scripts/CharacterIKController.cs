using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A controller script that used to control the character IK targets,
///     to control the hands and head motion.
///     
///     One can use the following function to control the character:
///     LookAtTarget(Vector3, float)
///     IsLookingAtTarget(Vector3)
///     MoveLeftHand(Vector3, Quaternion, MotionType)
///     MoveRightHand(Vector3, Quaternion, MotionType)
///     IsLeftHandReached(Vector3, Quaternion)
///     IsRightHandReached(Vector3, Quaternion)
///     OpenLeftHand()
///     OpenRightHand()
///     CloseLeftHand()
///     CloseRightHand()
///     
///     GetHeadIKTarget()
///     GetLeftHandIKTarget()
///     GetRightHandIKTarget()
/// </summary>
public class CharacterIKController : MonoBehaviour
{
    // Head IK motion
    [SerializeField] private Transform headIKTarget;
    private Coroutine lookAtCoroutine;

    // Hand IK motion
    [SerializeField] private Transform leftHandTarget;
    private Coroutine leftHandCoroutine;
    [SerializeField] private Transform rightHandTarget;
    private Coroutine rightHandCoroutine;
    public enum MotionType { Linear, Quadratic }
    // Hand 
    [SerializeField] private Animator animator;

    private float positionTolerance = 0.001f;
    private float rotationTolerance = 1f;

    void Start() {}

    void Update() {}

    public void LookAtTarget(Vector3 targetPosition, float turnSpeed = 1f)
    {
        // Start moving the head
        if (lookAtCoroutine != null) 
        {
            StopCoroutine(lookAtCoroutine);
        }
        lookAtCoroutine = StartCoroutine(
            LookAtTargetCoroutine(targetPosition, turnSpeed)
        );
    }

    private IEnumerator LookAtTargetCoroutine(
        Vector3 targetPosition, float turnSpeed
    )
    {
        Vector3 lookDirection = targetPosition - headIKTarget.position;
        if ((lookDirection - Vector3.zero).magnitude < 0.001f)
        {
            yield break;  // Exit if the target is too close or invalid
        }
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        while (!IsLookingAtTarget(targetRotation))
        {
            headIKTarget.rotation = Quaternion.Slerp(
                headIKTarget.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
            yield return null;  // Wait for the next frame
        }
    }

    public bool IsLookingAtTarget(Vector3 targetPosition)
    {
        Vector3 lookDirection = targetPosition - headIKTarget.position;
        if ((lookDirection - Vector3.zero).magnitude < 0.001f)
        {
            return false;  // Exit if the target is too close or invalid
        }
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        return IsLookingAtTarget(targetRotation);
    }

    public bool IsLookingAtTarget(Quaternion targetRotation)
    {
        return Quaternion.Angle(
            headIKTarget.rotation, targetRotation
        ) < rotationTolerance;
    }

    private void MoveHand(
        Transform handTarget,
        MotionType motionType,
        Vector3 targetPosition,
        Quaternion targetRotation,
        float positionSpeed,
        float rotationSpeed,
        float height,
        Coroutine handCoroutine
    )
    {
        // Start moving the head
        if (handCoroutine != null) 
        {
            StopCoroutine(handCoroutine);
        }
        handCoroutine = StartCoroutine(
            MoveHandCoroutine(
                handTarget,
                motionType,
                targetPosition,
                targetRotation,
                positionSpeed,
                rotationSpeed,
                height
            )
        );
    }

    private IEnumerator MoveHandCoroutine(
        Transform handTarget,
        MotionType motionType,
        Vector3 targetPosition,
        Quaternion targetRotation,
        float positionSpeed,
        float rotationSpeed,
        float height
    )
    {
        float startTime = Time.time;
        Vector3 startPosition = handTarget.position;
        Quaternion startRotation = handTarget.rotation;
        if (Quaternion.Dot(targetRotation, targetRotation) < Mathf.Epsilon)
        {
            targetRotation = startRotation;
        }

        float completionTime = Mathf.Max(
            Vector3.Distance(startPosition, targetPosition) / positionSpeed,
            Quaternion.Angle(startRotation, targetRotation) / rotationSpeed
        );
        if (completionTime < Mathf.Epsilon)
        {
            yield break;  // already reached the target
        }

        float journey = 0f;
        while (journey < 1f)
        {
            journey = Mathf.Clamp01((Time.time - startTime) / completionTime);

            // Interpolate position
            float curvedJourneyY = InterpolateHeight(
                motionType, journey, height
            );
            Vector3 currentPosition = new Vector3(
                Mathf.Lerp(startPosition.x, targetPosition.x, journey),
                Mathf.Lerp(startPosition.y, targetPosition.y, journey)
                    + curvedJourneyY,
                Mathf.Lerp(startPosition.z, targetPosition.z, journey)
            );
            handTarget.position = currentPosition;

            // Interpolate rotation
            handTarget.rotation = Quaternion.Slerp(
                startRotation, targetRotation, journey
            );

            yield return null;
        }
    }

    // Helper interpolation functions
    private float InterpolateHeight(
        MotionType motionType, float t, float height
    )
    {
        switch (motionType)
        {
            case MotionType.Linear:
                return 0;
            case MotionType.Quadratic:
                return QuadraticInterpolation(t, height);
            default:
                return 0;
        }
    }

    private float QuadraticInterpolation(float t, float height)
    {
        return height * (- 4 * t * t + 4 * t);
    }

    private bool IsHandReached(
        Transform handTarget,
        Vector3 targetPosition,
        Quaternion targetRotation
    )
    {
        bool positionReached = (
            handTarget.position - targetPosition
        ).magnitude < positionTolerance;

        bool rotationReached;
        if (Quaternion.Dot(targetRotation, targetRotation) < Mathf.Epsilon)
        {
            rotationReached = true;
        }
        else
        {
            rotationReached = Quaternion.Angle(
                handTarget.rotation, targetRotation
            ) < rotationTolerance;
        }

        return positionReached && rotationReached;
    }

    public void MoveLeftHand(
        MotionType motionType,
        Vector3 targetPosition,
        Quaternion targetRotation = new Quaternion(),
        float positionSpeed = 0.3f,
        float rotationSpeed = 30f,
        float height = 0.1f
    )
    {
        MoveHand(
            leftHandTarget,
            motionType,
            targetPosition,
            targetRotation,
            positionSpeed,
            rotationSpeed,
            height,
            leftHandCoroutine
        );
    }

    public void MoveRightHand(
        MotionType motionType,
        Vector3 targetPosition,
        Quaternion targetRotation = new Quaternion(),
        float positionSpeed = 0.3f,
        float rotationSpeed = 30f,
        float height = 0.1f
    )
    {
        MoveHand(
            rightHandTarget,
            motionType,
            targetPosition,
            targetRotation,
            positionSpeed,
            rotationSpeed,
            height,
            rightHandCoroutine
        );
    }

    public bool IsLeftHandReached(
        Vector3 targetPosition,
        Quaternion targetRotation = new Quaternion()
    )
    {
        return IsHandReached(leftHandTarget, targetPosition, targetRotation);
    }

    public bool IsRightHandReached(
        Vector3 targetPosition,
        Quaternion targetRotation = new Quaternion()
    )
    {
        return IsHandReached(rightHandTarget, targetPosition, targetRotation);
    }

    public void OpenLeftHand()
    {
        animator.SetFloat("Left Grab", 0.0f);
    }

    public void CloseLeftHand()
    {
        animator.SetFloat("Left Grab", 1.0f);
    }

    public void OpenRightHand()
    {
        animator.SetFloat("Left Grab", 0.0f);
    }

    public void CloseRightHand()
    {
        animator.SetFloat("Left Grab", 1.0f);
    }

    // Public getter
    public Transform GetHeadIKTarget()
    {
        return headIKTarget;
    }

    public Transform GetLeftHandIKTarget()
    {
        return leftHandTarget;
    }

    public Transform GetRightHandIKTarget()
    {
        return rightHandTarget;
    }
}
