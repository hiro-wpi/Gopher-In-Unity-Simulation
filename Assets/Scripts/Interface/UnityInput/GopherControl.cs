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
public class GopherControl : MonoBehaviour
{
    // Unity input & action maps
    // Enable / Disable to receive input or not
    [SerializeField] private PlayerInput playerInput;
    private InputActionMap baseInputMap;
    private InputActionMap chestInputMap;
    private InputActionMap leftArmInputMap;
    private InputActionMap rightArmInputMap;
    private InputActionMap cameraInputMap;
    private InputActionMap[] actionMaps;

    // Available control modes
    public enum Mode { Base, Chest, LeftArm, RightArm }
    // current control mode
    [field: SerializeField]
    public Mode ControlMode { get; private set; } = Mode.Base;
    [field: SerializeField]
    public bool MainCameraEnabled { get; private set; } = false;

    void Start()
    {
        // Set up input action maps
        baseInputMap = playerInput.actions.FindActionMap("GopherBase");
        chestInputMap = playerInput.actions.FindActionMap("GopherChest");
        leftArmInputMap = playerInput.actions.FindActionMap("GopherLeftArm");
        rightArmInputMap = playerInput.actions.FindActionMap("GopherRightArm");
        cameraInputMap = playerInput.actions.FindActionMap("GopherCamera");
        // store it the same as Mode for easy enable/disable later
        actionMaps = new InputActionMap[] {
            baseInputMap, chestInputMap, leftArmInputMap, rightArmInputMap
        };

        // Default to base mode
        SetMode(Mode.Base);
    }

    // Setting Mode
    public void SetMode(Mode mode)
    {
        ControlMode = mode;
        SetActionMap(ControlMode);
    }

    private void SetActionMap(Mode mode)
    {
        // Only one mode can be active at a time
        // Disable all action maps and enable the selected one
        foreach (InputActionMap map in actionMaps)
        {
            map.Disable();
        }
        actionMaps[(int)mode].Enable();
    }

    public void ChangeMainCameraActive(bool active)
    {
        MainCameraEnabled = active;
        if (active)
        {
            cameraInputMap.Enable();
        }
        else
        {
            cameraInputMap.Disable();
        }
    }

    // Control Mode - switch to mode
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
            ChangeMainCameraActive(!MainCameraEnabled);
        }
    }
}
