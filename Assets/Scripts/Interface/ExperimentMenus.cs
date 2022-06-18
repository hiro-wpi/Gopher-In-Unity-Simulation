using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentMenus : MonoBehaviour
{
    // UIs
    public GameObject mainMenus;
    public GameObject loadingScene;
    public GameObject quitMenus;

    // Load main menus
    public void LoadMainMenus()
    {
        // Show menus
        quitMenus.SetActive(false);
        loadingScene.SetActive(false);
        mainMenus.SetActive(true);
    }

    // Load loading scene
    public void LoadLoading()
    {
        // Show loading scene
        mainMenus.SetActive(false);
        quitMenus.SetActive(false);
        loadingScene.SetActive(true);
        // Pause for a while and hide loading scene
        StartCoroutine(LoadLoadingCoroutine(5.0f));
    }
    private IEnumerator LoadLoadingCoroutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        loadingScene.SetActive(false);
    }

    // Load quit menus
    public void LoadQuitMenus()
    {
        // Show quit menus
        mainMenus.SetActive(false);
        loadingScene.SetActive(false);
        quitMenus.SetActive(true);
    }

    // Hide all
    public void HideAll()
    {
        // Show quit menus
        mainMenus.SetActive(false);
        loadingScene.SetActive(false);
        quitMenus.SetActive(false);
    }
}
