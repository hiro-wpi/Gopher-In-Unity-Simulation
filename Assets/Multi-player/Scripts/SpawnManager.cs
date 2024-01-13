using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    public GameObject humanPrefab;
    public GameObject robotPrefab;

    [SerializeField] private Vector3 spawnPositionLower;
    [SerializeField] private Vector3 spawnPositionUpper;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnPlayerServerRpc(
                NetworkManager.Singleton.LocalClientId,
                true, Vector3.zero, Quaternion.identity
            );
        }
    }

    public void SpawnPlayer(bool isHuman)
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnPositionLower.x, spawnPositionUpper.x),
            Random.Range(spawnPositionLower.y, spawnPositionUpper.y),
            Random.Range(spawnPositionLower.z, spawnPositionUpper.z)
        );

        if (IsServer)
        {
            GameObject prefabToSpawn = isHuman ? humanPrefab : robotPrefab;
            GameObject newPlayer = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            newPlayer.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            Debug.LogError("Cannot spawn player on a non-server client.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(
        ulong clientId,
        bool isHuman,
        Vector3 spawnPosition,
        Quaternion spawnRotation
    )
    {
        GameObject prefabToSpawn = isHuman ? humanPrefab : robotPrefab;
        GameObject newPlayer = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(clientId);
    }
}
