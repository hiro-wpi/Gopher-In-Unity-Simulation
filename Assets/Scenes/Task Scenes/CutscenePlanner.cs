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
    private float holdDuration = 1.0f;
    private float waitTimer = 0.0f;
    private float minWait = 5.0f;
    private float maxWait = 10.0f;
    private float raiseTimer = 0.0f;
    // private float raiseDuration = 5.0f;

    private float headTurnSpeed = 10.0f;
    private float armMoveSpeed = 0.2f; 
    private int currentMedicineIndex = 0;
    // private int randomMedicine = 0;

    private enum CutsceneState
    {
        WaitingBeforeMedicine,
        RaisingHand,
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

            case CutsceneState.RaisingHand:
                RaiseHand();
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

        // // Select a random medicine and move towards it
        // if (medicineHandPositions.Count > 0)
        // {            
        //     randomMedicine = Random.Range(0, medicineHandPositions.Count - 1);
        //     ikController.LookAtTarget(medicineHeadPositions[randomMedicine].position, headTurnSpeed);
        //     ikController.MoveLeftHand(motionType, medicineHandPositions[randomMedicine].position, positionSpeed: armMoveSpeed, height: 0.1f);

        //     currentState = CutsceneState.GoingToMedicine;
        // }

        if (currentMedicineIndex < medicineHandPositions.Count)
        {
            ikController.LookAtTarget(medicineHeadPositions[currentMedicineIndex].position, headTurnSpeed);

            // ikController.MoveLeftHand(motionType, medicineHandPositions[currentMedicineIndex].position, positionSpeed: armMoveSpeed, height: 0.1f);
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

    private void RaiseHand()
    {
        raiseTimer += Time.deltaTime;
        if (raiseTimer < Random.Range(minWait, maxWait))
        {
            ikController.MoveLeftHand(motionType, leftHandRaise.position, positionSpeed: armMoveSpeed, height: 0.1f);
            return;
        }
        raiseTimer = 0.0f;

        ikController.MoveLeftHand(motionType, medicineHandPositions[currentMedicineIndex].position, positionSpeed: armMoveSpeed, height: 0.1f);

        eyeAR = Instantiate(eyeARPrefab, medicineBottles[currentMedicineIndex].transform.position + new Vector3(0f, eyeAROffset, 0f), eyeARPrefab.transform.rotation);
        eyeAR.transform.SetParent(medicineBottles[currentMedicineIndex].transform);

        currentState = CutsceneState.GoingToMedicine;
    }

    private void GoingToMedicine()
    {
        // ikController.LookAtTarget(medicineHeadPositions[randomMedicine].position, headTurnSpeed);

        // if (ikController.IsLeftHandReached(medicineHandPositions[randomMedicine].position))
        // {
        //     currentState = CutsceneState.WaitingAtMedicine;
        // }

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

        // medicineBottles[randomMedicine].transform.SetParent(ikController.GetLeftHandIKTarget());   
        medicineBottles[currentMedicineIndex].transform.SetParent(ikController.GetLeftHandIKTarget());

        // // Move back to home
        // ikController.LookAtTarget(medicineHeadPositions[randomMedicine].position, headTurnSpeed);
        // ikController.MoveLeftHand(motionType, leftHandHome.position, positionSpeed: armMoveSpeed, height: 0.1f);

        // Move back to home
        ikController.LookAtTarget(medicineHeadPositions[currentMedicineIndex].position, headTurnSpeed);
        ikController.MoveLeftHand(motionType, leftHandHome.position, positionSpeed: armMoveSpeed, height: 0.1f);

        currentState = CutsceneState.GoingToHome;
    }

    private void GoingToHome(float headSpeed)
    {
        // ikController.LookAtTarget(medicineHeadPositions[randomMedicine].position,headTurnSpeed);
        ikController.LookAtTarget(medicineHeadPositions[currentMedicineIndex].position, headTurnSpeed);

        if (ikController.IsLeftHandReached(leftHandHome.position))
        {
            ikController.OpenLeftHand();
            if (ikController.GetLeftHandIKTarget().childCount > 0)
            {
                Transform child = ikController.GetLeftHandIKTarget().GetChild(1);
                child.SetParent(null);
            }

            // medicineHeadPositions.RemoveAt(randomMedicine);
            // medicineHandPositions.RemoveAt(randomMedicine);
            // medicineBottles.RemoveAt(randomMedicine);
            
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