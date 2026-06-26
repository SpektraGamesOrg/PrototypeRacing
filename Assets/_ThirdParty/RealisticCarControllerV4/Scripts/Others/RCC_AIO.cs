//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the All-In-One (AIO) playable demo scene for RCC.
/// This script handles level loading, UI toggling, and application quitting.
/// </summary>
public class RCC_AIO : RCC_Core {

    /// <summary>
    /// Singleton instance of the RCC_AIO script.
    /// </summary>
    private static RCC_AIO instance;

    private int aioSceneBuildIndex = -1;

    /// <summary>
    /// UI panel for standard levels.
    /// </summary>
    [Tooltip("UI panel for standard levels.")]
    public GameObject levels;

    /// <summary>
    /// UI panel for Photon multiplayer levels.
    /// </summary>
    [Tooltip("UI panel for Photon multiplayer levels.")]
    public GameObject photonLevels;

    /// <summary>
    /// UI panel for enter/exit enabled levels.
    /// </summary>
    [Tooltip("UI panel for enter/exit enabled levels.")]
    public GameObject BCGLevels;

    /// <summary>
    /// Back button in the UI.
    /// </summary>
    [Tooltip("Back button in the UI.")]
    public GameObject back;

    /// <summary>
    /// Asynchronous operation for loading scenes.
    /// </summary>
    private AsyncOperation async;

    /// <summary>
    /// Slider used to display the scene loading progress.
    /// </summary>
    [Tooltip("Slider used to display the scene loading progress.")]
    public Slider slider;

    public void Start() {

        // Ensure only one instance exists in the scene. If an instance already exists, destroy this one.
        if (instance) {

            Destroy(gameObject);
            return;

        } else {

            instance = this;
            DontDestroyOnLoad(gameObject);

        }

        aioSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;

        // Re-check panels every time a scene finishes loading. Because this object is marked
        // DontDestroyOnLoad, panels left disabled by ToggleMenu would otherwise stay disabled
        // when the menu scene reloads.
        SceneManager.sceneLoaded += OnSceneLoaded;

        CheckPanels();

    }

    /// <summary>
    /// Called when a new scene has finished loading. Restores panel visibility because this
    /// component persists across scene loads and may have left panels disabled by ToggleMenu.
    /// </summary>
    /// <param name="scene">The loaded scene.</param>
    /// <param name="mode">The load mode used for the scene.</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {

        if (scene.buildIndex == aioSceneBuildIndex)
            CheckPanels();

    }

    /// <summary>
    /// Restores menu panels to their default visible state and disables interactability for
    /// buttons that depend on missing optional features (Photon, Enter/Exit). Runs on Start
    /// and on every scene load.
    /// </summary>
    private void CheckPanels() {

        // Restore the prefab default state: all level group panels visible, back button hidden.
        if (levels)
            levels.SetActive(true);

        if (photonLevels)
            photonLevels.SetActive(true);

        if (BCGLevels)
            BCGLevels.SetActive(true);

        if (back)
            back.SetActive(false);

        // Disabling Photon level buttons if RCC_PHOTON is not defined.
#if !RCC_PHOTON
        if (photonLevels) {

            Toggle[] pbuttons = photonLevels.GetComponentsInChildren<Toggle>();

            foreach (var button in pbuttons)
                button.interactable = false;

        }
#endif

        // Disabling Enter/Exit level buttons if BCG_ENTEREXIT is not defined.
#if !BCG_ENTEREXIT
        if (BCGLevels) {

            Toggle[] bbuttons = BCGLevels.GetComponentsInChildren<Toggle>();

            foreach (var button in bbuttons)
                button.interactable = false;

        }
#endif

    }

    private void OnDestroy() {

        SceneManager.sceneLoaded -= OnSceneLoaded;

    }

    private void Update() {

        if (!slider)
            return;

        // If a level is being loaded asynchronously, update the loading slider.
        if (async != null && !async.isDone) {

            if (!slider.gameObject.activeSelf)
                slider.gameObject.SetActive(true);

            // Update the slider value based on the async progress.
            slider.value = async.progress;

        } else {

            // Hide the slider if loading is complete.
            if (slider.gameObject.activeSelf)
                slider.gameObject.SetActive(false);

        }

    }

    /// <summary>
    /// Loads a specified level asynchronously.
    /// </summary>
    /// <param name="levelName">The name of the level to load.</param>
    public void LoadLevel(string levelName) {

        async = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);

    }

    /// <summary>
    /// Toggles between UI menus, enabling the specified menu while disabling others.
    /// </summary>
    /// <param name="menu">The menu GameObject to activate.</param>
    public void ToggleMenu(GameObject menu) {

        // Disable all UI panels before enabling the target menu.
        if (levels)
            levels.SetActive(false);

        if (photonLevels)
            photonLevels.SetActive(false);

        if (BCGLevels)
            BCGLevels.SetActive(false);

        if (back)
            back.SetActive(false);

        menu.SetActive(true);

    }

    /// <summary>
    /// Closes the application. Has no effect in the Unity Editor.
    /// </summary>
    public void Quit() {

        Application.Quit();

    }

}
