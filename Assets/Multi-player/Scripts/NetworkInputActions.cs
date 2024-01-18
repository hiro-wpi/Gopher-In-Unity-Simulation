using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Newtonsoft.Json;

/// <summary>
///    Synchronize input actions by sending the values to the server
///    
///    Note that the actions are not actually synchronized.
///    Only the values of the actions are sent to the server.
///    
///    For the server, the input actions and related values (string)
///    can be acquired by public function GetInputActionsState().
///    
///    Note that the received values are "string" type,
///    and they are not yet converted to the actual types (they can be "null").
///    Use public function GetInputActionValueAsType<>() to convert them.
/// </summary>
public class NetworkInputActions : NetworkBehaviour
{
    [SerializeField] private InputActionAsset inputAction;

    private int numActions = 0;
    private InputAction[] actions = new InputAction[0];
    private string[] actionValues = new string[0];

    [System.Serializable]
    public struct ActionValue
    {
        // Shorten name for network serialization purpose
        public object V;  // Value
        public bool P;  // WasPressedThisFrame
        public bool R;  // WasReleasedThisFrame
    }

    // Getter for the server
    public (InputAction[], string[]) GetInputActionsState()
    {
        return (actions, actionValues);
    }

    public (T, bool, bool) GetInputActionValueAsType<T>(string actionValues)
    {
        var value = JsonConvert.DeserializeObject<ActionValue>(actionValues);
        if (value.V == null)
        {
            return (default(T), value.P, value.R);
        }

        // Direct casting is somehow not working
        // var val = (T)value.V;
        var val = JsonConvert.DeserializeObject<T>(
            JsonConvert.SerializeObject(value.V)
        );
        return (val, value.P, value.R);
    }

    public override void OnNetworkSpawn()
    {
        // Enable input actions only for the owner
        if (IsOwner)
        {
            inputAction.Enable();
        }
        else
        {
            inputAction.Disable();
        }

        // This is only needed if 
        // you are the owner but not the server (send data)
        // or you are the server but not the owner (receive data)
        if ((!IsServer && IsOwner) || (IsServer && !IsOwner))
        {
            numActions = inputAction.Count<InputAction>();
            actions = new InputAction[numActions];
            actionValues = new string[numActions];

            int i = 0;
            foreach (var action in inputAction)
            {
                actions[i] = action;
                actionValues[i] = null;
                i++;
            }
        }
    }

    void Update()
    {
        // Send data to server
        if (IsOwner && !IsServer)
        {
            SendInputDataServerRpc(SerializeInputActionsState());
        }
    }

    private string SerializeInputActionsState()
    {
        // Update action values
        int i = 0;
        foreach (var action in inputAction)
        {
            actionValues[i] = SerializeInputAction(action);
            i++;
        }

        return string.Join(";", actionValues);
    }

    private string SerializeInputAction(InputAction action)
    {
        ActionValue actionValue = new ()
        {
            V = action.ReadValueAsObject(),
            P = action.WasPressedThisFrame(),
            R = action.WasReleasedThisFrame()
        };

        // No action in this frame
        if (actionValue.V == null && !actionValue.P && !actionValue.R)
        {
            return "null";
        }

        // Serialize the value of the action directly
        return JsonConvert.SerializeObject(
            actionValue,
            // avoid serializing reference loops (Vector2, Vector3, etc.)
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }
        );
    }

    [ServerRpc]
    private void SendInputDataServerRpc(string serializedActionValues)
    {
        ProcessInputDataOnServer(serializedActionValues);
    }

    private void ProcessInputDataOnServer(string serializedActionValues)
    {
        // Receive data from client
        string[] values = serializedActionValues.Split(";");
        // Validity check

        if (values.Length == numActions)
        {
            actionValues = values;
        }
        else
        {
            Debug.Log("Invalid number of actions received");
            return;
        }
    }
}
