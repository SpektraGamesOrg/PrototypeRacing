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
using UnityEngine.SceneManagement;

/// <summary>
/// Simple utility component for loading scenes by name. Intended for use with UI button OnClick events in demo scenes.
/// </summary>
public class RCC_LevelLoader : RCC_Core {

    /// <summary>
    /// Loads a scene by its name. The scene must be included in the Build Settings.
    /// </summary>
    /// <param name="levelName">The name of the scene to load.</param>
    public void LoadLevel(string levelName) {

        SceneManager.LoadScene(levelName);

    }

}
