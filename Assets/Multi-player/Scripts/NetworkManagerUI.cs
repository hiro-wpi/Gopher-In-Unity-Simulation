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
    }

    void TurnOffUI(bool value)
    {
        serverButton.gameObject.SetActive(value);
        hostButton.gameObject.SetActive(value);
        clientButton.gameObject.SetActive(value);
    }
}
