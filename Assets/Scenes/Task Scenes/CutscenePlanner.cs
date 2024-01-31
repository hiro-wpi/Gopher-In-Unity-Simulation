using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutscenePlanner : MonoBehaviour
{
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform leftHandHome;
    [SerializeField] private Transform rightHandHome;
    [SerializeField] private Transform headIKTarget;
    [SerializeField] private Transform monitor;
    [SerializeField] private Animator animator;
    [SerializeField] private List<Transform> medicineHandPositions; 
    [SerializeField] private List<Transform> medicineHeadPositions;
    [SerializeField] private List<GameObject> medicineBottles;

    private float holdTimer = 0.0f;
    private float holdDuration = 1.0f;
    private float waitTimer = 0.0f;
    private float minWait = 5.0f;
    private float maxWait = 15.0f;
    private float headTurnSpeed = 2.0f;
    private float armMoveSpeed = 2.0f; 
    private int randomMedicine = 0;
    private float t = 0.01f;

    private enum CutsceneState
    {
        WaitingBeforeMedicine,
        SelectingRandomMedicine,
        GoingToMedicine,
        WaitingAtMedicine,
        GoingToHome,
        PrepAfterHome,
        StayingAtHome
    }

    private CutsceneState currentState;

    private void Start()
    {
        startLookAtTarget(monitor);
        rightHandTarget.position = rightHandHome.position;
        leftHandTarget.position = leftHandHome.position;
        currentState = CutsceneState.WaitingBeforeMedicine;
    }

    private void Update()
    {
        switch (currentState)
        {
            case CutsceneState.WaitingBeforeMedicine:
                waitBeforeMedicine();
                break;

            case CutsceneState.SelectingRandomMedicine:
                selectRandomMedicine();
                break;

            case CutsceneState.GoingToMedicine:
                goToMedicine();
                break;

            case CutsceneState.WaitingAtMedicine:
                waitAtMedicine();
                break;
            
            case CutsceneState.GoingToHome:
                goToHome();
                break;

            case CutsceneState.PrepAfterHome:
                prepAfterHome();
                break;

            case CutsceneState.StayingAtHome:
                stayAtHome();
                break;
        }
    }

    private void waitBeforeMedicine()
    {
        waitTimer += Time.deltaTime;

        if (waitTimer >= Random.Range(minWait, maxWait))
        {
            currentState = CutsceneState.SelectingRandomMedicine;
            waitTimer = 0.0f;
        }
    }    

    private void selectRandomMedicine()
    {
        if (medicineHandPositions.Count > 0)
        {            
            randomMedicine = Random.Range(0, medicineHandPositions.Count - 1);
            currentState = CutsceneState.GoingToMedicine;
        }
        else
        {
            currentState = CutsceneState.StayingAtHome;
        }
    }

    private void goToMedicine()
    {
        if (medicineHeadPositions.Count <= 0)
        {
            return;
        }

        lookAtTarget(medicineHeadPositions[randomMedicine]);

        leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, medicineHandPositions[randomMedicine].position, armMoveSpeed * Time.deltaTime);
        // leftHandTarget.position = QuadraticInterpolation(leftHandTarget.position, medicineHandPositions[randomMedicine].position, t);

        if (isAtDestination(medicineHandPositions[randomMedicine].position))
        {
            closeHand();

            medicineBottles[randomMedicine].transform.SetParent(leftHandTarget);   

            currentState = CutsceneState.WaitingAtMedicine;
        }
    }

    private void waitAtMedicine()
    {
        holdTimer += Time.deltaTime;

        if (holdTimer >= holdDuration)
        {
            currentState = CutsceneState.GoingToHome;
            holdTimer = 0.0f;
        }
    }

    private void goToHome()
    {
        if (medicineHeadPositions.Count <= 0)
        {
            return;
        }

        lookAtTarget(medicineHeadPositions[randomMedicine]);

        leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, leftHandHome.position, armMoveSpeed * Time.deltaTime);

        if (isAtDestination(leftHandHome.position))
        {
            medicineBottles.RemoveAt(randomMedicine);
            medicineHandPositions.RemoveAt(randomMedicine);
            medicineHeadPositions.RemoveAt(randomMedicine);
            currentState = CutsceneState.PrepAfterHome;
        }
    }

    private void prepAfterHome()
    {
        openHand();
        
        if (leftHandTarget.childCount > 0)
        {
            Transform child = leftHandTarget.GetChild(0);
            child.SetParent(null);
        }
        
        lookAtTarget(monitor);

        if (isLookingAtTarget(monitor))
        {
            currentState = CutsceneState.WaitingBeforeMedicine;
        }
    }

    private void stayAtHome()
    {
        leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, leftHandHome.position, armMoveSpeed * Time.deltaTime);
        lookAtTarget(monitor);
    }

    private Vector3 QuadraticInterpolation(Vector3 start, Vector3 end, float t)
    {
        float tSquared = t * t;
        Vector3 lerpedPosition = Vector3.Lerp(start, end, t);
        lerpedPosition.z = Mathf.Lerp(start.z, end.z, 1 - tSquared);
        return lerpedPosition;
    }

    private bool isLookingAtTarget(Transform targetObject)
    {
        Vector3 lookDirection = targetObject.position - headIKTarget.position;
        float dotProduct = Vector3.Dot(lookDirection.normalized, headIKTarget.forward);
        return dotProduct > 0.99f;
    }    

    private void lookAtTarget(Transform targetObject)
    {
        Vector3 lookDirection = targetObject.position - headIKTarget.position;

        if (lookDirection != Vector3.zero)
        {
            // headIKTarget.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            headIKTarget.rotation = Quaternion.Slerp(headIKTarget.rotation, targetRotation, headTurnSpeed * Time.deltaTime);        
        }
    }

    private void startLookAtTarget(Transform targetObject)
    {
        Vector3 lookDirection = targetObject.position - headIKTarget.position;

        if (lookDirection != Vector3.zero)
        {
            headIKTarget.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);    
        }        
    }

    private bool isAtDestination(Vector3 destination)
    {
        return Vector3.Distance(leftHandTarget.position, destination) < 0.01f;
    }

    private void openHand()
    {
        animator.SetFloat("Left Grab", 0.0f);
    }

    private void closeHand()
    {
        animator.SetFloat("Left Grab", 1.0f);
    }    
}