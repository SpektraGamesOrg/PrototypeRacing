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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

/// <summary>
/// Hosts all menu items for the RCC Editor toolbar under Tools/BoneCracker Games/Realistic Car Controller/ and the matching GameObject/ menu paths.
/// </summary>
public class RCC_EditorWindows : Editor {

    /// <summary>
    /// Returns the RCC_CarControllerV4 on the currently selected GameObject (or one of its parents), or null if no vehicle is selected.
    /// </summary>
    /// <returns>The parent vehicle controller of the active selection, or null when nothing eligible is selected.</returns>
    public static RCC_CarControllerV4 SelectedCar() {

        if (Selection.activeGameObject == null)
            return null;

        return Selection.activeGameObject.GetComponentInParent<RCC_CarControllerV4>(true);

    }

    #region Main Settings (Priority: 0-100)
    /// <summary>
    /// Opens the RCC_Settings asset in the Inspector.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Edit RCC Settings", false, 0)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Edit RCC Settings", false, 0)]
    public static void OpenRCCSettings() {
        Selection.activeObject = RCC_Settings.Instance;
    }
    #endregion

    #region Configuration (Priority: 100-200)
    /// <summary>
    /// Opens the RCC_DemoVehicles asset in the Inspector.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure/Configure Demo Vehicles", false, 100)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Configure/Configure Demo Vehicles", false, 100)]
    public static void OpenDemoVehiclesSettings() {
        Selection.activeObject = RCC_DemoVehicles.Instance;
    }

#if RCC_PHOTON && PHOTON_UNITY_NETWORKING
    /// <summary>
    /// Opens the RCC_PhotonDemoVehicles asset in the Inspector (available only when the RCC_PHOTON and PHOTON_UNITY_NETWORKING defines are set).
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure/Configure Photon Demo Vehicles", false, 101)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Configure/Configure Photon Demo Vehicles", false, 101)]
    public static void OpenPhotonDemoVehiclesSettings() {
        Selection.activeObject = RCC_PhotonDemoVehicles.Instance;
    }
