using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using Unity.Netcode;

/// <summary>
///    Synchronize input actions by sending the values to the server
///    
///    Notify that the actions are not actually synchronized.
///    Only the values of the actions are sent to the server.
/// </summary>
public class NetworkInputActions : NetworkBehaviour
{
    [SerializeField] private InputActionMap actionMap;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            actionMap.Enable();
        }
        else
        {
            actionMap.Disable();
        }
    }

    void Update()
    { 
        if (IsOwner && !IsServer)
        {
            SendInputDataServerRpc(SerializeActionMapState());
        }
    }

    [System.Serializable]
    public struct ButtonInputData
    {
        public bool IsPressed;
        public bool PressedThisFrame;
        public bool ReleasedThisFrame;
    }

    private string SerializeActionMapState()
    {
        var input = new Dictionary<string, object>();
        foreach (var action in actionMap)
        {
            if (action.type == InputActionType.Button)
            {
                input[action.name] = new ButtonInputData
                {
                    IsPressed = action.IsPressed(),
                    PressedThisFrame = action.WasPressedThisFrame(),
                    ReleasedThisFrame = action.WasReleasedThisFrame()
                };
            }
            else if (action.type == InputActionType.Value)
            {
                input[action.name] = action.ReadValueAsObject();
            }
        }
        return JsonUtility.ToJson(input);
    }

    [ServerRpc]
    private void SendInputDataServerRpc(string serializedState)
    {
        ProcessInputDataOnServer(serializedState);
    }

    private void ProcessInputDataOnServer(string serializedState)
    {
        var state = JsonUtility.FromJson<Dictionary<string, object>>(
            serializedState
        );

        foreach (var action in actionMap)
        {
            if (state.ContainsKey(action.name))
            {
                Debug.Log(action.name);
                if (action.type == InputActionType.Button)
                {
                    var buttonData = JsonUtility.FromJson<ButtonInputData>(
                        state[action.name].ToString()
                    );
                    Debug.Log("Button");
                    Debug.Log(buttonData.IsPressed);
                    Debug.Log(buttonData.PressedThisFrame);
                    Debug.Log(buttonData.ReleasedThisFrame);
                }
                else if (action.type == InputActionType.Value)
                {
                    Debug.Log("Value");
                    Debug.Log(state[action.name]);
                }
            }
        }
    }
}
