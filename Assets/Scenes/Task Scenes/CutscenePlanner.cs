using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutscenePlanner : MonoBehaviour
{
    [SerializeField] private CharacterIKController ikController;
    private CharacterIKController.MotionType motionType = 
        CharacterIKController.MotionType.Quadratic;

    [SerializeField] private Transform leftHandHome;
    [SerializeField] private Transform rightHandHome;
    [SerializeField] private Transform monitor;

    [SerializeField] private List<Transform> medicineHandPositions = new(); 
    [SerializeField] private List<Transform> medicineHeadPositions = new();
    [SerializeField] private List<GameObject> medicineBottles = new();

    private float holdTimer = 0.0f;
    private float holdDuration = 1.0f;
    private float waitTimer = 0.0f;
    private float minWait = 5.0f;
    private float maxWait = 15.0f;

    private float headTurnSpeed = 10.0f;
    private float armMoveSpeed = 0.2f; 
    private int randomMedicine = 0;

    private enum CutsceneState
    {
        WaitingBeforeMedicine,
        GoingToMedicine,
        WaitingAtMedicine,
        GoingToHome,
        StayingAtHome
    }
    private CutsceneState currentState;

    private void Start()
    {
        SendToHome(180, 10);

        currentState = CutsceneState.WaitingBeforeMedicine;
    }

    private void Update()
    {
        switch (currentState)
        {
            case CutsceneState.WaitingBeforeMedicine:
                WaitingBeforeMedicine();
                break;

            case CutsceneState.GoingToMedicine:
                GoingToMedicine();
                break;

            case CutsceneState.WaitingAtMedicine:
                WaitingAtMedicine();
                break;
            
            case CutsceneState.GoingToHome:
                GoingToHome(headTurnSpeed);
                break;

            case CutsceneState.StayingAtHome:
                break;
        }
    }

    private void WaitingBeforeMedicine()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer < Random.Range(minWait, maxWait))
        {
            return;
        }
        waitTimer = 0.0f;

        // Select a random medicine and move towards it
        if (medicineHandPositions.Count > 0)
        {            
            randomMedicine = Random.Range(0, medicineHandPositions.Count - 1);
            ikController.LookAtTarget(
                medicineHeadPositions[randomMedicine].position,
                headTurnSpeed
            );
            ikController.MoveLeftHand(
                motionType, 
                medicineHandPositions[randomMedicine].position,
                positionSpeed: armMoveSpeed,
                height: 0.1f
            );

            currentState = CutsceneState.GoingToMedicine;
        }

        // No more medicine to reach
        else
        {
            SendToHome(headTurnSpeed, armMoveSpeed);
            currentState = CutsceneState.StayingAtHome;
        }
    }

    private void GoingToMedicine()
    {
        ikController.LookAtTarget(
            medicineHeadPositions[randomMedicine].position,
            headTurnSpeed
        );

        if (
            ikController.IsLeftHandReached(
                medicineHandPositions[randomMedicine].position
            )
        )
        {
            currentState = CutsceneState.WaitingAtMedicine;
        }
    }

    private void WaitingAtMedicine()
    {
        holdTimer += Time.deltaTime;
        if (holdTimer < holdDuration)
        {
            return;
        }
        holdTimer = 0.0f;

        ikController.CloseLeftHand();
        medicineBottles[randomMedicine].transform.SetParent(ikController.GetLeftHandIKTarget());   


        // Move back to home
        ikController.LookAtTarget(
            medicineHeadPositions[randomMedicine].position,
            headTurnSpeed
        );
        ikController.MoveLeftHand(
            motionType, 
            leftHandHome.position,
            positionSpeed: armMoveSpeed,
            height: 0.1f
        );

        currentState = CutsceneState.GoingToHome;
    }

    private void GoingToHome(float headSpeed)
    {
        ikController.LookAtTarget(
            medicineHeadPositions[randomMedicine].position,
            headTurnSpeed
        );

        if (
            ikController.IsLeftHandReached(
                leftHandHome.position
            )
        )
        {
            ikController.OpenLeftHand();
            if (ikController.GetLeftHandIKTarget().childCount > 0)
            {
                Transform child = ikController.GetLeftHandIKTarget().GetChild(0);
                child.SetParent(null);
            }
            medicineHeadPositions.RemoveAt(randomMedicine);
            medicineHandPositions.RemoveAt(randomMedicine);
            medicineBottles.RemoveAt(randomMedicine);

            ikController.LookAtTarget(monitor.position, headSpeed);

            currentState = CutsceneState.WaitingBeforeMedicine;
        }
    }

    private void SendToHome(float headSpeed, float armSpeed)
    {
        ikController.MoveLeftHand(
            motionType,
            leftHandHome.position,
            positionSpeed: armSpeed,
            height: 0.1f
        );
        ikController.MoveRightHand(
            motionType,
            rightHandHome.position,
            positionSpeed: armSpeed,
            height: 0.1f
        );
        ikController.LookAtTarget(monitor.position, headSpeed);
    }
}