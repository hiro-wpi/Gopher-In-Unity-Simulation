using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterWalk : MonoBehaviour
{
    public GameObject character;
    private Rigidbody rb;
    private Vector3 prevPosition;
    private float rbSpeed;
    private Transform tf;

    public float speed = 1.0f;
    public float angularSpeed = 1.0f;
    private float angularSpeedDegree;

    public bool loop;
    public Vector3[] targetTrajectory;
    private int currentIndex;
    public Vector3 currentTarget;
    private Quaternion currentTargetRotation;

    private Animator animator;

    private bool isBlocked;

    void Start()
    {
        rb = character.GetComponent<Rigidbody>();
        tf = character.GetComponent<Transform>();
        prevPosition = tf.position;

        angularSpeedDegree = angularSpeed * Mathf.Rad2Deg;

        currentTarget = tf.position;
        if (targetTrajectory.Length != 0)
            MoveTrajectory(targetTrajectory);

        animator = character.GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        // Animation
        rbSpeed = (tf.position - prevPosition).magnitude / Time.fixedDeltaTime;
        prevPosition = tf.position;
        animator.SetFloat("speed", rbSpeed);

        // Check collision
        RaycastHit hit;
        int i = -2;
        while (i < 2)
        {
            ++i;
            Vector3 detectPosition = tf.position + tf.TransformDirection(new Vector3(i*0.3f, 0.2f, 0));
            Vector3 detectRotation = tf.forward;
            if (Physics.Raycast(detectPosition, detectRotation, out hit, 1.0f))
            {
                Debug.DrawRay(detectPosition, hit.distance * detectRotation, Color.red, 0.1f);
                if (hit.collider.gameObject.layer == 8)
                    return;
            }
        }

        // Track current target
        if ((tf.position - currentTarget).magnitude > 0.1)
        {
            Vector3 position = Vector3.MoveTowards(tf.position, currentTarget,
                                                   speed * Time.fixedDeltaTime);
            rb.MovePosition(position);
            Quaternion rotation = Quaternion.RotateTowards(tf.rotation, currentTargetRotation,
                                                   angularSpeedDegree * Time.fixedDeltaTime);
            rb.MoveRotation(rotation);
        }
        // Start next target or Idle
        else
        {
            if (targetTrajectory.Length == 0)
                return;

            if (loop)
            {
                currentIndex = (currentIndex + 1) % targetTrajectory.Length;
                MoveTo(targetTrajectory[currentIndex]);
            }
            else
            {
                if (currentIndex + 1 != targetTrajectory.Length)
                {
                    currentIndex += 1;
                    MoveTo(targetTrajectory[currentIndex]);
                }
            }
        }
    }

    public void MoveTo(Vector3 target)
    {
        currentTarget = target;
        currentTargetRotation = Quaternion.LookRotation(currentTarget - tf.position);
    }

    public void MoveTrajectory(Vector3[] trajectory)
    {
        targetTrajectory = trajectory;
        currentIndex = 0;
        MoveTo(targetTrajectory[currentIndex]);
    }
}
