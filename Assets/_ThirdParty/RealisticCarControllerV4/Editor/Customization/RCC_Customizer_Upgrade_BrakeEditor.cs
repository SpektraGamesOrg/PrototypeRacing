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
/// Custom inspector for RCC_Customizer_Brake components.
/// </summary>
[CustomEditor(typeof(RCC_Customizer_Brake))]
public class RCC_Customizer_Upgrade_BrakeEditor : Editor {

    RCC_Customizer_Brake prop;

    public override void OnInspectorGUI() {

        prop = (RCC_Customizer_Brake)target;
        serializedObject.Update();

        RCC_CarControllerV4 carController = prop.GetComponentInParent<RCC_CarControllerV4>(true);

        if (carController)
            EditorGUILayout.HelpBox("Brake torque will be calculated by efficiency value of the upgrader depending on the level.\nDefault brake torque: " + carController.brakeTorque + "\nFully upgraded brake torque: " + carController.brakeTorque * prop.efficiency + "\nCurrent level: " + prop.BrakeLevel, MessageType.None);
        else
            EditorGUILayout.HelpBox("Brake torque will be calculated by efficiency value of the upgrader depending on the level.\nCurrent level: " + prop.BrakeLevel, MessageType.None);

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
