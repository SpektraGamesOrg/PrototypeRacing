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
/// Custom inspector for RCC_Customizer_Handling components.
/// </summary>
[CustomEditor(typeof(RCC_Customizer_Handling))]
public class RCC_Customizer_Upgrade_HandlingEditor : Editor {

    RCC_Customizer_Handling prop;

    public override void OnInspectorGUI() {

        prop = (RCC_Customizer_Handling)target;
        serializedObject.Update();

        RCC_CarControllerV4 carController = prop.GetComponentInParent<RCC_CarControllerV4>(true);

        if (carController)
            EditorGUILayout.HelpBox("Handling strength will be calculated by efficiency value of the upgrader depending on the level.\nDefault handling strength: " + carController.tractionHelperStrength + "\nFully upgraded handling strength: " + carController.tractionHelperStrength * prop.efficiency + "\nCurrent level: " + prop.HandlingLevel, MessageType.None);
        else
            EditorGUILayout.HelpBox("Handling strength will be calculated by efficiency value of the upgrader depending on the level.\nCurrent level: " + prop.HandlingLevel, MessageType.None);

        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(prop);

        if (GUILayout.Button("Back")) {

            RCC_Customizer customizer = prop.GetComponentInParent<RCC_Customizer>(true);

            if (customizer)
                Selection.activeGameObject = customizer.gameObject;

        }

    }

}
