using UnityEngine;
using UnityEngine.InputSystem;

public class GopherController : MonoBehaviour
{
    [SerializeField] private ArmController leftArm;
    [SerializeField] private ArmController rightArm;
    [SerializeField] private BaseController baseController;
    
    // Enum to select current control mode
    private enum ControlMode
    {
        LeftArm,
        RightArm,
        Base
    }
    
    [SerializeField]
    private ControlMode controlMode = ControlMode.LeftArm;

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
    
    public void OnBaseDrive(InputAction.CallbackContext context)
    {
        if (controlMode == ControlMode.Base)
        {
            baseController.OnDrive(context);
        }
    }
}
