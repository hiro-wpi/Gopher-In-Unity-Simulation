using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    public GameObject humanPrefab;
    public GameObject robotPrefab;

    [SerializeField] private Vector3 humanSpawnPositionLower;
    [SerializeField] private Vector3 humanSpawnPositionUpper;
    [SerializeField] private Vector3 robotSpawnPositionLower;
    [SerializeField] private Vector3 robotSpawnPositionUpper;

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
        Vector3 spawnPosition = GetRandomSpawnPosition(isHuman);

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

    Vector3 GetRandomSpawnPosition(bool isHuman)
    {
        Vector3 spawnPosition;

        if (isHuman)
        {
            spawnPosition = new Vector3(
                Random.Range(humanSpawnPositionLower.x, humanSpawnPositionUpper.x),
                Random.Range(humanSpawnPositionLower.y, humanSpawnPositionUpper.y),
                Random.Range(humanSpawnPositionLower.z, humanSpawnPositionUpper.z)
            );
        }
        else
        {
            spawnPosition = new Vector3(
                Random.Range(robotSpawnPositionLower.x, robotSpawnPositionUpper.x),
                Random.Range(robotSpawnPositionLower.y, robotSpawnPositionUpper.y),
                Random.Range(robotSpawnPositionLower.z, robotSpawnPositionUpper.z)
            );
        }

        return spawnPosition;
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
