using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

/// <summary>
///    A simple UI for multi-player server, host, and client
/// </summary>
public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button humanButton;
    [SerializeField] private Button robotButton;

    private void Awake()
    {
        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            TurnOffUI(false);
        });

        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            TurnOffUI(false);
        });

        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            TurnOffUI(false);
        });

        // Handling player selection
        humanButton.onClick.AddListener(() =>
        {
            PlayerSelection(true);
            TurnOffUI(false);
        });

        robotButton.onClick.AddListener(() =>
        {
            PlayerSelection(false);
            TurnOffUI(false);
        });
    }

    void TurnOffUI(bool value)
    {
        serverButton.gameObject.SetActive(value);
        hostButton.gameObject.SetActive(value);
        clientButton.gameObject.SetActive(value);
        humanButton.gameObject.SetActive(value);
        robotButton.gameObject.SetActive(value);
    }

    void PlayerSelection(bool isHuman)
    {
        // Access the SpawnManager script here
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();

        if (spawnManager != null)
        {
            // Call the SpawnPlayer method in the SpawnManager based on the selected character
            spawnManager.SpawnPlayer(isHuman);
        }
        else
        {
            Debug.LogError("SpawnManager not found! Make sure it's present in the scene.");
        }
    }
}
