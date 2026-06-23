//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom inspector for RCC_WheelCollider components.
/// </summary>
[CustomEditor(typeof(RCC_WheelCollider))]
[CanEditMultipleObjects]
public class RCC_WheelColliderEditor : Editor {

    RCC_WheelCollider prop;

    // SerializedProperties for the fields we want to expose in the custom inspector.
    private SerializedProperty wheelModel;
    private SerializedProperty alignWheel;
    private SerializedProperty drawSkid;
    private SerializedProperty canPower;
    private SerializedProperty powerMultiplier;
    private SerializedProperty canSteer;
    private SerializedProperty steeringMultiplier;
    private SerializedProperty canBrake;
    private SerializedProperty brakingMultiplier;
    private SerializedProperty canHandbrake;
    private SerializedProperty handbrakeMultiplier;
    private SerializedProperty wheelWidth;
    private SerializedProperty wheelOffset;
    private SerializedProperty camber;
    private SerializedProperty caster;
    private SerializedProperty toe;
    private SerializedProperty forwardGrip;
    private SerializedProperty sidewaysGrip;
    private SerializedProperty deflateRadiusMultiplier;
    private SerializedProperty deflatedStiffnessMultiplier;
    private SerializedProperty ackermanWheelBase;
    private SerializedProperty ackermanSteerReference;
    private SerializedProperty ackermanTrackWidth;

    // Foldouts to organize sections in the Inspector.
    private bool showWheelState = true;
    private bool showFriction = false;
    private bool showDeflation = false;
    private bool showAckerman = false;

    // Called once when this Editor is initialized.
    public void OnEnable() {
        // Link SerializedProperties with fields in RCC_WheelCollider
        wheelModel = serializedObject.FindProperty("wheelModel");
        alignWheel = serializedObject.FindProperty("alignWheel");
        drawSkid = serializedObject.FindProperty("drawSkid");
        canPower = serializedObject.FindProperty("canPower");
        powerMultiplier = serializedObject.FindProperty("powerMultiplier");
        canSteer = serializedObject.FindProperty("canSteer");
        steeringMultiplier = serializedObject.FindProperty("steeringMultiplier");
        canBrake = serializedObject.FindProperty("canBrake");
        brakingMultiplier = serializedObject.FindProperty("brakingMultiplier");
        canHandbrake = serializedObject.FindProperty("canHandbrake");
        handbrakeMultiplier = serializedObject.FindProperty("handbrakeMultiplier");
        wheelWidth = serializedObject.FindProperty("wheelWidth");
        wheelOffset = serializedObject.FindProperty("wheelOffset");
        camber = serializedObject.FindProperty("camber");
        caster = serializedObject.FindProperty("caster");
        toe = serializedObject.FindProperty("toe");
        forwardGrip = serializedObject.FindProperty("forwardGrip");
        sidewaysGrip = serializedObject.FindProperty("sidewaysGrip");
        deflateRadiusMultiplier = serializedObject.FindProperty("deflateRadiusMultiplier");
        deflatedStiffnessMultiplier = serializedObject.FindProperty("deflatedStiffnessMultiplier");
        ackermanWheelBase = serializedObject.FindProperty("ackermanWheelBase");
        ackermanSteerReference = serializedObject.FindProperty("ackermanSteerReference");
        ackermanTrackWidth = serializedObject.FindProperty("ackermanTrackWidth");
    }

    public override void OnInspectorGUI() {

        prop = (RCC_WheelCollider)target;

        // Mandatory call for serialized objects in custom inspectors
        serializedObject.Update();

        // Main title
        EditorGUILayout.LabelField("RCC Wheel Collider", EditorStyles.boldLabel);

        // Wheel Model & Alignment
        EditorGUILayout.PropertyField(
            wheelModel,
            new GUIContent("Wheel Model")
        );
        EditorGUILayout.PropertyField(
            alignWheel,
            new GUIContent("Align Wheel Model")
        );
        EditorGUILayout.PropertyField(
            drawSkid,
            new GUIContent("Draw Skidmarks")
        );

        EditorGUILayout.Space();
        showWheelState = EditorGUILayout.Foldout(showWheelState, "Wheel State & Geometry", true);
        if (showWheelState) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                canPower,
                new GUIContent("Can Power")
            );
            if (canPower.boolValue) {
                EditorGUILayout.PropertyField(
                    powerMultiplier,
                    new GUIContent("Power Multiplier")
                );
            }

