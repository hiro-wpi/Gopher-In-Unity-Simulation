using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterWalk : MonoBehaviour
{
    // Character
    public GameObject character;
    private Transform tf;
    private Rigidbody rb;
    private float characterSpeed;

    // Target speed
    public float speed = 1.0f;
    public float angularSpeed = 1.0f;

    // Target trajectory
    public bool loop;
    public Vector3[] targetTrajectory;
    private int currentIndex;
    private Vector3 prevPosition;
    public Vector3 currentTarget;
    private Quaternion currentTargetRotation;

    // Animation
    private Animator animator;

    // Other
    private bool isBlocked;

    void Start()
    {
        rb = character.GetComponent<Rigidbody>();
        tf = character.GetComponent<Transform>();
        animator = character.GetComponentInChildren<Animator>();
        animator.SetFloat("speed", 0);
        
        prevPosition = tf.position;
        currentTarget = tf.position;

        // If given target trajectory
        if (targetTrajectory.Length != 0)
            SetTrajectory(targetTrajectory);
    }

    void FixedUpdate()
    {
        // If not given target trajectory
        if (targetTrajectory.Length == 0)
            return;
        
        // Animation
        characterSpeed = (tf.position - prevPosition).magnitude / Time.fixedDeltaTime;
        prevPosition = tf.position;
        animator.SetFloat("speed", characterSpeed);

        // Check collision forwards with 3 rays
        RaycastHit hit;
        for (int i = -1; i < 2; i++)
        {
            Vector3 detectPosition = tf.position + tf.TransformDirection(new Vector3(i*0.3f, 0.2f, 0));
            Vector3 detectRotation = tf.forward;
            if (Physics.Raycast(detectPosition, detectRotation, out hit, 1.0f))
            {
                // Debug.DrawRay(detectPosition, hit.distance * detectRotation, Color.red, 0.1f);
                // if it is robot
                if (hit.collider.gameObject.layer == 8)
                    return;
            }
        }

        // Track current target
        if ((tf.position - currentTarget).magnitude > 0.1)
        {
            Vector3 position = Vector3.MoveTowards(tf.position, currentTarget,
                                                   speed * Time.fixedDeltaTime);
            Quaternion rotation = Quaternion.RotateTowards(tf.rotation, currentTargetRotation,
                                                           angularSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime);
            rb.MovePosition(position);
            rb.MoveRotation(rotation);
        }
        // Start next target or Idle
        else
        {
            MoveTo(targetTrajectory[currentIndex]);
            // Next target
            if (loop)
                currentIndex = (currentIndex + 1) % targetTrajectory.Length;
            else
                if (currentIndex != targetTrajectory.Length - 1)
                    currentIndex += 1;
        }
    }

    public void MoveTo(Vector3 target)
    {
        // Target position
        currentTarget = target;
        // Target rotation
        Vector3 diff = currentTarget - tf.position;
        if (diff != Vector3.zero)
            currentTargetRotation = Quaternion.LookRotation(diff);
        else
            currentTargetRotation = tf.rotation;
    }

    public void SetTrajectory(Vector3[] trajectory)
    {
        targetTrajectory = trajectory;
        currentIndex = 0;
    }
}
