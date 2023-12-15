using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour 
{
    public GameObject humanPrefab; // Assign the Human VR character prefab
    public GameObject robotPrefab; // Assign the Robot VR character prefab

    // Edit
    [SerializeField] private Vector3 spawnPositionLower;
    [SerializeField] private Vector3 spawnPositionUpper;
    [SerializeField] private Quaternion spawnRotation = Quaternion.identity;

    public void SpawnPlayer(bool isHuman) 
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnPositionLower.x, spawnPositionUpper.x), 
            Random.Range(spawnPositionLower.y, spawnPositionUpper.y), 
            Random.Range(spawnPositionLower.z, spawnPositionUpper.z)
        );
        spawnPosition = Vector3.zero;
        spawnRotation = Quaternion.identity;

        if (IsServer)
        {
            // Select the prefab to instantiate
            GameObject prefabToSpawn = isHuman ? humanPrefab : robotPrefab;
            // Instantiate the selected prefab
            GameObject newPlayer = Instantiate(
                prefabToSpawn, spawnPosition, spawnRotation
            );

            // Spawn the selected prefab
            newPlayer.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            SpawnPlayerServerRpc(OwnerClientId, isHuman, spawnPosition, spawnRotation);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(
        ulong clientId,
        bool isHuman,
        Vector3 spawnPosition,
        Quaternion spawnRotation
    ) {
        // Select the prefab to instantiate
        GameObject prefabToSpawn = isHuman ? humanPrefab : robotPrefab;
        // Instantiate the selected prefab
        GameObject newPlayer = Instantiate(
            prefabToSpawn, spawnPosition, spawnRotation
        );

        // Spawn the selected prefab
        NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();
        networkObject.ChangeOwnership(clientId);
        networkObject.Spawn();
    }
}
