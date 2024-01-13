using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button humanButton;
    [SerializeField] private Button robotButton;

    private bool isHosting;
    private bool isClienting;
    private bool isServering;

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            StartHost();
        });

        clientButton.onClick.AddListener(() =>
        {
            StartClient();
        });

        serverButton.onClick.AddListener(() =>
        {
            StartServer();
        });

        humanButton.onClick.AddListener(() =>
        {
            OnPlayerSelection(true);
        });

        robotButton.onClick.AddListener(() =>
        {
            OnPlayerSelection(false);
        });
    }

    void StartHost()
    {
        isHosting = true;
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        serverButton.gameObject.SetActive(false);
        humanButton.gameObject.SetActive(true);
        robotButton.gameObject.SetActive(true);
    }

    void StartClient()
    {
        isClienting = true;
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        serverButton.gameObject.SetActive(false);
        humanButton.gameObject.SetActive(true);
        robotButton.gameObject.SetActive(true);
    }

    void StartServer()
    {
        isServering = true;
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        serverButton.gameObject.SetActive(false);
        humanButton.gameObject.SetActive(true);
        robotButton.gameObject.SetActive(true);
    }

    void OnPlayerSelection(bool isHuman)
    {
        if (isHosting)
        {
            NetworkManager.Singleton.StartHost();
            isHosting = false;
        }
        else if (isClienting)
        {
            NetworkManager.Singleton.StartClient();
            isClienting = false;
        }
        else if (isServering)
        {
            NetworkManager.Singleton.StartServer();
            isServering = false;
        }

        TurnOffPlayerSelectionUI();
        SpawnPlayer(isHuman);
    }

    void TurnOffPlayerSelectionUI()
    {
        humanButton.gameObject.SetActive(false);
        robotButton.gameObject.SetActive(false);
    }

    void SpawnPlayer(bool isHuman)
    {
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();

        if (spawnManager != null)
        {
            spawnManager.SpawnPlayer(isHuman);
        }
        else
        {
            Debug.LogError("SpawnManager not found! Make sure it's present in the scene.");
        }
    }
}
