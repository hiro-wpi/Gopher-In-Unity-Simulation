using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GopherController : MonoBehaviour
{
    [SerializeField] private ArmController leftArm;
    [SerializeField] private ArmController rightArm;
    [SerializeField] private BaseController baseController;

    // Image elements to be controlled
    [SerializeField] private Image leftArmImage;
    [SerializeField] private Image rightArmImage;
    [SerializeField] private Image baseImage;

    // Enum to select current control mode
    private enum ControlMode
    {
        LeftArm,
        RightArm,
        Base
    }

    // Create getter and setter for control mode
    [SerializeField]
    private ControlMode _controlMode = ControlMode.LeftArm;

    private void Start()
    {
        // Set initial control mode
        controlMode = _controlMode;
    }
    
    private ControlMode controlMode
    {
        get => _controlMode;
        set
        {
            _controlMode = value;
            ResetImages();
            switch (value)
            {
                case ControlMode.LeftArm:
                    leftArmImage.color = Color.green;
                    break;
                case ControlMode.RightArm:
                    rightArmImage.color = Color.green;
                    break;
                case ControlMode.Base:
                    baseImage.color = Color.green;
                    break;
                default:
                    break;
            }
        }
    }
    
    private void ResetImages()
    {
        leftArmImage.color = Color.white;
        rightArmImage.color = Color.white;
        baseImage.color = Color.white;
    }

    public void OnLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            controlMode = ControlMode.LeftArm;
        }
    }

    public void OnRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            controlMode = ControlMode.RightArm;
        }
    }

    public void OnBase(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            controlMode = ControlMode.Base;
        }
    }

    public void OnArmMove(InputAction.CallbackContext context)
    {
        switch (controlMode)
        {
            case ControlMode.LeftArm:
                leftArm.OnMove(context);
                break;
            case ControlMode.RightArm:
                rightArm.OnMove(context);
                break;
            case ControlMode.Base:
            default:
                break;
        }
    }

    public void OnArmRotate(InputAction.CallbackContext context)
    {
        switch (controlMode)
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

    public void OnArmGrip(InputAction.CallbackContext context)
    {
        switch (controlMode)
        {
            case ControlMode.LeftArm:
                leftArm.OnGrip(context);
                break;
            case ControlMode.RightArm:
                rightArm.OnGrip(context);
                break;
            case ControlMode.Base:
            default:
                break;
        }
    }

    public void OnArmTarget(InputAction.CallbackContext context)
    {
        switch (controlMode)
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

    public void OnBaseDrive(InputAction.CallbackContext context)
    {
        if (controlMode == ControlMode.Base)
        {
            baseController.OnDrive(context);
        }
    }
}