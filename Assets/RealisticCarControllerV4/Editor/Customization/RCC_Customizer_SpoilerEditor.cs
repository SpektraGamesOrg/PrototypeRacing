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

/// <summary>
/// Custom inspector for RCC_Customizer_SpoilerManager components.
/// </summary>
[CustomEditor(typeof(RCC_Customizer_SpoilerManager))]
public class RCC_Customizer_SpoilerEditor : Editor {

    RCC_Customizer_SpoilerManager prop;

    public override void OnInspectorGUI() {

        prop = (RCC_Customizer_SpoilerManager)target;
        serializedObject.Update();

        EditorGUILayout.HelpBox("All spoilers can be used under this manager. Each spoiler has target index and paintable renderer. Click 'Get All Spoilers' after editing spoilers.", MessageType.None);

        DrawDefaultInspector();

        if (GUILayout.Button("Get All Spoilers"))
            prop.spoilers = prop.GetComponentsInChildren<RCC_Customizer_Spoiler>(true);

        if (GUI.changed)
            EditorUtility.SetDirty(prop);

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Back")) {

            RCC_Customizer customizer = prop.GetComponentInParent<RCC_Customizer>(true);

            if (customizer)
                Selection.activeGameObject = customizer.gameObject;

        }

    }

}
