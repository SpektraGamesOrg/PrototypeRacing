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
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Custom inspector for RCC_Customizer_UpgradeManager components.
/// </summary>
[CustomEditor(typeof(RCC_Customizer_UpgradeManager))]
public class RCC_Customizer_UpgradeEditor : Editor {

    RCC_Customizer_UpgradeManager prop;
    Color guiColor;

    public void OnEnable() {

        guiColor = GUI.color;

    }

    public override void OnInspectorGUI() {

        prop = (RCC_Customizer_UpgradeManager)target;
        serializedObject.Update();

        EditorGUILayout.HelpBox("Upgrade manager that contains engine torque, brake torque, handling stability, and maximum speed.", MessageType.None);

        RCC_Customizer_Engine engine = prop.GetComponentInChildren<RCC_Customizer_Engine>(true);
        RCC_Customizer_Brake brake = prop.GetComponentInChildren<RCC_Customizer_Brake>(true);
        RCC_Customizer_Handling handling = prop.GetComponentInChildren<RCC_Customizer_Handling>(true);
        RCC_Customizer_Speed speed = prop.GetComponentInChildren<RCC_Customizer_Speed>(true);

        EditorGUILayout.BeginVertical(GUI.skin.box);

        GUI.enabled = true;
        GUI.color = guiColor;

        if (engine == null) {

            GUI.enabled = false;
            GUI.color = Color.red;

        }

        if (GUILayout.Button("Engine Upgrade") && engine)
            Selection.activeGameObject = engine.gameObject;

        GUI.enabled = true;
        GUI.color = guiColor;

        if (brake == null) {

            GUI.enabled = false;
            GUI.color = Color.red;

        }

        if (GUILayout.Button("Brake Upgrade") && brake)
            Selection.activeGameObject = brake.gameObject;

        GUI.enabled = true;
        GUI.color = guiColor;

        if (handling == null) {

            GUI.enabled = false;
            GUI.color = Color.red;

        }

        if (GUILayout.Button("Handling Upgrade") && handling)
            Selection.activeGameObject = handling.gameObject;

        GUI.enabled = true;
        GUI.color = guiColor;

        if (speed == null) {

            GUI.enabled = false;
            GUI.color = Color.red;

        }

        if (GUILayout.Button("Speed Upgrade") && speed)
            Selection.activeGameObject = speed.gameObject;

        GUI.enabled = true;
        GUI.color = guiColor;

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical(GUI.skin.box);

        if (engine == null) {

            if (GUILayout.Button("Create Engine Upgrade"))
                CreateEngineUpgrade();

        }

        if (brake == null) {

            if (GUILayout.Button("Create Brake Upgrade"))
                CreateBrakeUpgrade();

        }

        if (handling == null) {

            if (GUILayout.Button("Create Handling Upgrade"))
                CreateHandlingUpgrade();

        }

        if (speed == null) {

            if (GUILayout.Button("Create Speed Upgrade"))
                CreateSpeedUpgrade();

        }

        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(prop);

        if (GUILayout.Button("Back")) {

            RCC_Customizer customizer = prop.GetComponentInParent<RCC_Customizer>(true);

            if (customizer)
                Selection.activeGameObject = customizer.gameObject;

        }

    }

    /// <summary>
    /// Creates the engine upgrade sub-component on the active customizer GameObject.
    /// </summary>
    public void CreateEngineUpgrade() {

        RCC_Customizer_Engine newGO = new GameObject("Upgrade_Engine").AddComponent<RCC_Customizer_Engine>();
        newGO.transform.SetParent(prop.transform);
        newGO.transform.localPosition = Vector3.zero;
        newGO.transform.localRotation = Quaternion.identity;

        // Register the creation of the object for undo/redo functionality.
        Undo.RegisterCreatedObjectUndo(newGO.gameObject, "Create Engine Upgrade");

        // Mark the scene as dirty so Unity knows it has changed.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    }

    /// <summary>
    /// Creates the brake upgrade sub-component on the active customizer GameObject.
    /// </summary>
    public void CreateBrakeUpgrade() {

        RCC_Customizer_Brake newGO = new GameObject("Upgrade_Brake").AddComponent<RCC_Customizer_Brake>();
        newGO.transform.SetParent(prop.transform);
        newGO.transform.localPosition = Vector3.zero;
        newGO.transform.localRotation = Quaternion.identity;

        // Register the creation of the object for undo/redo functionality.
        Undo.RegisterCreatedObjectUndo(newGO.gameObject, "Create Brake Upgrade");

        // Mark the scene as dirty so Unity knows it has changed.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    }

    /// <summary>
    /// Creates the handling upgrade sub-component on the active customizer GameObject.
    /// </summary>
    public void CreateHandlingUpgrade() {

        RCC_Customizer_Handling newGO = new GameObject("Upgrade_Handling").AddComponent<RCC_Customizer_Handling>();
        newGO.transform.SetParent(prop.transform);
        newGO.transform.localPosition = Vector3.zero;
        newGO.transform.localRotation = Quaternion.identity;

        // Register the creation of the object for undo/redo functionality.
        Undo.RegisterCreatedObjectUndo(newGO.gameObject, "Create Handling Upgrade");

        // Mark the scene as dirty so Unity knows it has changed.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    }

    /// <summary>
    /// Creates the speed upgrade sub-component on the active customizer GameObject.
    /// </summary>
    public void CreateSpeedUpgrade() {

        RCC_Customizer_Speed newGO = new GameObject("Upgrade_Speed").AddComponent<RCC_Customizer_Speed>();
        newGO.transform.SetParent(prop.transform);
        newGO.transform.localPosition = Vector3.zero;
        newGO.transform.localRotation = Quaternion.identity;

        // Register the creation of the object for undo/redo functionality.
        Undo.RegisterCreatedObjectUndo(newGO.gameObject, "Create Speed Upgrade");

        // Mark the scene as dirty so Unity knows it has changed.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    }

}
