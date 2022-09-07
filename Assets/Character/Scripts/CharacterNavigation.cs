using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterNavigation : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;

    private Vector3 worldVelocity;
    private Vector3 agentVelocity;
    private float forward;
    private float turn;

    public bool loop;
    private Vector3[] targetTrajectory;
    private int currentIndex = 0;


    void Start()
    {
    }

    void Update()
    {
        // Update animator according to agent velocity
        // Convert from world to local velocity
        worldVelocity = agent.desiredVelocity;
        if (worldVelocity.magnitude > 1f) 
            worldVelocity.Normalize(); // animation blender range [0, 1]
        agentVelocity = agent.transform.InverseTransformDirection(worldVelocity);
        UpdateAnimator(agentVelocity);
    }

    private void UpdateAnimator(Vector3 velocity)
    {
        // Compute turn and forward amount
        forward = velocity.z;
        turn = Mathf.Atan2(velocity.x, velocity.z);
        
        // Update animator
        animator.SetFloat("Forward", forward, 0.1f, Time.deltaTime);
        animator.SetFloat("Turn", turn, 0.1f, Time.deltaTime);
    }


    public void SetTrajectory(Vector3[] trajectory)
    {
        targetTrajectory = trajectory;

        currentIndex = 0;
        agent.SetDestination(targetTrajectory[currentIndex]);

        CancelInvoke("CheckDestination");
        InvokeRepeating("CheckDestination", 0.2f, 0.2f);
    }

    private void CheckDestination()
    {
        // agent may get destroyed before this script
        if (agent == null || !agent.isActiveAndEnabled)
            return;
        
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // Next target
            currentIndex += 1;
            if (loop)
                currentIndex = currentIndex % targetTrajectory.Length;
            else
                if (currentIndex == targetTrajectory.Length)
                {
                    targetTrajectory = null;
                    CancelInvoke("CheckDestination");
                    return;
                }
            agent.SetDestination(targetTrajectory[currentIndex]);
        }
    }
}
