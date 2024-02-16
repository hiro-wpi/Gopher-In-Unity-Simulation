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
    [SerializeField] private Transform leftHandRaise;
    [SerializeField] private Transform generalMedicineEyeLocation;
    [SerializeField] private Transform leftHandHomeEyeLocation;
    [SerializeField] private Transform monitor;
    [SerializeField] private GameObject handARPrefab;
    private GameObject handAR;
    private float handAROffset = -0.076144f;
    [SerializeField] private GameObject eyeARPrefab;
    private GameObject eyeAR;
    private float eyeAROffset = 0.03f;

    [SerializeField] private List<Transform> medicineHandPositions = new(); 
    [SerializeField] private List<Transform> medicineHeadPositions = new();
    [SerializeField] private List<GameObject> medicineBottles = new();

    private float holdTimer = 0.0f;
    private float holdDuration = 0.5f;
    private float waitTimer = 0.0f;
    private float waitDuration = 2.0f;
    private float raiseTimer = 0.0f;
    private float raiseDuration = 1.5f;
    private float eyeARTimer = 0.0f;
    private float eyeARDuration = 1.0f;
    // private float startTimer = 0.0f;
    // private float startDuration = 4.0f;

    private float headTurnSpeed = 10.0f;
    private float armMoveSpeed = 0.3f; 
    private int currentMedicineIndex = 0;

    private enum CutsceneState
    {
        StartState,
        WaitingBeforeMedicine,
        RaisingHand,
        HighlightingMedicine,
        GoingToMedicine,
        WaitingAtMedicine,
        GoingToHome,
        StayingAtHome
    }
    private CutsceneState currentState;

    private void Start()
    {
        SendToHome(180, 10);

        currentState = CutsceneState.StartState;
    }

    private void Update()
    {
        switch (currentState)
        {
            case CutsceneState.StartState:
                // startTimer += Time.deltaTime;
                // if (startTimer < startDuration)
                // {
                //     return;
                // }
                // startTimer = 0.0f;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    currentState = CutsceneState.WaitingBeforeMedicine;
                }

                break;

            case CutsceneState.WaitingBeforeMedicine:
                WaitingBeforeMedicine();
                break;

            case CutsceneState.RaisingHand:
                RaisingHand();
                break;

            case CutsceneState.HighlightingMedicine:
                HighlightingMedicine();
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
        if (waitTimer < waitDuration)
        {
            return;
        }
        waitTimer = 0.0f;

        if (currentMedicineIndex < medicineHandPositions.Count)
        {
            ikController.LookAtTarget(generalMedicineEyeLocation.position, headTurnSpeed);

            ikController.MoveLeftHand(motionType, leftHandRaise.position, positionSpeed: armMoveSpeed, height: 0.1f);

            handAR = Instantiate(handARPrefab, ikController.GetLeftHandIKTarget().position + new Vector3(handAROffset, 0f, 0f), handARPrefab.transform.rotation);
            handAR.transform.SetParent(ikController.GetLeftHandIKTarget());

            currentState = CutsceneState.RaisingHand;
        }

        else
        {
            SendToHome(headTurnSpeed, armMoveSpeed);
            currentState = CutsceneState.StayingAtHome;
        }
    }

    private void RaisingHand()
    {
        raiseTimer += Time.deltaTime;
        if (raiseTimer < raiseDuration)
        {
            return;
        }
        raiseTimer = 0.0f;

        eyeAR = Instantiate(eyeARPrefab, medicineBottles[currentMedicineIndex].transform.position + new Vector3(0f, eyeAROffset, 0f), eyeARPrefab.transform.rotation);
        eyeAR.transform.SetParent(medicineBottles[currentMedicineIndex].transform);

        currentState = CutsceneState.HighlightingMedicine;
    }

    private void HighlightingMedicine()
    {
        eyeARTimer += Time.deltaTime;
        if (eyeARTimer < eyeARDuration)
        {
            return;
        }
        eyeARTimer = 0.0f;
        
        ikController.MoveLeftHand(motionType, medicineHandPositions[currentMedicineIndex].position, positionSpeed: armMoveSpeed, height: 0.1f);
        currentState = CutsceneState.GoingToMedicine;
    }

    private void GoingToMedicine()
    {
        ikController.LookAtTarget(medicineHeadPositions[currentMedicineIndex].position, headTurnSpeed);

        if (ikController.IsLeftHandReached(medicineHandPositions[currentMedicineIndex].position))
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

        medicineBottles[currentMedicineIndex].transform.SetParent(ikController.GetLeftHandIKTarget());

        ikController.LookAtTarget(medicineHeadPositions[currentMedicineIndex].position, headTurnSpeed);
        ikController.MoveLeftHand(motionType, leftHandHome.position, positionSpeed: armMoveSpeed, height: 0.1f);

        currentState = CutsceneState.GoingToHome;
    }

    private void GoingToHome(float headSpeed)
    {
        ikController.LookAtTarget(leftHandHomeEyeLocation.position, headTurnSpeed);

        if (ikController.IsLeftHandReached(leftHandHome.position))
        {
            ikController.OpenLeftHand();
            if (ikController.GetLeftHandIKTarget().childCount > 0)
            {
                Transform child = ikController.GetLeftHandIKTarget().GetChild(1);
                child.SetParent(null);
            }
            
            Destroy(eyeAR);
            Destroy(handAR);
            
            currentMedicineIndex++;

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