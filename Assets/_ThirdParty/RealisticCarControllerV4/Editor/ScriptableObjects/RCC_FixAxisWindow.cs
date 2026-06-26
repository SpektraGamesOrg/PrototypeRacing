//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Editor window for rotating a mesh's vertices to correct its axis orientation and saving the result as a new mesh asset.
/// </summary>
public class RCC_FixAxisWindow : EditorWindow {

    private GUISkin skin;
    public MeshFilter target;

    private const int windowWidth = 450;
    private const int windowHeight = 250;

    public string savedInstanceLocation = "";

    private Mesh tempMesh;
    private bool saved = false;

    public void OnEnable() {

        titleContent = new GUIContent("Fix Axis And Rotate Mesh Origin");
        maxSize = new Vector2(windowWidth, windowHeight);
        minSize = maxSize;

        InitStyle();
        saved = false;

    }

    /// <summary>
    /// Lazily loads the shared RCC editor GUISkin used to style this window.
    /// </summary>
    public void InitStyle() {

        if (!skin)
            skin = Resources.Load("RCC_WindowSkin") as GUISkin;

    }

    public void OnGUI() {

        GUI.skin = skin != null ? skin : EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);

        if (target == null || target.sharedMesh == null) {

            EditorGUILayout.HelpBox("No mesh filter target assigned.", MessageType.Warning);
            return;

        }

        if (tempMesh == null)
            tempMesh = (Mesh)Instantiate(target.sharedMesh);

        savedInstanceLocation = "Assets/RealisticCarControllerV4/Fixed Meshes";

        EditorGUILayout.LabelField("Fixing axis of the " + target.name + ".");
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Model will not be overwritten, new mesh data will be saved as another instance.");
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Saved location of the mesh: ");
        EditorGUILayout.LabelField(savedInstanceLocation);
        EditorGUILayout.Space();

        bool fixedRotation = 1 - Mathf.Abs(Quaternion.Dot(target.transform.rotation, target.transform.root.rotation)) < .01f;

        if (!fixedRotation) {

            EditorGUILayout.BeginHorizontal(GUI.skin.box);

            if (GUILayout.Button("Reset Pivot Rotation"))
                target.transform.rotation = target.transform.root.rotation;

            EditorGUILayout.EndHorizontal();

        }

        //if (target.transform.rotation != target.transform.root.rotation)
        //    fixedRotation = false;

        GUI.enabled = fixedRotation;

        if (!fixedRotation)
            EditorGUILayout.HelpBox("Reset pivot rotation to rotate mesh.", MessageType.Info);

        EditorGUILayout.BeginHorizontal(GUI.skin.box);

        if (GUILayout.Button("Mesh Rotate X")) {

            Vector3[] vertices = tempMesh.vertices;
            Vector3[] newVertices = new Vector3[vertices.Length];
            Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f);

            for (int i = 0; i < vertices.Length; i++)
                newVertices[i] = rotation * vertices[i];

            tempMesh.vertices = newVertices;
            tempMesh.RecalculateNormals();
            tempMesh.RecalculateBounds();
            target.mesh = tempMesh;
            EditorUtility.SetDirty(target);

        }

        if (GUILayout.Button("Mesh Rotate Y")) {

            Vector3[] vertices = tempMesh.vertices;
            Vector3[] newVertices = new Vector3[vertices.Length];
            Quaternion rotation = Quaternion.Euler(0f, -90f, 0f);

            for (int i = 0; i < vertices.Length; i++)
                newVertices[i] = rotation * vertices[i];

            tempMesh.vertices = newVertices;
            tempMesh.RecalculateNormals();
            tempMesh.RecalculateBounds();
            target.mesh = tempMesh;
            EditorUtility.SetDirty(target);

        }

        if (GUILayout.Button("Mesh Rotate Z")) {

            Vector3[] vertices = tempMesh.vertices;
            Vector3[] newVertices = new Vector3[vertices.Length];
            Quaternion rotation = Quaternion.Euler(0f, 0f, -90f);

            for (int i = 0; i < vertices.Length; i++)
                newVertices[i] = rotation * vertices[i];

            tempMesh.vertices = newVertices;
            tempMesh.RecalculateNormals();
            tempMesh.RecalculateBounds();
            target.mesh = tempMesh;
            EditorUtility.SetDirty(target);

        }

        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;

        if (GUILayout.Button("Save Mesh & Close")) {

            saved = true;
            Mesh tmp = SaveMesh(target.sharedMesh);
            target.mesh = tmp;
            CheckMeshCollider();
            Close();

        }

    }

    /// <summary>
    /// Persists changes to the underlying asset by writing a copy of the supplied mesh into the Fixed Meshes folder.
    /// </summary>
    /// <param name="mesh">The source mesh that will be cloned and saved as a new asset.</param>
    /// <returns>The newly created mesh asset.</returns>
    public Mesh SaveMesh(Mesh mesh) {

        Mesh tmp = (Mesh)Instantiate(mesh);

        if (!AssetDatabase.IsValidFolder(savedInstanceLocation + "/" + target.transform.root.name))
            AssetDatabase.CreateFolder("Assets/RealisticCarControllerV4/Fixed Meshes", target.transform.root.name);

        AssetDatabase.CreateAsset(tmp, savedInstanceLocation + "/" + target.transform.root.name + "/" + mesh.name + ".mesh");
        return tmp;

    }

    /// <summary>
    /// Updates the target's MeshCollider (if any) to reference the newly saved mesh.
    /// </summary>
    public void CheckMeshCollider() {

        MeshCollider mCol = target.GetComponent<MeshCollider>();

        if (mCol)
            mCol.sharedMesh = target.sharedMesh;

    }

    public void OnDisable() {

        target = null;

        int goBack = 0;

        if (!saved)
            goBack = EditorUtility.DisplayDialogComplex("Realistic Car Controller | Mesh not saved", "Mesh is not saved yet. You will need to click save mesh button to save fixed mesh.", "Back", "Don't Save", "");

    }

}
