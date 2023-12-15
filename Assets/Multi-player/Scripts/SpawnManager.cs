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


    public void SpawnPlayer(bool isHuman) {
        GameObject prefabToSpawn = isHuman ? humanPrefab : robotPrefab;

        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnPositionLower.x, spawnPositionUpper.x), 
            Random.Range(spawnPositionLower.y, spawnPositionUpper.y), 
            Random.Range(spawnPositionLower.z, spawnPositionUpper.z)
        );

        // Instantiate the selected prefab at the designated spawn position
        GameObject newPlayer = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        newPlayer.GetComponent<NetworkObject>().Spawn();
    }
}
