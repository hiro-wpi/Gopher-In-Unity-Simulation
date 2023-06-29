using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///     This script handles Unity input for Gopher.
///     It controls the enabling and disabling of
///     action maps and the control mode.
///
///     For now, only one component is controlled 
///     at a time when using keyboard.
/// </summary>
public class GopherControlManager : MonoBehaviour
{
    // Unity input & action maps
    // Enable / Disable to receive input or not
    [SerializeField] private PlayerInput playerInput;
    private InputActionMap baseInputMap;
    private InputActionMap chestInputMap;
    private InputActionMap leftArmInputMap;
    private InputActionMap rightArmInputMap;
    private InputActionMap cameraInputMap;
    private InputActionMap[] simultaneousActionMaps;

    // Available control modes
    public enum Mode { Base, Chest, LeftArm, RightArm }
    // current control mode
    [field: SerializeField]
    public Mode ControlMode { get; private set; } = Mode.Base;
    [field: SerializeField]
    public bool MainCameraEnabled { get; private set; } = false;

    void OnEnable()
    {
        // Set up input action maps
        InputActionAsset action = playerInput.actions;
        baseInputMap = action.FindActionMap("GopherBase");
        chestInputMap = action.FindActionMap("GopherChest");
        leftArmInputMap = action.FindActionMap("GopherLeftArm");
        rightArmInputMap = action.FindActionMap("GopherRightArm");
        cameraInputMap = action.FindActionMap("GopherCamera");
        // store it the same as Mode for easy enable/disable later
        simultaneousActionMaps = new InputActionMap[] {
            baseInputMap, chestInputMap, leftArmInputMap, rightArmInputMap
        };

        // Default to base mode
        SetMode(Mode.Base);
    }

    // Setting Mode
    public void SetMode(Mode mode)
    {
        ControlMode = mode;

        // Only one mode can be active at a time
        // Disable all action maps and enable the selected one
        foreach (InputActionMap map in simultaneousActionMaps)
        {
            map.Disable();
        }

        simultaneousActionMaps[(int)mode].Enable();
    }

    public void ChangeMainCameraActive()
    {
        MainCameraEnabled = !MainCameraEnabled;
        if (MainCameraEnabled)
        {
            cameraInputMap.Enable();
        }
        else
        {
            cameraInputMap.Disable();
        }
    }

    // Control Mode - Unity Input System
    public void OnBase(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SetMode(Mode.Base);
        }
    }

    public void OnChest(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SetMode(Mode.Chest);
        }
    }

    public void OnLeftArm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SetMode(Mode.LeftArm);
        }
    }

    public void OnRightArm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SetMode(Mode.RightArm);
        }
    }

    public void OnMainCamera(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ChangeMainCameraActive();
        }
    }
}
