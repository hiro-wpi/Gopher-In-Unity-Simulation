using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
//      This script is used to manage different
//      control schemes/interfaces in the Unity Input.
/// </summary>
public class ControlInterface : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    void Start() {}

    void Update() {}

    public void SetControlScheme(string controlScheme)
    {
        playerInput.SwitchCurrentControlScheme(
            controlScheme, InputSystem.devices[0]
        );
    }
}
