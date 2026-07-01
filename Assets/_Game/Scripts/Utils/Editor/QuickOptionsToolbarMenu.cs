using System;
using System.Reflection;
using Clutch;
using Save;
using Sirenix.OdinValidator.Editor;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using Vehicles;

namespace Utils.Editor.QuickOptionsToolbarMenu
{
    public static class QuickOptionsToolbarMenu
    {
        private const string QuickOptionsPath = "SpektraGames/QuickOptionsToolbarMenu";

        [MainToolbarElement(QuickOptionsPath, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement CreateQuickOptionsMenu()
        {
            var icon = EditorGUIUtility.IconContent("d_Settings").image as Texture2D;
            var content = new MainToolbarContent("Quick Options", icon, "Quick Options Menu");

            return new MainToolbarButton(content, ShowQuickOptionsMenu);
        }

        // Dock in the Middle zone before Unity's Play Mode Controls so the button sits immediately to the
        // left of the Play/Pause/Step buttons.
        [MainToolbarElement("SpektraGames/SelectSaveHelper",
            defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = 0)]
        public static MainToolbarElement CreateSelectSaveHelperButton()
        {
            var icon = EditorGUIUtility.IconContent("d_Search Icon").image as Texture2D;
            var content = new MainToolbarContent(icon, "Select SaveHelper in scene");

            return new MainToolbarButton(content, SelectSaveHelper);
        }

        private static void SelectSaveHelper()
        {
            // Editor-only toolbar code, so scene scanning is fine here. Works in both edit and play mode.
            var saveHelper = UnityEngine.Object.FindFirstObjectByType<SaveHelper>(FindObjectsInactive.Include);
            if (!saveHelper)
            {
                Debug.LogError("SaveHelper was not found in the active scene.");
                return;
            }

            Selection.activeGameObject = saveHelper.gameObject;
            EditorGUIUtility.PingObject(saveHelper.gameObject);
        }

        private static void ShowQuickOptionsMenu()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Vehicle Container"), false, () => { Selection.activeObject = VehicleContainer.Instance; });
            menu.AddItem(new GUIContent("Clutch Config"), false, () => { Selection.activeObject = ClutchConfig.Instance; });
            menu.AddItem(new GUIContent("Event Object Container"), false, () => { Selection.activeObject = Events.EventObjectContainer.Instance; });

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Open Level Designer"), false, Events.Editor.LevelDesignerWindow.ShowWindow);
            menu.AddSeparator(string.Empty);

            menu.AddItem(new GUIContent("Run Custom Odin Validator"), false, RunOdinCustomValidator);

            menu.ShowAsContext();
        }

        private static void RunOdinCustomValidator()
        {
            var profile = AssetDatabase.LoadAssetAtPath<ValidationProfile>(
                "Assets/Plugins/Sirenix/Odin Validator/Editor/Profiles/CustomGameValidationProfile.asset");

            ValidationSessionEditor validationSessionEditor = OdinValidatorWindow.OpenWindow(profile);
            OdinValidatorWindow window = validationSessionEditor.Window;

            FieldInfo handleFieldInfo = typeof(OdinValidatorWindow).GetField("handle",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default);
            if (handleFieldInfo == null)
            {
                Debug.LogError("handleFieldInfo is null");
                return;
            }

            if (handleFieldInfo.GetValue(window) == null)
            {
                Debug.LogError("handle is null");
                return;
            }

            ValidationSessionAssetHandle handle =
                handleFieldInfo.GetValue(window) as ValidationSessionAssetHandle;
            ValidationSession session = handle.Session;

            MethodInfo validateEverythingNowMethodInfo =
                typeof(ValidationSession).GetMethod("ValidateEverythingNow",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default);
            if (validateEverythingNowMethodInfo == null)
            {
                Debug.LogError("validateEverythingNowMethodInfo is null");
                return;
            }

            try
            {
                validateEverythingNowMethodInfo.Invoke(session, new object[] { true, true });
                SceneView.RepaintAll();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}