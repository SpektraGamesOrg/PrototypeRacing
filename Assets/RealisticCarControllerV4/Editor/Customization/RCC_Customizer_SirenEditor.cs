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
/// Custom inspector for RCC_Customizer_SirenManager components.
/// </summary>
[CustomEditor(typeof(RCC_Customizer_SirenManager))]
public class RCC_Customizer_SirenEditor : Editor {

    RCC_Customizer_SirenManager prop;

    public override void OnInspectorGUI() {

        prop = (RCC_Customizer_SirenManager)target;
        serializedObject.Update();

        EditorGUILayout.HelpBox("All sirens can be used under this manager. Each siren has target index. Click 'Get All Sirens' after editing sirens.", MessageType.None);

        DrawDefaultInspector();

        if (GUILayout.Button("Get All Sirens"))
            prop.sirens = prop.GetComponentsInChildren<RCC_Customizer_Siren>(true);

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
