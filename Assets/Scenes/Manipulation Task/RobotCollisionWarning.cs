using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotCollisionWarning : MonoBehaviour
{
    [SerializeField] private GameObject nurseSitting;
    [SerializeField] private RobotCollisionDetection foreArmCollisionDetection;
    [SerializeField] private RobotCollisionDetection handCollisionDetection;
    [SerializeField] private RobotCollisionDetection smallForeArmCollisionDetection;
    [SerializeField] private RobotCollisionDetection smallHandCollisionDetection;
    [SerializeField] private HighlightObjectOnCanvas highlightObject;
    [SerializeField] private GraphicalInterface graphicalInterface;
    [SerializeField] private RectTransform displayRect;
    private Camera cam;
    [SerializeField] private GameObject warningIconObject;
    [SerializeField] private Sprite warningIcon;
    private GameObject warningIconCanvas;
    [SerializeField] private GameObject collisionIconObject;
    [SerializeField] private Sprite collisionIcon;
    private GameObject collisionIconCanvas;
    private GameObject robotBracelet;
    [SerializeField] private GameObject warningUI;
    [SerializeField] private GameObject collisionUI;
    public int collisionCounter = 0;
    private bool wasInCollision = false;  // New variable to track previous collision state

    private void Update()
    {
        if (cam == null)
        {
            Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();
            
            if (cameras.Length > 0)
            {
                cam = cameras[0];
            }
        }   

        if (nurseSitting == null)
        {
            nurseSitting = GameObject.Find("Nurse Sitting");

            if (nurseSitting != null)
            {
                foreArmCollisionDetection = nurseSitting.transform.Find("Ch16_nonPBR/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm").GetComponent<RobotCollisionDetection>();
                handCollisionDetection = nurseSitting.transform.Find("Ch16_nonPBR/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand").GetComponent<RobotCollisionDetection>();
                smallForeArmCollisionDetection = nurseSitting.transform.Find("Ch16_nonPBR/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/CollisionLeftForeArm").GetComponent<RobotCollisionDetection>();
                smallHandCollisionDetection = nurseSitting.transform.Find("Ch16_nonPBR/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand/CollisionLeftHand").GetComponent<RobotCollisionDetection>();
            }
        }

        if (robotBracelet == null)
        {
            robotBracelet = GameObject.Find("gopher/right_arm_bracelet_link");

            if (robotBracelet != null)
            {
                CreateWarningGameObject(robotBracelet.transform);
                CreateCollisionGameObject(robotBracelet.transform);
            }

            else
            {
                return;
            }

        }

        // if (foreArmCollisionDetection != null && handCollisionDetection != null && smallForeArmCollisionDetection != null && smallHandCollisionDetection != null)
        // {
        //     if (foreArmCollisionDetection.onRobotCollision || handCollisionDetection.onRobotCollision) 
        //     {
        //         if (smallForeArmCollisionDetection.onRobotCollision || smallHandCollisionDetection.onRobotCollision)
        //         {
        //             // Hide the warning
        //             HideWarningGameObject();
        //             warningUI.SetActive(false);

        //             // Show the collision
        //             ShowCollisionGameObject();
        //             collisionUI.SetActive(true);
                    
        //             // Increase the collision counter
        //             collisionCounter++;
        //         }

        //         else
        //         {
        //             // Show the warning
        //             ShowWarningGameObject();
        //             warningUI.SetActive(true);

        //             // Hide the collision
        //             HideCollisionGameObject();
        //             collisionUI.SetActive(false);
        //         }
        //     }
            
        //     else
        //     {
        //         // Hide the warning
        //         HideWarningGameObject();
        //         warningUI.SetActive(false);

        //         // Hide the collision
        //         HideCollisionGameObject();
        //         collisionUI.SetActive(false);
        //     } 
        // }

        if (foreArmCollisionDetection != null && handCollisionDetection != null && smallForeArmCollisionDetection != null && smallHandCollisionDetection != null)
        {
            bool isInCollision = foreArmCollisionDetection.onRobotCollision || handCollisionDetection.onRobotCollision;

            if (isInCollision)
            {
                if (smallForeArmCollisionDetection.onRobotCollision || smallHandCollisionDetection.onRobotCollision)
                {
                    if (!wasInCollision)  // Check if entering the collision state
                    {
                        // Hide the warning
                        HideWarningGameObject();
                        warningUI.SetActive(false);

                        // Show the collision
                        ShowCollisionGameObject();
                        collisionUI.SetActive(true);

                        // Increase the collision counter
                        collisionCounter++;
                    }
                }
                else
                {
                    // Show the warning
                    ShowWarningGameObject();
                    warningUI.SetActive(true);

                    // Hide the collision
                    HideCollisionGameObject();
                    collisionUI.SetActive(false);
                }
            }
            else
            {
                if (wasInCollision)  // Check if leaving the collision state
                {
                    // Hide the warning
                    HideWarningGameObject();
                    warningUI.SetActive(false);

                    // Hide the collision
                    HideCollisionGameObject();
                    collisionUI.SetActive(false);
                }
            }

            wasInCollision = isInCollision;  // Update the previous collision state
        }       
    }

    private void CreateWarningGameObject(Transform parent)
    {
        warningIconObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        warningIconObject.name = "Warning Icon Object";

        warningIconObject.GetComponent<MeshRenderer>().enabled = false;
        warningIconObject.GetComponent<Collider>().enabled = false;

        warningIconObject.transform.parent = parent;
        warningIconObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        warningIconObject.transform.localRotation = Quaternion.identity;
        warningIconObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    private void ShowWarningGameObject()
    {
        highlightObject.RemoveHighlight(warningIconObject);

        var warningIconLocation = HighlightObjectOnCanvas.ElementPosition.Center;
        highlightObject.Highlight(warningIconObject, cam, displayRect, warningIcon, Color.yellow, adjustUIScale: false, position: warningIconLocation);
    }

    private void HideWarningGameObject()
    {
        highlightObject.RemoveHighlight(warningIconObject);
    }

    private void CreateCollisionGameObject(Transform parent)
    {
        collisionIconObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        collisionIconObject.name = "Collision Icon Object";

        collisionIconObject.GetComponent<MeshRenderer>().enabled = false;
        collisionIconObject.GetComponent<Collider>().enabled = false;

        collisionIconObject.transform.parent = parent;
        collisionIconObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        collisionIconObject.transform.localRotation = Quaternion.identity;
        collisionIconObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    private void ShowCollisionGameObject()
    {
        highlightObject.RemoveHighlight(collisionIconObject);

        var collisionIconLocation = HighlightObjectOnCanvas.ElementPosition.Center;
        highlightObject.Highlight(collisionIconObject, cam, displayRect, collisionIcon, Color.red, adjustUIScale: false, position: collisionIconLocation);
    }

    private void HideCollisionGameObject()
    {
        highlightObject.RemoveHighlight(collisionIconObject);
    }
}