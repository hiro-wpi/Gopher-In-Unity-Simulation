using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentMenus : MonoBehaviour
{
    // UIs
    public GameObject mainMenus;
    public GameObject loadingScene;
    public GameObject quitMenus;
    public GameObject experimentCompletionScene;

    // Load main menus
    public void LoadMainMenus()
    {
        // Show menus
        HideAll();
        mainMenus.SetActive(true);
    }

    // Load loading scene
    public void LoadLoading(float delayTime = 4f)
    {
        // Show loading scene
        HideAll();
        loadingScene.SetActive(true);
        // Pause for a while then hide loading scene
        StartCoroutine(LoadLoadingCoroutine(delayTime));
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
        HideAll();
        quitMenus.SetActive(true);
    }

    public void LoadExperimentCompleted()
    {
        // Show quit menus
        HideAll();
        experimentCompletionScene.SetActive(true);
    }

    // Hide all
    public void HideAll()
    {
        // Show quit menus
        mainMenus.SetActive(false);
        loadingScene.SetActive(false);
        quitMenus.SetActive(false);
        experimentCompletionScene.SetActive(false);
    }
}
