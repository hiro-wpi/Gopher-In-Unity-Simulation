using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverRobotButton;
    [SerializeField] private Button clientRobotButton;
    [SerializeField] private Button serverHumanButton;
    [SerializeField] private Button clientHumanButton;

    private void Awake()
    {
        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            ShowButtons(false, true, false);
        });

        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            ShowButtons(false, false, true);
        });

        serverHumanButton.onClick.AddListener(() =>
        {
            ShowButtons(false, false, false);
            StartServer(true);
        });

        serverRobotButton.onClick.AddListener(() =>
        {
            ShowButtons(false, false, false);
            StartServer(false);
        });

        clientHumanButton.onClick.AddListener(() =>
        {
            ShowButtons(false, false, false);
            StartClient(true);
        });

        clientRobotButton.onClick.AddListener(() =>
        {
            ShowButtons(false, false, false);
            StartClient(false);
        });

        // Initial setup
        ShowButtons(true, false, false);
    }

    void StartServer(bool isHuman)
    {
        spawnManager.SpawnPlayer(isHuman);
    }

    void StartClient(bool isHuman)
    {
        spawnManager.SpawnPlayer(isHuman);
    }

    void ShowButtons(bool showMainButtons, bool showServerButtons, bool showClientButtons)
    {
        serverButton.gameObject.SetActive(showMainButtons);
        clientButton.gameObject.SetActive(showMainButtons);

        serverHumanButton.gameObject.SetActive(showServerButtons);
        serverRobotButton.gameObject.SetActive(showServerButtons);
        clientHumanButton.gameObject.SetActive(showClientButtons);
        clientRobotButton.gameObject.SetActive(showClientButtons);
    }
}
