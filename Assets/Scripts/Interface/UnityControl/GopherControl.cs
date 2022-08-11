using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GopherControl : MonoBehaviour
{ 
    public ArmControl leftArm;
    public ArmControl rightArm;
    public BaseControl baseControl;
    public PanTiltCameraControl mainCameraControl;
    
    // Enum to select current control mode
    public enum ControlMode
    {
        Base,
        LeftArm,
        RightArm,
    }
    // Create getter and setter for control mode
    [SerializeField]
    private ControlMode _mode = ControlMode.Base;
    public ControlMode Mode { get{return _mode;} set{ _mode = value;} }
    public bool cameraControlEnabled = false;

    void Start()
    {}

    
    // CONTROL MODE
    public void OnLeftArm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            baseControl.StopBase();
            rightArm.StopArm();
            _mode = ControlMode.LeftArm;
        }
    }

    public void OnRightArm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            baseControl.StopBase();
            leftArm.StopArm();
            _mode = ControlMode.RightArm;
        }
    }

    public void OnBase(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            leftArm.StopArm();
            rightArm.StopArm();
            _mode = ControlMode.Base;
        }
    }

    public void OnMainCamera(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            cameraControlEnabled = !cameraControlEnabled;
            if (cameraControlEnabled)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.Confined;
        }
    }


    // BASE
    public void OnBaseDrive(InputAction.CallbackContext context)
    {
        if (_mode == ControlMode.Base)
        {
            baseControl.OnDrive(context);
        }
    }


    // ARM PRESET
    public void OnArmPreset1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (_mode)
            {
                case ControlMode.LeftArm:
                    leftArm.OnPreset1(context);
                    break;
                case ControlMode.RightArm:
                    rightArm.OnPreset1(context);
                    break;
                case ControlMode.Base:
                default:
                    break;
            }
        }
    }
    public void OnArmPreset2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (_mode)
            {
                case ControlMode.LeftArm:
                    leftArm.OnPreset2(context);
                    break;
                case ControlMode.RightArm:
                    rightArm.OnPreset2(context);
                    break;
                case ControlMode.Base:
                default:
                    break;
            }
        }
    }
    public void OnArmPreset3(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (_mode)
            {
                case ControlMode.LeftArm:
                    leftArm.OnPreset3(context);
                    break;
                case ControlMode.RightArm:
                    rightArm.OnPreset3(context);
                    break;
                case ControlMode.Base:
                default:
                    break;
            }
        }
    }
    public void OnArmPreset4(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (_mode)
            {
                case ControlMode.LeftArm:
                    leftArm.OnPreset4(context);
                    break;
                case ControlMode.RightArm:
                    rightArm.OnPreset4(context);
                    break;
                case ControlMode.Base:
                default:
                    break;
            }
        }
    }
    public void OnArmPreset5(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (_mode)
            {
                case ControlMode.LeftArm:
                    leftArm.OnPreset5(context);
                    break;
                case ControlMode.RightArm:
                    rightArm.OnPreset5(context);
                    break;
                case ControlMode.Base:
                default:
                    break;
            }
        }
    }


    // ARM IK
    public void OnArmTranslate(InputAction.CallbackContext context)
    {
        switch (_mode)
        {
            case ControlMode.LeftArm:
                leftArm.OnTranslate(context);
                break;
            case ControlMode.RightArm:
                rightArm.OnTranslate(context);
                break;
            case ControlMode.Base:
            default:
                break;
        }
    }

    // TODO Input key positions don't really match "up down left right forward backward"
    // Something may be wrong with the coordinate in NewtonIK?
    public void OnArmRotate(InputAction.CallbackContext context)
    {
        switch (_mode)
        {
            case ControlMode.LeftArm:
                leftArm.OnRotate(context);
                break;
            case ControlMode.RightArm:
                rightArm.OnRotate(context);
                break;
            case ControlMode.Base:
            default:
                break;
        }
    }

    public void OnArmTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (_mode)
            {
                case ControlMode.LeftArm:
                    leftArm.OnTarget(context);
                    break;
                case ControlMode.RightArm:
                    rightArm.OnTarget(context);
                    break;
                case ControlMode.Base:
                default:
                    break;
            }
        }
    }

    public void OnArmGrasp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (_mode)
            {
                case ControlMode.LeftArm:
                    leftArm.OnGrasp(context);
                    break;
                case ControlMode.RightArm:
                    rightArm.OnGrasp(context);
                    break;
                case ControlMode.Base:
                default:
                    break;
            }
        }
    }


    // CAMERA
    public void OnCameraRotate(InputAction.CallbackContext context)
    {
        if (cameraControlEnabled)
        {
            mainCameraControl.OnRotate(context);
        }
    }

    public void OnCameraCenter(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (cameraControlEnabled)
                mainCameraControl.OnCenter(context);
        }
    }
}