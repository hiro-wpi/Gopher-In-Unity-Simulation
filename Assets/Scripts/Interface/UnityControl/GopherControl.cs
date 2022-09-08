using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GopherControl : MonoBehaviour
{ 
    public ArmControl leftArm;
    public ArmControl rightArm;
    public BaseControl baseControl;
    public CameraControl mainCameraControl;
    
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


    // Unity does not allow same key for different actions
    public void OnTranslate(InputAction.CallbackContext context)
    {
        if (_mode == ControlMode.Base)
        {
            OnBaseDrive(context);
        }
        if (_mode == ControlMode.LeftArm || 
            _mode == ControlMode.RightArm)
        {
            OnArmTranslate(context);
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

    public void OnBaseTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_mode == ControlMode.Base)
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
            if(_mode == ControlMode.LeftArm)
                leftArm.OnHome(context);
            else if (_mode == ControlMode.RightArm)
                rightArm.OnHome(context);
        }
    }
    public void OnArmPreset1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(_mode == ControlMode.LeftArm)
                leftArm.OnPreset1(context);
            else if (_mode == ControlMode.RightArm)
                rightArm.OnPreset1(context);
        }
    }
    public void OnArmPreset2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(_mode == ControlMode.LeftArm)
                leftArm.OnPreset2(context);
            else if (_mode == ControlMode.RightArm)
                rightArm.OnPreset2(context);
        }
    }
    public void OnArmPreset3(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(_mode == ControlMode.LeftArm)
                leftArm.OnPreset3(context);
            else if (_mode == ControlMode.RightArm)
                rightArm.OnPreset3(context);
        }
    }
    public void OnArmPreset4(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(_mode == ControlMode.LeftArm)
                leftArm.OnPreset4(context);
            else if (_mode == ControlMode.RightArm)
                rightArm.OnPreset4(context);
        }
    }
    public void OnArmPreset5(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(_mode == ControlMode.LeftArm)
                leftArm.OnPreset5(context);
            else if (_mode == ControlMode.RightArm)
                rightArm.OnPreset5(context);
        }
    }


    // ARM IK
    public void OnArmTranslate(InputAction.CallbackContext context)
    {
        if(_mode == ControlMode.LeftArm)
            leftArm.OnTranslate(context);
        else if (_mode == ControlMode.RightArm)
            rightArm.OnTranslate(context);
    }

    public void OnArmRotate(InputAction.CallbackContext context)
    {
        if(_mode == ControlMode.LeftArm)
            leftArm.OnRotate(context);
        else if (_mode == ControlMode.RightArm)
            rightArm.OnRotate(context);
    }

    public void OnArmTarget(InputAction.CallbackContext context)
    {
        if(_mode == ControlMode.LeftArm)
            leftArm.OnTarget(context);
        else if (_mode == ControlMode.RightArm)
            rightArm.OnTarget(context);
    }

    public void OnArmGrasp(InputAction.CallbackContext context)
    {
        if(_mode == ControlMode.LeftArm)
            leftArm.OnGrasp(context);
        else if (_mode == ControlMode.RightArm)
            rightArm.OnGrasp(context);
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
