using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class RpcTest : NetworkBehaviour
{
    public List<float> listOfFloats = new List<float> {1.0f, 2.5f, 3.7f, 4.2f}; // Define your list of floats here

    public override void OnNetworkSpawn()
    {
        if (!IsServer && IsOwner)
        {
            // Convert list of floats to a string
            string stringOfFloats = ConvertListToCommaSeparatedString(listOfFloats);

            // Send the string to the server via RPC
            TestServerRpc(stringOfFloats, NetworkObjectId);
        }
    }

    [ServerRpc]
    void TestServerRpc(string floatString, ulong sourceNetworkObjectId)
    {
        Debug.Log($"Server Received the RPC {floatString} on NetworkObject #{sourceNetworkObjectId}");

        // Convert the received string back to a list of floats
        List<float> receivedListOfFloats = ConvertStringToList(floatString);

        // Perform actions with the received list of floats
    }

    // Helper method to convert list of floats to a comma-separated string
    string ConvertListToCommaSeparatedString(List<float> floats)
    {
        string stringOfFloats = string.Join(",", floats);
        return stringOfFloats;
    }

    // Helper method to convert a comma-separated string to a list of floats
    List<float> ConvertStringToList(string floatString)
    {
        List<float> listOfFloats = new List<float>();
        string[] floatArray = floatString.Split(',');

        foreach (string str in floatArray)
        {
            if (float.TryParse(str, out float floatValue))
            {
                listOfFloats.Add(floatValue);
            }
        }

        return listOfFloats;
    }
}