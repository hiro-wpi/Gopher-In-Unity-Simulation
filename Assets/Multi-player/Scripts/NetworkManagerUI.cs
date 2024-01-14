using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverHumanButton;
    [SerializeField] private Button serverRobotButton;
    [SerializeField] private Button hostHumanButton;
    [SerializeField] private Button hostRobotButton;
    [SerializeField] private Button clientHumanButton;
    [SerializeField] private Button clientRobotButton;

    private void Awake()
    {
        serverButton.onClick.AddListener(() =>
        {
            EnableServerButtons();
        });

        hostButton.onClick.AddListener(() =>
        {
            EnableHostButtons();
        });

        clientButton.onClick.AddListener(() =>
        {
            EnableClientButtons();
        });

        serverHumanButton.onClick.AddListener(() =>
        {
            StartServer(true);
        });

        serverRobotButton.onClick.AddListener(() =>
        {
            StartServer(false);
        });

        hostHumanButton.onClick.AddListener(() =>
        {
            StartHost(true);
        });

        hostRobotButton.onClick.AddListener(() =>
        {
            StartHost(false);
        });

        clientHumanButton.onClick.AddListener(() =>
        {
            StartClient(true);
        });

        clientRobotButton.onClick.AddListener(() =>
        {
            StartClient(false);
        });

        // Initial setup
        SetMainButtonsEnabled(true);
        SetPlayerSelectionButtonsEnabled(false, false, false, false);
    }

    void EnableServerButtons()
    {
        SetMainButtonsEnabled(false);
        SetPlayerSelectionButtonsEnabled(true, true, false, false);
    }

    void EnableHostButtons()
    {
        SetMainButtonsEnabled(false);
        SetPlayerSelectionButtonsEnabled(true, false, true, false);
    }

    void EnableClientButtons()
    {
        SetMainButtonsEnabled(false);
        SetPlayerSelectionButtonsEnabled(true, false, false, true);
    }

    void StartServer(bool isHuman)
    {
        NetworkManager.Singleton.StartServer();
        SetPlayerSelectionButtonsEnabled(false, false, false, false);
        spawnManager.SpawnPlayer(isHuman);
    }

    void StartHost(bool isHuman)
    {
        NetworkManager.Singleton.StartHost();
        SetPlayerSelectionButtonsEnabled(false, false, false, false);
        spawnManager.SpawnPlayer(isHuman);
    }

    void StartClient(bool isHuman)
    {
        NetworkManager.Singleton.StartClient();
        SetPlayerSelectionButtonsEnabled(false, false, false, false);
        spawnManager.SpawnPlayer(isHuman);
    }

    void SetMainButtonsEnabled(bool isEnabled)
    {
        serverButton.gameObject.SetActive(isEnabled);
        hostButton.gameObject.SetActive(isEnabled);
        clientButton.gameObject.SetActive(isEnabled);
    }

    void SetPlayerSelectionButtonsEnabled(
        bool isEnabled,
        bool showServerButtons,
        bool showHostButtons,
        bool showClientButtons
    ) {
        serverHumanButton.gameObject.SetActive(isEnabled && showServerButtons);
        serverRobotButton.gameObject.SetActive(isEnabled && showServerButtons);
        hostHumanButton.gameObject.SetActive(isEnabled && showHostButtons);
        hostRobotButton.gameObject.SetActive(isEnabled && showHostButtons);
        clientHumanButton.gameObject.SetActive(isEnabled && showClientButtons);
        clientRobotButton.gameObject.SetActive(isEnabled && showClientButtons);
    }
}