#endif

    /// <summary>
    /// Opens the RCC_GroundMaterials asset in the Inspector.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure/Configure Ground Materials", false, 110)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Configure/Configure Ground Materials", false, 110)]
    public static void OpenGroundMaterialsSettings() {
        Selection.activeObject = RCC_GroundMaterials.Instance;
    }

    /// <summary>
    /// Opens the RCC_ChangableWheels asset in the Inspector.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure/Configure Changable Wheels", false, 111)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Configure/Configure Changable Wheels", false, 111)]
    public static void OpenChangableWheelSettings() {
        Selection.activeObject = RCC_ChangableWheels.Instance;
    }

    /// <summary>
    /// Opens the RCC_Records asset in the Inspector.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure/Configure Recorded Clips", false, 112)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Configure/Configure Recorded Clips", false, 112)]
    public static void OpenRecordSettings() {
        Selection.activeObject = RCC_Records.Instance;
    }

    /// <summary>
    /// Opens the RCC_InitialSettings asset in the Inspector.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure/Configure Initial Vehicle Setup Settings", false, 113)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Configure/Configure Initial Vehicle Setup Settings", false, 113)]
    public static void OpenInitialSettings() {
        Selection.activeObject = RCC_InitialSettings.Instance;
    }
    #endregion

    #region Create Tools (Priority: 200-600)

    #region Managers (Priority: 200-250)
    /// <summary>
    /// Ensures the RCC_SceneManager singleton exists in the active scene and selects its GameObject.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Managers/Add RCC Scene Manager", false, 200)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Managers/Add RCC Scene Manager", false, 200)]
    public static void AddRCCSceneManager() {

        if (RCC_SceneManager.Instance)
            Selection.activeObject = RCC_SceneManager.Instance.gameObject;

    }

    /// <summary>
    /// Ensures the RCC_SkidmarksManager singleton exists in the active scene and selects its GameObject.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Managers/Add RCC Skidmarks Manager", false, 201)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Managers/Add RCC Skidmarks Manager", false, 201)]
    public static void AddRCCSkidmarksManager() {

        if (RCC_SkidmarksManager.Instance)
            Selection.activeObject = RCC_SkidmarksManager.Instance.gameObject;

    }

    /// <summary>
    /// Ensures the RCC_CustomizationManager singleton exists in the active scene and selects its GameObject.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Managers/Add RCC Customization Manager", false, 202)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Managers/Add RCC Customization Manager", false, 202)]
    public static void AddCustomizationManager() {

        if (RCC_CustomizationManager.Instance)
            Selection.activeObject = RCC_CustomizationManager.Instance.gameObject;

    }
    #endregion

    #region Cameras (Priority: 300-350)
    /// <summary>
    /// Instantiates the RCC main camera prefab into the active scene, or selects the existing one if a camera is already present.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add RCC Camera To Scene", false, 300)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add RCC Camera To Scene", false, 300)]
    public static void CreateRCCCamera() {

#if !UNITY_2022_1_OR_NEWER

        RCC_Camera existingCam = FindObjectOfType<RCC_Camera>();

#else

        RCC_Camera existingCam = FindFirstObjectByType<RCC_Camera>();

#endif

        if (existingCam) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Scene has RCC Camera already!", "Scene has RCC Camera already!", "Close");
            Selection.activeGameObject = existingCam.gameObject;

        } else if (RCC_Settings.Instance && RCC_Settings.Instance.RCCMainCamera) {

            GameObject cam = Instantiate(RCC_Settings.Instance.RCCMainCamera.gameObject);
            cam.name = RCC_Settings.Instance.RCCMainCamera.name;
            Selection.activeGameObject = cam.gameObject;

            Undo.RegisterCreatedObjectUndo(cam, "Create RCC Camera");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

    }

    /// <summary>
    /// Instantiates the hood camera prefab as a child of the selected vehicle and connects its ConfigurableJoint to the vehicle Rigidbody.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Hood Camera To Vehicle", false, 301)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Hood Camera To Vehicle", false, 301)]
    public static void CreateHoodCamera() {

        RCC_CarControllerV4 car = SelectedCar();

        if (car == null) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            RCC_HoodCamera existingHoodCam = car.gameObject.GetComponentInChildren<RCC_HoodCamera>();

            if (existingHoodCam) {

                EditorUtility.DisplayDialog("Realistic Car Controller | Your Vehicle Has Hood Camera Already!", "Your vehicle has hood camera already!", "Close");
                Selection.activeGameObject = existingHoodCam.gameObject;
                return;

            }

            if (!RCC_Settings.Instance || !RCC_Settings.Instance.hoodCamera)
                return;

            GameObject hoodCam = (GameObject)Instantiate(RCC_Settings.Instance.hoodCamera, car.transform.position, car.transform.rotation);
            hoodCam.name = RCC_Settings.Instance.hoodCamera.name;
            hoodCam.transform.SetParent(car.transform, true);

            ConfigurableJoint joint = hoodCam.GetComponent<ConfigurableJoint>();

            if (joint)
                joint.connectedBody = car.gameObject.GetComponent<Rigidbody>();

            Selection.activeGameObject = hoodCam;

            Undo.RegisterCreatedObjectUndo(hoodCam, "Create Hood Camera");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

    }

    /// <summary>
    /// Validates the "Add Hood Camera To Vehicle" menu item.
    /// </summary>
    /// <returns>True when exactly one active GameObject is selected.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Hood Camera To Vehicle", true)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Hood Camera To Vehicle", true)]
    public static bool CheckCreateHoodCamera() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }

    /// <summary>
    /// Creates a WheelCamera GameObject with an RCC_WheelCamera component as a child of the selected vehicle.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Wheel Camera To Vehicle", false, 302)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Wheel Camera To Vehicle", false, 302)]
    public static void CreateWheelCamera() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            if (SelectedCar().gameObject.GetComponentInChildren<RCC_WheelCamera>()) {

                EditorUtility.DisplayDialog("Realistic Car Controller | Your Vehicle Has Wheel Camera Already!", "Your vehicle has wheel camera already!", "Close");
                Selection.activeGameObject = SelectedCar().gameObject.GetComponentInChildren<RCC_WheelCamera>().gameObject;
                return;

            }

            GameObject wheelCam = new GameObject("WheelCamera");
            wheelCam.transform.SetParent(SelectedCar().transform, false);
            wheelCam.AddComponent<RCC_WheelCamera>();
            Selection.activeGameObject = wheelCam;

            Undo.RegisterCreatedObjectUndo(wheelCam, "Create WheelCamera");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

    }

    /// <summary>
    /// Validates the "Add Wheel Camera To Vehicle" menu item.
    /// </summary>
    /// <returns>True when exactly one active GameObject is selected.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Wheel Camera To Vehicle", true)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Wheel Camera To Vehicle", true)]
    public static bool CheckCreateWheelCamera() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }
    #endregion

    #region Lights (Priority: 400-450)
    /// <summary>
    /// Instantiates a configured headlight prefab under a "Lights" container parented to the selected vehicle.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/HeadLight", false, 400)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/HeadLight", false, 400)]
    public static void CreateHeadLight() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            GameObject lightsMain;

            if (!SelectedCar().transform.Find("Lights")) {

                lightsMain = new GameObject("Lights");
                lightsMain.transform.SetParent(SelectedCar().transform, false);

            } else {

                lightsMain = SelectedCar().transform.Find("Lights").gameObject;

            }

            if (!RCC_Settings.Instance || !RCC_Settings.Instance.headLights)
                return;

            GameObject headLight = Instantiate(RCC_Settings.Instance.headLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
            headLight.name = RCC_Settings.Instance.headLights.name;
            headLight.transform.SetParent(lightsMain.transform);
            headLight.transform.localRotation = Quaternion.identity;
            headLight.transform.localPosition = new Vector3(0f, 0f, 2f);
            Selection.activeGameObject = headLight;

            Undo.RegisterCreatedObjectUndo(headLight, "Create Headlight");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

    }

    /// <summary>
    /// Validates the "Add HeadLight" menu item.
    /// </summary>
    /// <returns>True when exactly one active GameObject is selected.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/HeadLight", true)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/HeadLight", true)]
    public static bool CheckHeadLight() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }

    /// <summary>
    /// Instantiates a configured brake light prefab under a "Lights" container parented to the selected vehicle.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Brake", false, 401)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Brake", false, 401)]
    public static void CreateBrakeLight() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            GameObject lightsMain;

            if (!SelectedCar().transform.Find("Lights")) {

                lightsMain = new GameObject("Lights");
                lightsMain.transform.SetParent(SelectedCar().transform, false);

            } else {

                lightsMain = SelectedCar().transform.Find("Lights").gameObject;

            }

            if (!RCC_Settings.Instance || !RCC_Settings.Instance.brakeLights)
                return;

            GameObject brakeLight = Instantiate(RCC_Settings.Instance.brakeLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
            brakeLight.name = RCC_Settings.Instance.brakeLights.name;
            brakeLight.transform.SetParent(lightsMain.transform);
            brakeLight.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            brakeLight.transform.localPosition = new Vector3(0f, 0f, -2f);
            Selection.activeGameObject = brakeLight;

            Undo.RegisterCreatedObjectUndo(brakeLight, "Create Brakelight");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

    }

    /// <summary>
    /// Validates the "Add Brake Light" menu item.
    /// </summary>
    /// <returns>True when exactly one active GameObject is selected.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Brake", true)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Brake", true)]
    public static bool CheckBrakeLight() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }

    /// <summary>
    /// Instantiates a configured reverse light prefab under a "Lights" container parented to the selected vehicle.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Reverse", false, 402)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Reverse", false, 402)]
    public static void CreateReverseLight() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            GameObject lightsMain;

            if (!SelectedCar().transform.Find("Lights")) {

                lightsMain = new GameObject("Lights");
                lightsMain.transform.SetParent(SelectedCar().transform, false);

            } else {

                lightsMain = SelectedCar().transform.Find("Lights").gameObject;

            }

            if (!RCC_Settings.Instance || !RCC_Settings.Instance.reverseLights)
                return;

            GameObject reverseLight = Instantiate(RCC_Settings.Instance.reverseLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
            reverseLight.name = RCC_Settings.Instance.reverseLights.name;
            reverseLight.transform.SetParent(lightsMain.transform);
            reverseLight.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            reverseLight.transform.localPosition = new Vector3(0f, 0f, -2f);
            Selection.activeGameObject = reverseLight;

            Undo.RegisterCreatedObjectUndo(reverseLight, "Create Reverselight");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

    }

    /// <summary>
    /// Validates the "Add Reverse Light" menu item.
    /// </summary>
    /// <returns>True when exactly one active GameObject is selected.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Reverse", true)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Reverse", true)]
    public static bool CheckReverseLight() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }

    /// <summary>
    /// Instantiates a configured indicator light prefab under a "Lights" container parented to the selected vehicle, orienting it forward or rearward based on its local position.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Indicator", false, 403)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Indicator", false, 403)]
    public static void CreateIndicatorLight() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            GameObject lightsMain;

            if (!SelectedCar().transform.Find("Lights")) {

                lightsMain = new GameObject("Lights");
                lightsMain.transform.SetParent(SelectedCar().transform, false);

            } else {

                lightsMain = SelectedCar().transform.Find("Lights").gameObject;

            }

            if (!RCC_Settings.Instance || !RCC_Settings.Instance.indicatorLights)
                return;

            GameObject indicatorLight = Instantiate(RCC_Settings.Instance.indicatorLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
            Vector3 relativePos = SelectedCar().transform.InverseTransformPoint(indicatorLight.transform.position);
            indicatorLight.name = RCC_Settings.Instance.indicatorLights.name;
            indicatorLight.transform.SetParent(lightsMain.transform);

            if (relativePos.z > 0f)
                indicatorLight.transform.localRotation = Quaternion.identity;
            else
                indicatorLight.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            indicatorLight.transform.localPosition = new Vector3(0f, 0f, -2f);
            Selection.activeGameObject = indicatorLight;

            Undo.RegisterCreatedObjectUndo(indicatorLight, "Create Indicatorlight");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

    }

    /// <summary>
    /// Validates the "Add Indicator Light" menu item.
    /// </summary>
    /// <returns>True when exactly one active GameObject is selected.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Indicator", true)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Indicator", true)]
    public static bool CheckIndicatorLight() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }

    /// <summary>
    /// Instantiates a configured interior light prefab under a "Lights" container parented to the selected vehicle.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Interior", false, 404)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Interior", false, 404)]
    public static void CreateInteriorLight() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            GameObject lightsMain;

            if (!SelectedCar().transform.Find("Lights")) {

                lightsMain = new GameObject("Lights");
                lightsMain.transform.SetParent(SelectedCar().transform, false);

            } else {

                lightsMain = SelectedCar().transform.Find("Lights").gameObject;

            }

            if (!RCC_Settings.Instance || !RCC_Settings.Instance.interiorLights)
                return;

            GameObject interiorLight = Instantiate(RCC_Settings.Instance.interiorLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
            interiorLight.name = RCC_Settings.Instance.interiorLights.name;
            interiorLight.transform.SetParent(lightsMain.transform);
            interiorLight.transform.localRotation = Quaternion.identity;
            interiorLight.transform.localPosition = Vector3.zero;
            Selection.activeGameObject = interiorLight;

            Undo.RegisterCreatedObjectUndo(interiorLight, "Create Interiorlight");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

    }

    /// <summary>
    /// Validates the "Add Interior Light" menu item.
    /// </summary>
    /// <returns>True when exactly one active GameObject is selected.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Interior", true)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Interior", true)]
    public static bool CheckInteriorLight() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }

    /// <summary>
    /// Duplicates the selected RCC_Light GameObject and mirrors its local X position to place the copy on the opposite side of the vehicle.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Duplicate Selected Light", false, 420)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Duplicate Selected Light", false, 420)]
    public static void DuplicateLight() {

        GameObject duplicatedLight = Instantiate(Selection.activeGameObject);

        duplicatedLight.transform.name = Selection.activeGameObject.transform.name + "_D";
        duplicatedLight.transform.SetParent(Selection.activeGameObject.transform.parent);
        duplicatedLight.transform.localPosition = new Vector3(-Selection.activeGameObject.transform.localPosition.x, Selection.activeGameObject.transform.localPosition.y, Selection.activeGameObject.transform.localPosition.z);
        duplicatedLight.transform.localRotation = Selection.activeGameObject.transform.localRotation;
        duplicatedLight.transform.localScale = Selection.activeGameObject.transform.localScale;

        Selection.activeGameObject = duplicatedLight;

        Undo.RegisterCreatedObjectUndo(duplicatedLight, "Duplicated light");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    }

    /// <summary>
    /// Validates the "Duplicate Selected Light" menu item.
    /// </summary>
    /// <returns>True when exactly one active GameObject containing an RCC_Light component is selected.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Duplicate Selected Light", true)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Lights/Duplicate Selected Light", true)]
    public static bool CheckDuplicateLight() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        if (!Selection.activeGameObject.GetComponent<RCC_Light>())
            return false;

        return true;

    }
    #endregion

    #region UI (Priority: 500-550)
    /// <summary>
    /// Instantiates the RCC Canvas prefab into the active scene, or selects the existing one if a dashboard canvas is already present.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/UI/Add RCC Canvas To Scene", false, 500)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/UI/Add RCC Canvas To Scene", false, 500)]
    public static void CreateRCCCanvas() {

#if !UNITY_2022_1_OR_NEWER

        RCC_DashboardInputs existingDashboard = FindObjectOfType<RCC_DashboardInputs>();

#else

        RCC_DashboardInputs existingDashboard = FindFirstObjectByType<RCC_DashboardInputs>();

#endif

        if (existingDashboard) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Scene has RCC Canvas already!", "Scene has RCC Canvas already!", "Close");
            Selection.activeGameObject = existingDashboard.gameObject;

        } else if (RCC_Settings.Instance && RCC_Settings.Instance.RCCCanvas) {

            GameObject canvas = Instantiate(RCC_Settings.Instance.RCCCanvas);
            canvas.name = RCC_Settings.Instance.RCCCanvas.name;
            Selection.activeGameObject = canvas;

            Undo.RegisterCreatedObjectUndo(canvas, "Create RCC Canvas");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

    }
    #endregion

    #region Misc (Priority: 550-600)
    /// <summary>
    /// Instantiates the configured exhaust prefab under an "Exhausts" container parented to the selected vehicle.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Exhaust To Vehicle", false, 550)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Exhaust To Vehicle", false, 550)]
    public static void CreateExhaust() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            GameObject exhaustsMain;

            if (!SelectedCar().transform.Find("Exhausts")) {
                exhaustsMain = new GameObject("Exhausts");
                exhaustsMain.transform.SetParent(SelectedCar().transform, false);
            } else {
                exhaustsMain = SelectedCar().transform.Find("Exhausts").gameObject;
            }

            if (!RCC_Settings.Instance || !RCC_Settings.Instance.exhaustGas)
                return;

            GameObject exhaust = (GameObject)Instantiate(RCC_Settings.Instance.exhaustGas, SelectedCar().transform.position, SelectedCar().transform.rotation * Quaternion.Euler(0f, 180f, 0f));
            exhaust.name = RCC_Settings.Instance.exhaustGas.name;
            exhaust.transform.SetParent(exhaustsMain.transform);
            exhaust.transform.localPosition = new Vector3(1f, 0f, -2f);
            Selection.activeGameObject = exhaust;

            Undo.RegisterCreatedObjectUndo(exhaust, "Create Exhaust");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

    }

    /// <summary>
    /// Validates the "Add Exhaust To Vehicle" menu item.
    /// </summary>
    /// <returns>True when exactly one active GameObject is selected.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Exhaust To Vehicle", true)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Exhaust To Vehicle", true)]
    public static bool CheckCreateExhaust() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }

    /// <summary>
    /// Adds the configured mirror prefab to the selected vehicle by delegating to <see cref="CreateMirrors(GameObject)"/>.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Mirrors To Vehicle", false, 551)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Mirrors To Vehicle", false, 551)]
    public static void CreateMirrorsMenuItem() {

        if (SelectedCar() == null)
            EditorUtility.DisplayDialog("Realistic Car Controller | Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");
        else
            CreateMirrors(SelectedCar().gameObject);

    }

    /// <summary>
    /// Validates the "Add Mirrors To Vehicle" menu item.
    /// </summary>
    /// <returns>True when exactly one active GameObject is selected.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Mirrors To Vehicle", true)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Mirrors To Vehicle", true)]
    public static bool CheckCreateMirrorsMenuItem() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }
    #endregion

    #endregion

    #region AI Tools (Priority: 700-800)
    /// <summary>
    /// Adds an RCC_AICarController component to the selected vehicle's RCC_CarControllerV4.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/AI/Add AI Controller To Vehicle", false, 700)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/AI/Add AI Controller To Vehicle", false, 700)]
    static void CreateAIBehavior() {

        if (!Selection.activeGameObject)
            return;

        RCC_CarControllerV4 carController = Selection.activeGameObject.GetComponentInParent<RCC_CarControllerV4>(true);

        if (!carController) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Your Vehicle Has Not RCC_CarControllerV4", "Your Vehicle Has Not RCC_CarControllerV4.", "Close");
            return;

        }

        if (Selection.activeGameObject.GetComponentInParent<RCC_AICarController>(true)) {

            EditorUtility.DisplayDialog("Realistic Car Controller | Your Vehicle Already Has AI Car Controller", "Your Vehicle Already Has AI Car Controller", "Close");
            return;

        }

        RCC_AICarController ai = carController.gameObject.AddComponent<RCC_AICarController>();
        GameObject vehicle = carController.gameObject;
        Selection.activeGameObject = vehicle;

        Undo.RegisterCreatedObjectUndo(ai, "Add AI Controller");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    }

    /// <summary>
    /// Validates the "Add AI Controller To Vehicle" menu item.
    /// </summary>
    /// <returns>True when exactly one active GameObject is selected.</returns>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/AI/Add AI Controller To Vehicle", true)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/AI/Add AI Controller To Vehicle", true)]
    static bool CheckAIBehavior() {

        if (Selection.gameObjects.Length > 1 || !Selection.activeTransform)
            return false;
        else
            return true;

    }

    /// <summary>
    /// Creates a new "Waypoints Container" GameObject with an RCC_AIWaypointsContainer component in the active scene.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/AI/Add Waypoints Container To Scene", false, 720)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/AI/Add Waypoints Container To Scene", false, 720)]
    static void CreateWaypointsContainer() {

        GameObject wp = new GameObject("Waypoints Container");
        wp.transform.position = Vector3.zero;
        wp.transform.rotation = Quaternion.identity;
        wp.AddComponent<RCC_AIWaypointsContainer>();
        Selection.activeGameObject = wp;

        Undo.RegisterCreatedObjectUndo(wp, "Create Waypoints Container");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    }

    /// <summary>
    /// Creates a new "Brake Zones Container" GameObject with an RCC_AIBrakeZonesContainer component in the active scene, or warns if one already exists.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/AI/Add BrakeZones Container To Scene", false, 721)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/AI/Add BrakeZones Container To Scene", false, 721)]
    static void CreateBrakeZonesContainer() {

#if !UNITY_2022_1_OR_NEWER

        if (FindObjectOfType<RCC_AIBrakeZonesContainer>() == null) {

            GameObject bz = new GameObject("Brake Zones Container");
            bz.transform.position = Vector3.zero;
            bz.transform.rotation = Quaternion.identity;
            bz.AddComponent<RCC_AIBrakeZonesContainer>();
            Selection.activeGameObject = bz;

            Undo.RegisterCreatedObjectUndo(bz, "Create Brake Zones Container");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        } else {

            EditorUtility.DisplayDialog("Realistic Car Controller | Your Scene Already Has Brake Zones Container", "Your Scene Already Has Brake Zones", "Close");

        }

#else

        if (FindFirstObjectByType<RCC_AIBrakeZonesContainer>() == null) {

            GameObject bz = new GameObject("Brake Zones Container");
            bz.transform.position = Vector3.zero;
            bz.transform.rotation = Quaternion.identity;
            bz.AddComponent<RCC_AIBrakeZonesContainer>();
            Selection.activeGameObject = bz;

            Undo.RegisterCreatedObjectUndo(bz, "Create Brake Zones Container");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        } else {

            EditorUtility.DisplayDialog("Realistic Car Controller | Your Scene Already Has Brake Zones Container", "Your Scene Already Has Brake Zones", "Close");

        }

#endif

    }
    #endregion

    #region Tools (Priority: 900-1000)
    /// <summary>
    /// Opens the Render Pipeline Converter window for migrating RCC materials between Built-in, URP, and HDRP.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Render Pipeline Converter", false, 900)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Render Pipeline Converter", false, 900)]
    public static void PipelineConverter() {

        RCC_RenderPipelineConverterWindow.ShowWindow();

    }
    #endregion

    /// <summary>
    /// Opens the Script Header Checker window that scans RCC scripts for the required copyright header.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Other/Script Header Checker", false, 1000)]
    public static void ScriptHeaderChecker() {

        RCC_ScriptHeaderCheckerWindow.ShowWindow();

    }

    /// <summary>
    /// Manually triggers the Input System package version check.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Other/Input System Version Checker", false, 1001)]
    public static void InputSystemVersionChecker() {

        RCC_InputSystemVersionChecker.ManualCheck();

    }

    #region Help and Upgrade (Priority: 2000+)
    /// <summary>
    /// Opens the Realistic Car Controller Pro listing on the Unity Asset Store in the user's browser.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Upgrade to Realistic Car Controller Pro", false, 2000)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Upgrade to Realistic Car Controller Pro", false, 2000)]
    public static void Pro() {

        string url = "http://u3d.as/22Bf";
        Application.OpenURL(url);

    }

    /// <summary>
    /// Shows a contact information dialog and opens the BoneCracker Games support page in the user's browser.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Help", false, 2001)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller/Help", false, 2001)]
    public static void Help() {

        EditorUtility.DisplayDialog("Realistic Car Controller | Contact", "Please include your invoice number while sending a contact form.", "Close");

        string url = "http://www.bonecrackergames.com/contact/";
        Application.OpenURL(url);

    }
    #endregion

    #region Static Methods
    /// <summary>
    /// Instantiates the configured mirror prefab onto the given vehicle, or warns if the vehicle already has mirrors.
    /// </summary>
    /// <param name="vehicle">The vehicle GameObject (must have an RCC_CarControllerV4 component) to receive the mirrors.</param>
    public static void CreateMirrors(GameObject vehicle) {

        if (!vehicle.transform.GetComponentInChildren<RCC_Mirror>()) {

            if (!RCC_Settings.Instance || !RCC_Settings.Instance.mirrors)
                return;

            RCC_CarControllerV4 carController = vehicle.GetComponent<RCC_CarControllerV4>();

            if (!carController)
                return;

            GameObject mirrors = (GameObject)Instantiate(RCC_Settings.Instance.mirrors, vehicle.transform.position, vehicle.transform.rotation);
            mirrors.transform.SetParent(carController.transform, true);
            mirrors.name = "Mirrors";
            Selection.activeGameObject = mirrors;
            EditorUtility.DisplayDialog("Realistic Car Controller | Created Mirrors!", "Created mirrors. Adjust their positions.", "Close");

            Undo.RegisterCreatedObjectUndo(mirrors, "Create Mirrors");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        } else {

            EditorUtility.DisplayDialog("Realistic Car Controller | Vehicle Has Mirrors Already", "Vehicle has mirrors already!", "Close");

        }

    }
    #endregion

}