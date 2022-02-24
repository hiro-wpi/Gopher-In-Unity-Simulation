using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics;

/// <summary>
///     This script detects mouse position
///     use it to control the active camera
/// </summary>
public class KeyboardJointControl : MonoBehaviour
{
    // Robot
    public GameObject jointRoot;
    public GameObject endEffector;
    private ArticulationBody[] articulationChain;
    private float[] currJointAngles;
    private int jointLength;

    // Controllers
    public ArticulationGripperController gripperController;
    public ArticulationJointController jointController;
    public CCDIK iK;

    // Control modes
    private enum ControlMode { JointControl=0, PositionControl=1, RotationControl=2 };
    private ControlMode controlMode;
    
    public float jointSpeed = 0.5f;
    public float linearSpeed = 0.2f;
    public float angularSpeed = 0.15f;

    // Joint Control
    private int previousIndex;
    private int selectedIndex;
    private Color[] prevColor;
    private Color highLightColor;
    private float currSpeed;

    // Position Control
    private Vector3 deltaPosition;
    private Quaternion prevRotation;

    // Rotation Control
    private Vector3 deltaRotation;
    private Vector3 prevPosition;


    void Start()
    {
        // Get joints
        articulationChain = jointRoot.GetComponentsInChildren<ArticulationBody>();
        articulationChain = articulationChain.Where(joint => joint.jointType 
                                                    != ArticulationJointType.FixedJoint).ToArray();
        jointLength = iK.numJoint;
        currJointAngles = new float[jointLength];

        // Default control mode - Joint Control
        controlMode = ControlMode.JointControl;

        // Initialize Joint Control
        previousIndex = 0;
        StoreJointColors(previousIndex);
        selectedIndex = 0;
        currSpeed = 0;
        highLightColor = new Color(1.0f, 0, 0, 1.0f);
        Highlight(selectedIndex);

        // Initialize Position and Rotation Control
        deltaRotation = Vector3.zero;
        deltaPosition = Vector3.zero;
    }

    void FixedUpdate()
    {
        // Joint control
        if (controlMode == ControlMode.JointControl)
        {
            jointController.SetJointSpeedStep(selectedIndex, currSpeed);
        }

        // Position control
        else if (controlMode == ControlMode.PositionControl)
        {
            if (deltaPosition != Vector3.zero)
            {
                float[] newJoints = SolveIK(deltaPosition, Vector3.zero);
                MoveJoints(newJoints);
            }
        }

        // Rotation control
        else if (controlMode == ControlMode.RotationControl)
        {
            if (deltaRotation != Vector3.zero)
            {
                float[] newJoints = SolveIK(Vector3.zero, deltaRotation);
                MoveJoints(newJoints);
            }
        }
    }

    void Update()
    {
        // Home all joints
        if (Input.GetKeyDown(KeyCode.H))
            jointController.HomeJoints();

        // Gripper
        if (Input.GetKeyDown(KeyCode.G))
            gripperController.CloseGrippers();
        else if (Input.GetKeyDown(KeyCode.R))
            gripperController.OpenGrippers();

        // Switch control mode
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            SwitchMode((int)controlMode - 1);
        }
        else if (Input.GetKeyDown(KeyCode.Period))
        {
            SwitchMode((int)controlMode + 1);
        }

        // Joint control
        if (controlMode == ControlMode.JointControl)
        {
            // switch joint
            if (Input.GetKeyDown(KeyCode.J))
            {
                SetSelectedJointIndex(selectedIndex - 1);
                Highlight(selectedIndex);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                SetSelectedJointIndex(selectedIndex + 1);
                Highlight(selectedIndex);
            }
            // control
            if (Input.GetKey(KeyCode.K))
            {
                currSpeed = -jointSpeed;
            }
            else if (Input.GetKey(KeyCode.I))
            {
                currSpeed = jointSpeed;
            }
            else
            {
                currSpeed = 0f;
            }
        }

        // Position control
        else if (controlMode == ControlMode.PositionControl)
        {
            deltaPosition = Vector3.zero;
            float delta = linearSpeed * 0.05f; // 0.05 sample size

            // x -> unity coordinate
            if (Input.GetKey(KeyCode.J))
            {
                deltaPosition += new Vector3(0, 0, delta);
            }
            else if (Input.GetKey(KeyCode.L))
            {
                deltaPosition += new Vector3(0, 0, -delta);
            }
            // y -> unity coordinate
            if (Input.GetKey(KeyCode.K))
            {
                deltaPosition += new Vector3(-delta, 0, 0);
            }
            else if (Input.GetKey(KeyCode.I))
            {
                deltaPosition += new Vector3(delta, 0, 0);
            }
            // z -> unity coordinate
            if (Input.GetKey(KeyCode.U))
            {
                deltaPosition += new Vector3(0, -delta, 0);
            }
            else if (Input.GetKey(KeyCode.O))
            {
                deltaPosition += new Vector3(0, delta, 0);
            }
        }

