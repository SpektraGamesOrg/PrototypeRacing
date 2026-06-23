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
/// Step-by-step wizard window that converts a selected GameObject into a fully configured RCC vehicle: adds the main controller, validates axis orientation, assigns front and rear wheels, and creates the body collider.
/// </summary>
public class RCC_QuickVehicleSetupWizardWindow : EditorWindow {

    private GUISkin skin;
    private string nextStepButtonString;

    private GameObject selectedRoot;

    public GameObject body;
    public GameObject wheel_FL, wheel_FR, wheel_RL, wheel_RR;

    public int width = 500;
    public int height = 260;

    public int toolbarIndex = 0;

    public float startTime = 0f;
    public float endTime = 0f;

    /// <summary>
    /// Opens the Quick Vehicle Setup Wizard window via the BoneCracker Games menu items.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Quick Vehicle Setup Wizard", false, 50)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Quick Vehicle Setup Wizard", false, 50)]
    public static void OpenWindow() {

        GetWindow(typeof(RCC_QuickVehicleSetupWizardWindow), false);

    }

    /// <summary>
    /// Menu validation callback that enables the wizard menu only when a single active scene GameObject is selected.
    /// </summary>
    /// <returns>True if the wizard can be opened with the current selection.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Quick Vehicle Setup Wizard", true, 50)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Quick Vehicle Setup Wizard", true, 50)]
    public static bool CheckOpenWindow() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeGameObject.activeSelf)
            return false;

        if (EditorUtility.IsPersistent(Selection.activeGameObject))
            return false;

        return true;

    }

    public void OnEnable() {

        startTime = 0f;
        endTime = 0f;

        minSize = new Vector2(width, height);
        maxSize = minSize;

        skin = (GUISkin)Resources.Load("RCC_WindowSkin");
        nextStepButtonString = "Next";

    }

    public void OnGUI() {

        GUI.skin = skin != null ? skin : EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);

        EditorGUILayout.BeginVertical(GUI.skin.box);

        switch (toolbarIndex) {

            case 0:
                ToolbarMenu_Welcome();
                break;

            case 1:
                ToolbarMenu_CreateBehavior();
                break;

            case 2:
                ToolbarMenu_CustomizeBehavior();
                break;

            case 3:
                ToolbarMenu_SelectFrontWheels();
                break;

            case 4:
                ToolbarMenu_SelectRearWheels();
                break;

            case 5:
                ToolbarMenu_Collider();
                break;

            case 6:
                ToolbarMenu_Finish();
                break;

        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();

        Color defGuiColor = GUI.color;
        GUI.color = Color.green;

        if (toolbarIndex == 1 && selectedRoot == null)
            GUI.enabled = false;

        if (toolbarIndex == 3 && (wheel_FL == null || wheel_FR == null))
            GUI.enabled = false;

        if (toolbarIndex == 4 && (wheel_RL == null || wheel_RR == null))
            GUI.enabled = false;

        if (GUILayout.Button(nextStepButtonString))
            Next();

        GUI.enabled = true;
        GUI.color = defGuiColor;

        if (mouseOverWindow == this)
            Repaint();

    }

    /// <summary>
    /// Renders the Welcome page of the wizard, introducing the setup flow and resetting wizard state.
    /// </summary>
    public void ToolbarMenu_Welcome() {

        GUILayout.Label("Welcome", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Welcome to Quick Vehicle Setup Wizard. You'll be able to create your new vehicle with a few steps. Setup includes these simple steps;\n\n1. Selecting the root of the vehicle gameobject in the scene and adding the main controller.\n2. Selecting the front wheels\n3. Selecting the rear wheels\n4. Adding / checking body collider.\n5. Adding addon components.\n\nPlease proceed to the next step to get started.\n\nThis setup only covers the main essentials, editing lights, damage, audio, and other things can be done by accesing to the corresponding addon component after the setup.", MessageType.None);
        nextStepButtonString = "Next";

        startTime = (float)EditorApplication.timeSinceStartup;
        endTime = 0f;

        selectedRoot = null;
        wheel_FL = null;
        wheel_FR = null;
        wheel_RL = null;
        wheel_RR = null;
        body = null;

    }

    /// <summary>
    /// Renders the Create Behavior page of the wizard, prompting the user to select the vehicle root GameObject in the scene.
    /// </summary>
    public void ToolbarMenu_CreateBehavior() {

        GUILayout.Label("Add Main Controller", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("We must add the main controller (RCC_CarControllerV4) to the root of the vehicle gameobject. Don't select multiple gameobjects, or prefabs located in the project directory. \n\n1. Drag and drop the vehicle model, gameobject, or prefab to the scene.\n2. Select the root of your vehicle gameobject in the scene.\n3. And add the main controller by clicking the below green button.", MessageType.None);
        nextStepButtonString = "Yes, I've Selected The Root Of The GameObject, Next";

        EditorGUILayout.Space();

        selectedRoot = null;

        if (Selection.activeGameObject != null) {

            if (Selection.gameObjects.Length == 1 && Selection.activeGameObject.scene.name != null && !EditorUtility.IsPersistent(Selection.activeGameObject))
                selectedRoot = Selection.activeGameObject;

        }

        if (selectedRoot != null) {

            GUILayout.Label("This GameObject Is Root Of My Vehicle", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(selectedRoot.name);
            EditorGUILayout.EndVertical();

        }

    }

    /// <summary>
    /// Renders the Customize Behavior page of the wizard, verifying the model's axis orientation before proceeding.
    /// </summary>
    public void ToolbarMenu_CustomizeBehavior() {

        GUILayout.Label("Customize Controller", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Verifying the axis orientation of your vehicle model. The model must have the correct forward (Z), up (Y), and right (X) axes for wheels, steering, and physics to work properly. If a warning appears below, fix the model orientation before continuing.", MessageType.None);
        nextStepButtonString = "Next";

        bool hasCorrectOrientation = RCC_CheckAxisOrientation.IsAxisOrientationCorrect(selectedRoot);

        if (!hasCorrectOrientation)
            EditorGUILayout.HelpBox("Root of your vehicle model has wrong X Y Z axis orientation. It's essential to have proper axes, otherwise vehicle won't work properly. ", MessageType.Error);

    }

    /// <summary>
    /// Renders the Select Front Wheels page of the wizard, letting the user pick or auto-detect the front left and right wheel transforms.
    /// </summary>
    public void ToolbarMenu_SelectFrontWheels() {

        GUILayout.Label("Front Wheels", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("This vehicle has two axes for front wheels and rear wheels. Please select the front left and front right wheels now.\n\nYou can have any amount of axles on your vehicle. Axles can be accesed through the Axles component on the vehicle. You can create / edit / remove any axle. But once you create another new axle, you must select its left and right wheel manually. No worries, editor will guide you.", MessageType.None);
        nextStepButtonString = "Yes, I've Selected Front Left And Right Wheels, Next";

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);

        wheel_FL = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Front Left Wheel"), wheel_FL, typeof(GameObject), true);
        EditorGUILayout.Space();
        wheel_FR = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Front Right Wheel"), wheel_FR, typeof(GameObject), true);

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Try Detect Front Wheels")) {

            GameObject[] possibleFrontWheels = RCC_DetectPossibleWheels.DetectPossibleFrontWheels(selectedRoot);

            if (possibleFrontWheels != null && possibleFrontWheels.Length >= 2) {

                RCC_PopupWindow_PossibleWheels.ShowWindow(possibleFrontWheels, (input) => {

                    possibleFrontWheels = input;

                    for (int i = 0; i < possibleFrontWheels.Length; i++) {

                        if (IsOnRight(selectedRoot, possibleFrontWheels[i]))
                            wheel_FR = possibleFrontWheels[i];
                        else
                            wheel_FL = possibleFrontWheels[i];

                    }

                });

            } else {

                EditorUtility.DisplayDialog("Realistic Car Controller | Couldn't find any possible wheel models", "Are you sure your model has separated wheel models? If wheel models are part of the main body, it won't work. Be sure your model has separated wheel models.", "Ok");

            }

        }

    }

    /// <summary>
    /// Returns true if the wheel sits on the right side of the vehicle's local space.
    /// </summary>
    /// <param name="vehicle">The vehicle root GameObject providing the local coordinate frame.</param>
    /// <param name="wheel">The wheel GameObject to classify.</param>
    /// <returns>True when the wheel's local X position is positive (right side of the vehicle).</returns>
    public bool IsOnRight(GameObject vehicle, GameObject wheel) {

        // Transform the child's position to the parent's local space
        Vector3 localPosition = vehicle.transform.InverseTransformPoint(wheel.transform.position);

        // Check if the local X coordinate is positive
        return localPosition.x > 0;

    }

    /// <summary>
    /// Renders the Select Rear Wheels page of the wizard, letting the user pick or auto-detect the rear left and right wheel transforms.
    /// </summary>
    public void ToolbarMenu_SelectRearWheels() {

        GUILayout.Label("Rear Wheels", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("This vehicle has two axes for front wheels and rear wheels. Please select the rear left and rear right wheels now.\n\nYou can have any amount of axles on your vehicle. Axles can be accesed through the Axles component on the vehicle. You can create / edit / remove any axle. But once you create another new axle, you must select its left and right wheel manually. No worries, editor will guide you.", MessageType.None);
        nextStepButtonString = "Yes, I've Selected Rear Left And Right Wheels, Next";

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);

        wheel_RL = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Rear Left Wheel"), wheel_RL, typeof(GameObject), true);
        EditorGUILayout.Space();
        wheel_RR = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Rear Right Wheel"), wheel_RR, typeof(GameObject), true);

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Try Detect Rear Wheels")) {

            GameObject[] possibleRearWheels = RCC_DetectPossibleWheels.DetectPossibleRearWheels(selectedRoot);

            if (possibleRearWheels != null && possibleRearWheels.Length >= 2) {

                RCC_PopupWindow_PossibleWheels.ShowWindow(possibleRearWheels, (input) => {

                    possibleRearWheels = input;

                    for (int i = 0; i < possibleRearWheels.Length; i++) {

                        if (IsOnRight(selectedRoot, possibleRearWheels[i]))
                            wheel_RR = possibleRearWheels[i];
                        else
                            wheel_RL = possibleRearWheels[i];

                    }

                });

            } else {

                EditorUtility.DisplayDialog("Realistic Car Controller | Couldn't find any possible wheel models", "Are you sure your model has separated wheel models? If wheel models are part of the main body, it won't work. Be sure your model has separated wheel models.", "Ok");

            }

        }

    }

    /// <summary>
    /// Renders the Collider page of the wizard, detecting existing body colliders and offering to add a MeshCollider to the selected body.
    /// </summary>
    public void ToolbarMenu_Collider() {

        GUILayout.Label("Body Collider", EditorStyles.boldLabel);

        if (!selectedRoot)
            return;

        Collider[] colliders = selectedRoot.GetComponentsInChildren<Collider>();
        bool colliderFound = false;

        for (int i = 0; i < colliders.Length; i++) {

            if (!(colliders[i] as WheelCollider))
                colliderFound = true;

        }

        if (!colliderFound)
            EditorGUILayout.HelpBox("This vehicle doesn't have a body collider. Please select the main body part of your vehicle, and add a mesh collider.", MessageType.None);
        else
            EditorGUILayout.HelpBox("Few colliders have been found in the vehicle. I'm not sure which one is the body collider, please make sure your vehicle as a body collider.", MessageType.None);

        nextStepButtonString = "Yes, I've Added A Body Collider, Next";

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);

        body = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Main Body"), body, typeof(GameObject), true);
        EditorGUILayout.Space();

        if (!body || (body && body.GetComponent<MeshCollider>()))
            GUI.enabled = false;

        if (GUILayout.Button("Add MeshCollider To Selected Body")) {

            Undo.IncrementCurrentGroup();
            int bodyColliderGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Add Body Collider");

            MeshCollider meshCol = Undo.AddComponent<MeshCollider>(body);
            Undo.RecordObject(meshCol, "Configure Body Collider");
            meshCol.convex = true;

            if (RCC_Settings.Instance)
                meshCol.material = RCC_Settings.Instance.colliderMaterial;

            Undo.CollapseUndoOperations(bodyColliderGroup);

        }

        GUI.enabled = true;

        EditorGUILayout.EndVertical();

    }

    /// <summary>
    /// Renders the Finish page of the wizard, reporting the total time taken and closing instructions.
    /// </summary>
    public void ToolbarMenu_Finish() {

        GUILayout.Label("Congratulations!", EditorStyles.boldLabel);

        if (endTime == 0)
            endTime = (float)EditorApplication.timeSinceStartup;

        float timeToCreate = endTime - startTime;
        EditorGUILayout.HelpBox("Congratulations, you've created a new vehicle within " + timeToCreate.ToString("F0") + " seconds!\n\nThis vehicle using default resources, you can change any of them by clicking the corresponding addon component button. Have fun!", MessageType.None);
        nextStepButtonString = "Thanks, See You Next Time...";

    }

    /// <summary>
    /// Advances the wizard to the next step, applying any step-specific actions such as adding the controller, assigning wheels, or creating wheel colliders.
    /// </summary>
    public void Next() {

        if (toolbarIndex == 1 && selectedRoot) {

            RCC_CarControllerV4 newVehicle = RCC_CreateNewVehicle.NewVehicle(selectedRoot);

            if (newVehicle)
                selectedRoot = newVehicle.gameObject;
            else
                Close();

        }

        if (toolbarIndex == 3 && selectedRoot && wheel_FL && wheel_FR) {

            RCC_CarControllerV4 carController = selectedRoot.GetComponent<RCC_CarControllerV4>();

            if (carController) {

                Undo.RecordObject(carController, "Assign Front Wheels");
                carController.FrontLeftWheelTransform = wheel_FL.transform;
                carController.FrontRightWheelTransform = wheel_FR.transform;

            }

        }

        if (toolbarIndex == 4 && selectedRoot && wheel_RL && wheel_RR) {

            Undo.IncrementCurrentGroup();
            int rearWheelsGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Assign Rear Wheels and Create Wheel Colliders");

            RCC_CarControllerV4 carController = selectedRoot.GetComponent<RCC_CarControllerV4>();

            if (carController) {

                Undo.RecordObject(carController, "Assign Rear Wheels and Wheel Models");
                carController.RearLeftWheelTransform = wheel_RL.transform;
                carController.RearRightWheelTransform = wheel_RR.transform;

                carController.CreateWheelColliders();

                // CreateWheelColliders is runtime code (RCC_Core) and cannot use the Undo API directly.
                // Register the resulting "Wheel Colliders" subtree so the user can Ctrl+Z out of this step.
                Transform wheelCollidersParent = carController.transform.Find("Wheel Colliders");
                if (wheelCollidersParent != null)
                    Undo.RegisterCreatedObjectUndo(wheelCollidersParent.gameObject, "Create Wheel Colliders Hierarchy");

                if (carController.FrontLeftWheelCollider && carController.FrontLeftWheelTransform) {
                    Undo.RecordObject(carController.FrontLeftWheelCollider, "Assign Front Left Wheel Model");
                    carController.FrontLeftWheelCollider.wheelModel = carController.FrontLeftWheelTransform;
                }

                if (carController.FrontRightWheelCollider && carController.FrontRightWheelTransform) {
                    Undo.RecordObject(carController.FrontRightWheelCollider, "Assign Front Right Wheel Model");
                    carController.FrontRightWheelCollider.wheelModel = carController.FrontRightWheelTransform;
                }

                if (carController.RearLeftWheelCollider && carController.RearLeftWheelTransform) {
                    Undo.RecordObject(carController.RearLeftWheelCollider, "Assign Rear Left Wheel Model");
                    carController.RearLeftWheelCollider.wheelModel = carController.RearLeftWheelTransform;
                }

                if (carController.RearRightWheelCollider && carController.RearRightWheelTransform) {
                    Undo.RecordObject(carController.RearRightWheelCollider, "Assign Rear Right Wheel Model");
                    carController.RearRightWheelCollider.wheelModel = carController.RearRightWheelTransform;
                }

            }

            RCC_WheelCollider[] wheels = selectedRoot.GetComponentsInChildren<RCC_WheelCollider>(true);

            foreach (RCC_WheelCollider wc in wheels) {

                if (wc.WheelCollider) {
                    Undo.RecordObject(wc.transform, "Adjust Wheel Position");
                    wc.transform.position += selectedRoot.transform.up * (wc.WheelCollider.suspensionDistance / 2f);
                }

            }

            Undo.CollapseUndoOperations(rearWheelsGroup);

        }

        toolbarIndex++;

        if (toolbarIndex >= 7) {

            toolbarIndex = 0;
            Close();

        }

    }

    /// <summary>
    /// Resets the wizard back to its first step.
    /// </summary>
    public void ResetToDefault() {

        toolbarIndex = 0;

    }

}
