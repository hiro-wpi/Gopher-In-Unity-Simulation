using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using Unity.Netcode;

/// <summary>
///    Synchronize input actions across network
/// </summary>
public class NetworkInputActions : NetworkBehaviour
{
    [SerializeField] private InputActionMap actionMap;

    private void Update()
    { 
        if (IsOwner && !IsServer)
        {
            SendInputDataToServer(SerializeActionMapState());
        }
    }

    private string SerializeActionMapState()
    {
        var input = new Dictionary<string, object>();
        foreach (var action in actionMap)
        {
            if (action.type == InputActionType.Button)
            {
                input[action.name] = action.triggered;
            }
            else if (action.type == InputActionType.Value)
            {
                input[action.name] = action.ReadValueAsObject();
            }
        }
        return JsonUtility.ToJson(input);
    }

    [ServerRpc]
    private void SendInputDataToServer(string serializedState)
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
                var value = state[action.name];
                if (action.type == InputActionType.Button && value is bool)
                {
                    
                }
                else if (action.type == InputActionType.Value)
                {
                    
                }
            }
        }
    }
}