            EditorGUILayout.PropertyField(
                canSteer,
                new GUIContent("Can Steer")
            );
            if (canSteer.boolValue) {
                EditorGUILayout.PropertyField(
                    steeringMultiplier,
                    new GUIContent("Steering Multiplier")
                );
            }

            EditorGUILayout.PropertyField(
                canBrake,
                new GUIContent("Can Brake")
            );
            if (canBrake.boolValue) {
                EditorGUILayout.PropertyField(
                    brakingMultiplier,
                    new GUIContent("Braking Multiplier")
                );
            }

            EditorGUILayout.PropertyField(
                canHandbrake,
                new GUIContent("Can Handbrake")
            );
            if (canHandbrake.boolValue) {
                EditorGUILayout.PropertyField(
                    handbrakeMultiplier,
                    new GUIContent("Handbrake Multiplier")
                );
            }

            EditorGUILayout.PropertyField(
                wheelWidth,
                new GUIContent("Wheel Width")
            );
            EditorGUILayout.PropertyField(
                wheelOffset,
                new GUIContent("Wheel Offset")
            );

            EditorGUILayout.Slider(
                camber, -5f, 5f,
                new GUIContent("Camber")
            );
            EditorGUILayout.Slider(
                caster, -5f, 5f,
                new GUIContent("Caster")
            );
            EditorGUILayout.Slider(
                toe, -5f, 5f,
                new GUIContent("Toe")
            );
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        showFriction = EditorGUILayout.Foldout(showFriction, "Friction & Grip", true);
        if (showFriction) {
            EditorGUI.indentLevel++;
            EditorGUILayout.Slider(
                forwardGrip, 0f, 1f,
                new GUIContent("Forward Grip")
            );
            EditorGUILayout.Slider(
                sidewaysGrip, 0f, 1f,
                new GUIContent("Sideways Grip")
            );
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        showDeflation = EditorGUILayout.Foldout(showDeflation, "Deflation Settings", true);
        if (showDeflation) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                deflateRadiusMultiplier,
                new GUIContent("Deflate Radius Multiplier")
            );
            EditorGUILayout.PropertyField(
                deflatedStiffnessMultiplier,
                new GUIContent("Deflated Stiffness Multiplier")
            );
            EditorGUILayout.HelpBox(
                "Deflation settings reduce wheel radius and stiffness, " +
                "useful for simulating flat tires or punctures.",
                MessageType.Info
            );
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        showAckerman = EditorGUILayout.Foldout(showAckerman, "Ackerman Steering", true);
        if (showAckerman) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                ackermanWheelBase,
                new GUIContent("Wheel Base")
            );
            EditorGUILayout.PropertyField(
                ackermanSteerReference,
                new GUIContent("Steer Reference")
            );
            EditorGUILayout.PropertyField(
                ackermanTrackWidth,
                new GUIContent("Track Width")
            );
            EditorGUILayout.HelpBox(
                "Ackerman parameters help simulate realistic front wheel angles in tight turns. " +
                "It adjusts the inside/outside wheel steer angles for a more accurate turn radius.",
                MessageType.Info
            );
            EditorGUI.indentLevel--;
        }

        if (GUI.changed)
            EditorUtility.SetDirty(prop);

        // Apply changes back to the target object
        serializedObject.ApplyModifiedProperties();

        if (!EditorApplication.isPlaying && EditorWindow.mouseOverWindow != null && EditorWindow.mouseOverWindow.GetType().Name == "InspectorWindow")
            Repaint();

    }

}

