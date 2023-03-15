using System;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
///     This script handles Unity input for Gopher control.
///     For now, only one component of the Gopher is controlled 
///     at a time.
/// </summary>
public class GopherControl : MonoBehaviour
{
    // Children control handlers
    [SerializeField] private ArmControl leftArmControl;
    [SerializeField] private ArmControl rightArmControl;
    [SerializeField] private BaseControl baseControl;
    [SerializeField] private CameraControl mainCameraControl;

    // Available control modes
    public enum Mode { Base, LeftArm, RightArm }
    // current control mode
    [field: SerializeField] 
    public bool CameraControlEnabled { get; set; } = false;
    [field: SerializeField]
    public Mode ControlMode { get; private set; } = Mode.Base;
    
    void Start() { }

    // CONTROL MODE
    public void OnLeftArm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            baseControl.StopBase();
            rightArmControl.StopArm();
            ControlMode = Mode.LeftArm;
        }
    }

    public void OnRightArm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            baseControl.StopBase();
            leftArmControl.StopArm();
            ControlMode = Mode.RightArm;
        }
    }

    public void OnBase(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            leftArmControl.StopArm();
            rightArmControl.StopArm();
            ControlMode = Mode.Base;
        }
    }

    public void OnMainCamera(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CameraControlEnabled = !CameraControlEnabled;
            if (CameraControlEnabled)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.Confined;
        }
    }


    // TODO Split the action //
    // Unity does not allow same key for different actions
    public void OnTranslate(InputAction.CallbackContext context)
    {
        if (ControlMode == Mode.Base)
        {
            OnBaseDrive(context);
        }
        if (ControlMode == Mode.LeftArm || 
            ControlMode == Mode.RightArm)
        {
            OnArmTranslate(context);
        }
    }


    // BASE
    public void OnBaseDrive(InputAction.CallbackContext context)
    {
        if (ControlMode == Mode.Base)
        {
            baseControl.OnDrive(context);
        }
    }

    public void OnModeChange(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (ControlMode == Mode.Base)
            {
                baseControl.OnModeChange(context);
            }
            else if (ControlMode == Mode.LeftArm)
            {
                leftArmControl.OnModeChange(context);
            }
            else if (ControlMode == Mode.RightArm)
            {
                rightArmControl.OnModeChange(context);
            }
        }
    }

    public void OnBaseTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (ControlMode == Mode.Base)
            {
                baseControl.OnTarget(context);
            }
        }
    }


    // ARM PRESET
    public void OnArmHome(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(ControlMode == Mode.LeftArm)
                leftArmControl.OnHome(context);
            else if (ControlMode == Mode.RightArm)
                rightArmControl.OnHome(context);
        }
    }
    public void OnArmPreset1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(ControlMode == Mode.LeftArm)
                leftArmControl.OnPreset1(context);
            else if (ControlMode == Mode.RightArm)
                rightArmControl.OnPreset1(context);
        }
    }
    public void OnArmPreset2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(ControlMode == Mode.LeftArm)
                leftArmControl.OnPreset2(context);
            else if (ControlMode == Mode.RightArm)
                rightArmControl.OnPreset2(context);
        }
    }
    public void OnArmPreset3(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(ControlMode == Mode.LeftArm)
                leftArmControl.OnPreset3(context);
            else if (ControlMode == Mode.RightArm)
                rightArmControl.OnPreset3(context);
        }
    }
    public void OnArmPreset4(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(ControlMode == Mode.LeftArm)
                leftArmControl.OnPreset4(context);
            else if (ControlMode == Mode.RightArm)
                rightArmControl.OnPreset4(context);
        }
    }
    public void OnArmPreset5(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(ControlMode == Mode.LeftArm)
                leftArmControl.OnPreset5(context);
            else if (ControlMode == Mode.RightArm)
                rightArmControl.OnPreset5(context);
        }
    }


    // ARM IK
    public void OnArmTranslate(InputAction.CallbackContext context)
    {
        if(ControlMode == Mode.LeftArm)
            leftArmControl.OnTranslate(context);
        else if (ControlMode == Mode.RightArm)
            rightArmControl.OnTranslate(context);
    }

    public void OnArmRotate(InputAction.CallbackContext context)
    {
        if(ControlMode == Mode.LeftArm)
            leftArmControl.OnRotate(context);
        else if (ControlMode == Mode.RightArm)
            rightArmControl.OnRotate(context);
    }

    public void OnArmTarget(InputAction.CallbackContext context)
    {
        if(ControlMode == Mode.LeftArm)
            leftArmControl.OnTarget(context);
        else if (ControlMode == Mode.RightArm)
            rightArmControl.OnTarget(context);
    }

    public void OnArmGripper(InputAction.CallbackContext context)
    {
        if(ControlMode == Mode.LeftArm)
            leftArmControl.OnGripper(context);
        else if (ControlMode == Mode.RightArm)
            rightArmControl.OnGripper(context);
    }


    // CAMERA
    public void OnCameraRotate(InputAction.CallbackContext context)
    {
        if (CameraControlEnabled)
        {
            mainCameraControl.OnRotate(context);
        }
    }

    public void OnCameraCenter(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CameraControlEnabled)
                mainCameraControl.OnCenter(context);
        }
    }
}
