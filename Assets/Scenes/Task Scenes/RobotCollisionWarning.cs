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

        if (foreArmCollisionDetection != null && handCollisionDetection != null && smallForeArmCollisionDetection != null && smallHandCollisionDetection != null)
        {
            if (foreArmCollisionDetection.onRobotCollision || handCollisionDetection.onRobotCollision) 
            {
                if (smallForeArmCollisionDetection.onRobotCollision || smallHandCollisionDetection.onRobotCollision)
                {
                    HideWarningGameObject();
                    warningUI.SetActive(false);
                    ShowCollisionGameObject();
                    collisionUI.SetActive(true);
                }

                else
                {
                    ShowWarningGameObject();
                    warningUI.SetActive(true);
                    HideCollisionGameObject();
                    collisionUI.SetActive(false);
                }
            }
            
            else
            {
                HideWarningGameObject();
                warningUI.SetActive(false);
                HideCollisionGameObject();
                collisionUI.SetActive(false);
            } 
        }
    }

    private void CreateWarningGameObject(Transform parent)
    {
        warningIconObject = new GameObject("Warning Icon Object");
        
        GameObject warningSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        warningSphere.name = "Warning Sphere";

        warningSphere.GetComponent<MeshRenderer>().enabled = false;
        warningSphere.GetComponent<Collider>().enabled = false;

        warningSphere.transform.parent = warningIconObject.transform;
        warningSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        warningIconObject.transform.parent = parent;
        warningIconObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        var warningIconLocation = HighlightObjectOnCanvas.ElementPosition.Center;
        highlightObject.Highlight(warningSphere, cam, displayRect, warningIcon, Color.yellow, adjustUIScale: false, position: warningIconLocation);

        List<GameObject> arWarningList = highlightObject.GetHighlightGameObject(warningSphere);

        arWarningList[0].name = "Warning Icon";

        arWarningList[0].SetActive(false);

        warningIconCanvas = arWarningList[0];
    }

    private void ShowWarningGameObject()
    {
        warningIconCanvas.SetActive(true);
    }

    private void HideWarningGameObject()
    {
        warningIconCanvas.SetActive(false);
    }

    private void CreateCollisionGameObject(Transform parent)
    {
        collisionIconObject = new GameObject("Collision Icon Object");
        
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Collision Sphere";

        sphere.GetComponent<MeshRenderer>().enabled = false;
        sphere.GetComponent<Collider>().enabled = false;

        sphere.transform.parent = collisionIconObject.transform;
        sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        collisionIconObject.transform.parent = parent;
        collisionIconObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        var collisionIconLocation = HighlightObjectOnCanvas.ElementPosition.Center;
        highlightObject.Highlight(sphere, cam, displayRect, collisionIcon, Color.red, adjustUIScale: false, position: collisionIconLocation);
        List<GameObject> arList = highlightObject.GetHighlightGameObject(sphere);

        arList[0].name = "Collision Icon";
        arList[0].SetActive(false);

        collisionIconCanvas = arList[0];
    }

    private void ShowCollisionGameObject()
    {
        collisionIconCanvas.SetActive(true);
    }

    private void HideCollisionGameObject()
    {
        collisionIconCanvas.SetActive(false);
    }
}

