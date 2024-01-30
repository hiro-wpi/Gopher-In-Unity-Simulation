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
        SelectingRandomMedicine,
        GoingToMedicine,
        WaitingAtMedicine,
        StayingAtHome
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
                break;

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

            case CutsceneState.StayingAtHome:
                goToHome();
                break;
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

    private void goToMedicine()
    {
        leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, medicineBoxes[randomMedicine].position, 1.0f * Time.deltaTime);

        if (isAtDestination(medicineBoxes[randomMedicine].position))
        {
            medicineBoxes.RemoveAt(randomMedicine);
            currentState = CutsceneState.WaitingAtMedicine;
        }
    }

    private void selectRandomMedicine()
    {
        if (medicineBoxes.Count > 0)
        {            
            randomMedicine = Random.Range(0, medicineBoxes.Count - 1);
            currentState = CutsceneState.GoingToMedicine;
        }
        else
        {
            currentState = CutsceneState.StayingAtHome;
        }
    }

    private void waitBeforeMedicine()
    {
        waitTimer += Time.deltaTime;

        if (waitTimer >= Random.Range(minWait, maxWait))
        {
            currentState = CutsceneState.GoingToMedicine;
            selectRandomMedicine();
            waitTimer = 0.0f;
        }
    }

    private void goToHome()
    {
        leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, leftHandHome.position, 1.0f * Time.deltaTime);

        if (isAtDestination(leftHandHome.position))
        {
            currentState = CutsceneState.WaitingBeforeMedicine;
        }
    }

    private bool isAtDestination(Vector3 destination)
    {
        return Vector3.Distance(leftHandTarget.position, destination) < 0.01f;
    }
}