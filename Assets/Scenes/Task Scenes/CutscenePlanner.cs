using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutscenePlanner : MonoBehaviour
{
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private List<Transform> medicineBoxes; 
    [SerializeField] private Transform leftHandHome;
    [SerializeField] private Transform rightHandHome;

    private float holdTimer = 0.0f;
    private float holdDuration = 2.0f;

    private float waitTimer = 0.0f;
    private float minWait = 5.0f;
    private float maxWait = 15.0f;

    private int randomMedicine = 0;

    private enum CutsceneState
    {
        GoingToHome,
        WaitingBeforeMedicine,
        GoingToMedicine,
        WaitingAtMedicine,
        StayAtHome
    }

    private CutsceneState currentState;

    private void Start()
    {
        rightHandTarget.position = rightHandHome.position;
        leftHandTarget.position = leftHandHome.position;
        currentState = CutsceneState.GoingToHome;
    }

    private void Update()
    {
        switch (currentState)
        {
            case CutsceneState.GoingToHome:
                goToHome();
                if (isAtDestination(leftHandHome.position))
                {
                    Debug.Log("Switching state to WaitingBeforeMedicine");
                    currentState = CutsceneState.WaitingBeforeMedicine;
                }
                break;

            case CutsceneState.WaitingBeforeMedicine:
                waitBeforeMedicine();
                Debug.Log("Finished waiting for medicine");
                break;

            case CutsceneState.GoingToMedicine:
                Debug.Log("About to go to medicine");
                goToMedicine();
                Debug.Log("Exited goToMedicine()");
                break;

            case CutsceneState.WaitingAtMedicine:
                Debug.Log("Made it to WaitingAtMedicine!");
                waitAtMedicine();
                break;

            case CutsceneState.StayAtHome:
                goToHome();
                break;
        }
    }

    private void goToHome()
    {
        leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, leftHandHome.position, 1.0f * Time.deltaTime);
    }

    // go to a random medicine in the list and remove it from the list after it has been visited
    // if there are no more medicines in the list, go to home
    private void goToMedicine()
    {
        Debug.Log("medicineBoxes.count: " + medicineBoxes.Count);

        if (medicineBoxes.Count > 0)
        {            
            randomMedicine = Random.Range(0, medicineBoxes.Count - 1);
            Debug.Log("randomMedicine: " + randomMedicine);
            
            leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, medicineBoxes[randomMedicine].position, 1.0f * Time.deltaTime);
            Debug.Log("Going to medicine");

            if (isAtDestination(medicineBoxes[randomMedicine].position))
            {
                Debug.Log("Removed medicine from list");
                medicineBoxes.RemoveAt(randomMedicine);

                Debug.Log("Reached medicine and switching to WaitingAtMedicine");
                currentState = CutsceneState.WaitingAtMedicine;
            }
        }

        else 
        {
            currentState = CutsceneState.StayAtHome;
        }
    }

    private bool isAtDestination(Vector3 destination)
    {
        return Vector3.Distance(leftHandTarget.position, destination) < 0.01f;
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

    private void waitBeforeMedicine()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer >= Random.Range(minWait, maxWait))
        {
            Debug.Log("Switching state to GoingToMedicine");
            currentState = CutsceneState.GoingToMedicine;
            waitTimer = 0.0f;
        }
    }
}