        // Rotation control
        else if (controlMode == ControlMode.RotationControl)
        {
            deltaRotation = Vector3.zero;
            float delta = angularSpeed*Mathf.Rad2Deg * 0.5f; // 0.5 sample rate
            
            // x -> unity coordinate
            if (Input.GetKey(KeyCode.J))
            {
                deltaRotation += new Vector3(0, 0, -delta);
            }
            else if (Input.GetKey(KeyCode.L))
            {
                deltaRotation += new Vector3(0, 0, delta);
            }
            // y -> unity coordinate
            if (Input.GetKey(KeyCode.K))
            {
                deltaRotation += new Vector3(delta, 0, 0);
            }
            else if (Input.GetKey(KeyCode.I))
            {
                deltaRotation += new Vector3(-delta, 0, 0);
            }
            // z -> unity coordinate
            if (Input.GetKey(KeyCode.U))
            {
                deltaRotation += new Vector3(0, -delta, 0);
            }
            else if (Input.GetKey(KeyCode.O))
            {
                deltaRotation += new Vector3(0, delta, 0);
            }
        }
    }

    private void SwitchMode(int controlModeC)
    {
        // Switch to mode controlModeC
        int numMode = Enum.GetNames(typeof(ControlMode)).Length;
        int modeIndex = (controlModeC + numMode) % numMode;
        controlMode = (ControlMode)modeIndex;

        // Change color for entering and leaving joint control mode
        if (controlMode == ControlMode.JointControl)
        {
            Highlight(selectedIndex);
        }
        else if (controlMode == ControlMode.PositionControl)
        {
            ResetJointColors(selectedIndex);
            prevRotation = endEffector.transform.rotation;
        }
        else if (controlMode == ControlMode.RotationControl)
        {
            ResetJointColors(selectedIndex);
            prevPosition = endEffector.transform.position;
        }
        
        // Set delta to 0
        if (controlMode == ControlMode.PositionControl)
            deltaRotation = Vector3.zero;
        else if (controlMode == ControlMode.RotationControl)
            deltaPosition = Vector3.zero;

        Debug.Log("Switch to mode: " + controlMode);
    }

    private float[] SolveIK(Vector3 deltaPosition, Vector3 deltaRotation)
    {
        // Target position and rotation   
        Vector3 position = jointRoot.transform.InverseTransformPoint(
                                               endEffector.transform.position) + 
                           deltaPosition;
        Vector3 rotation = (Quaternion.Inverse(jointRoot.transform.rotation) *
                            endEffector.transform.rotation).eulerAngles + 
                           deltaRotation;

        if (deltaPosition == Vector3.zero)
        {
            position = jointRoot.transform.InverseTransformPoint(
                                           prevPosition);
        }
        if (deltaRotation == Vector3.zero)   
        {   
            rotation = (Quaternion.Inverse(jointRoot.transform.rotation) *
                        prevRotation).eulerAngles;
        }
        
        // Solve IK
        // get current joints
        for (int i = 0; i < jointLength; ++i)
            // currJointAngles[i] = articulationChain[i].jointPosition[0]; 
            // Use drive target instead of exact joint position
            // to avoid unintended oscillation due to controller's static error
            currJointAngles[i] = articulationChain[i].xDrive.target * Mathf.Deg2Rad;       
        iK.SetJointAngle(currJointAngles);

        // set target
        iK.SetTarget(position, Quaternion.Euler(rotation.x, rotation.y, rotation.z));
        
        // solve
        (float[] resultJointAngles, bool foundSolution) = iK.CCD();

        return resultJointAngles;
    }

    private void MoveJoints(float[] joints)
    {
        for (int i=0; i < joints.Length; ++i)
        {
            jointController.SetJointTarget(i, joints[i]);
        }
    }


    // The following codes are from Controller.cs of URDF-Importer //

    private void Highlight(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= articulationChain.Length) 
        {
            return;
        }

        // reset colors for the previously selected joint
        ResetJointColors(previousIndex);

        // store colors for the current selected joint
        StoreJointColors(selectedIndex);
        previousIndex = selectedIndex;

        // DisplaySelectedJoint(selectedIndex);
        Renderer[] rendererList = articulationChain[selectedIndex].transform.GetChild(0).
                                                    GetComponentsInChildren<Renderer>();

        // set the color of the selected join meshes to the highlight color
        foreach (var mesh in rendererList)
        {
            MaterialExtensions.SetMaterialColor(mesh.material, highLightColor);
        }
    }
    private void StoreJointColors(int index)
    {
        Renderer[] materialLists = articulationChain[index].transform.GetChild(0).
                                                    GetComponentsInChildren<Renderer>();
        prevColor = new Color[materialLists.Length];
        for (int counter = 0; counter < materialLists.Length; counter++)
        {
            prevColor[counter] = MaterialExtensions.GetMaterialColor(materialLists[counter]);
        }
    }
    private void ResetJointColors(int index)
    {
        Renderer[] previousRendererList = articulationChain[index].transform.GetChild(0).
                                                            GetComponentsInChildren<Renderer>();
        for (int counter = 0; counter < previousRendererList.Length; counter++)
        {
            MaterialExtensions.SetMaterialColor(previousRendererList[counter].material, 
                                                prevColor[counter]);
        }
    }
    private void SetSelectedJointIndex(int index)
    {
        if (articulationChain.Length > 0) 
        {
            selectedIndex = (index + jointLength) % jointLength;
        }
    }
}
