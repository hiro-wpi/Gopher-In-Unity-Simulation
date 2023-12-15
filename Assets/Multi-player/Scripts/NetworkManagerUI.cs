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
    [SerializeField] private Button humanHostButton;
    [SerializeField] private Button humanClientButton;
    [SerializeField] private Button robotHostButton;
    [SerializeField] private Button robotClientButton;

    private void Awake()
    {
        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            TurnOffUI(false);
        });

        humanHostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            PlayerSelection(true);
            TurnOffUI(false);
        });

        humanClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            PlayerSelection(true);
            TurnOffUI(false);
        });

        robotHostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            PlayerSelection(false);
            TurnOffUI(false);
        });

        robotClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            PlayerSelection(false);
            TurnOffUI(false);
        });
    }

    void TurnOffUI(bool value)
    {
        serverButton.gameObject.SetActive(value);
        humanHostButton.gameObject.SetActive(value);
        humanClientButton.gameObject.SetActive(value);
        robotHostButton.gameObject.SetActive(value);
        robotClientButton.gameObject.SetActive(value);
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
