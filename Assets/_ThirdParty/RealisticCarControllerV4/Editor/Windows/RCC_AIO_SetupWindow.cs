//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor window that automatically sets up the AIO demo scene selector prefab
/// with category headers, tooltip system, level button descriptions, and controls panel.
/// </summary>
public class RCC_AIO_SetupWindow : EditorWindow {

    /// <summary>
    /// Prefab path for the AIO canvas.
    /// </summary>
    private const string PrefabPath = "Assets/RealisticCarControllerV4/Prefabs/UI/RCC_Canvas (AIO).prefab";

    /// <summary>
    /// Scene descriptions for each level button, keyed by toggle GameObject name.
    /// </summary>
    private static readonly Dictionary<string, string> LevelDescriptions = new Dictionary<string, string>() {
        { "RCC City", "Drive freely in the city with traffic and AI vehicles." },
        { "RCC City Car Selection", "Browse and select vehicles before driving in the city." },
        { "RCC Multiple Terrain", "Drive across different surfaces: asphalt, grass, sand, gravel." },
        { "RCC Damage", "Crash testing with obstacles: hammers, presses, and shredders." },
        { "RCC Blank API", "Demonstrates spawning, registering, and controlling vehicles via code." },
        { "RCC Blank Override Inputs", "Shows how to override vehicle inputs programmatically." },
        { "RCC Blank Customization", "Vehicle customization: paint, wheels, spoilers, upgrades, decals, neons." },
        { "RCC Blank", "Minimal empty scene with a single vehicle for quick testing." },
        { "RCC City Photon2", "Photon multiplayer city gameplay. Requires PUN2 package." },
        { "RCC City Photon2 City", "Photon multiplayer city gameplay. Requires PUN2 package." },
        { "RCC Photon2 Lobby", "Photon multiplayer lobby. Requires PUN2 package." },
        { "RCC City Enter/Exit FPS", "First-person enter/exit vehicles. Requires BCG Shared Assets." },
        { "RCC City Enter/Exit TPS", "Third-person enter/exit vehicles. Requires BCG Shared Assets." },
    };

    /// <summary>
    /// Group renaming map: sibling index in Background → new name.
    /// </summary>
    private static readonly string[] GroupNames = {
        "Group_City",
        "Group_Sandbox",
        "Group_Photon",
        "Group_EnterExit"
    };

    /// <summary>
    /// Category header labels for each group.
    /// </summary>
    private static readonly string[] GroupHeaders = {
        "CITY SCENES",
        "SANDBOX / API",
        "PHOTON MULTIPLAYER",
        "ENTER / EXIT"
    };

    /// <summary>
    /// Keyboard controls reference text.
    /// </summary>
    private const string ControlsText =
        "<b>DRIVING</b>\n" +
        "W / Up Arrow\t\tThrottle\n" +
        "S / Down Arrow\t\tBrake\n" +
        "A / Left Arrow\t\tSteer Left\n" +
        "D / Right Arrow\t\tSteer Right\n" +
        "Space\t\t\tHandbrake\n" +
        "F\t\t\tNOS / Boost\n" +
        "\n" +
        "<b>ENGINE & GEARS</b>\n" +
        "I\t\t\tStart / Stop Engine\n" +
        "Left Shift\t\tGear Up\n" +
        "Left Ctrl\t\tGear Down\n" +
        "N\t\t\tNeutral\n" +
        "\n" +
        "<b>LIGHTS & SIGNALS</b>\n" +
        "L\t\t\tLow Beam Lights\n" +
        "K\t\t\tHigh Beam Lights\n" +
        "M\t\t\tInterior Lights\n" +
        "Q\t\t\tLeft Indicator\n" +
        "E\t\t\tRight Indicator\n" +
        "Z\t\t\tHazard Lights\n" +
        "\n" +
        "<b>CAMERA & EXTRAS</b>\n" +
        "C\t\t\tChange Camera\n" +
        "B\t\t\tLook Back\n" +
        "T\t\t\tTrailer Detach\n" +
        "R / P / G\t\tRecord / Replay / SlowMo";

    /// <summary>
    /// Opens this editor window.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Setup AIO Demo Selector", false, 500)]
    public static void ShowWindow() {

        RCC_AIO_SetupWindow window = GetWindow<RCC_AIO_SetupWindow>("AIO Setup");
        window.minSize = new Vector2(400, 300);
        window.Show();

    }

    private void OnGUI() {

        GUILayout.Space(10);
        EditorGUILayout.LabelField("AIO Demo Scene Selector Setup", EditorStyles.boldLabel);
        GUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "This tool will modify the RCC_Canvas (AIO) prefab to add:\n\n" +
            "1. Category headers for each button group\n" +
            "2. Tooltip system (hover descriptions on level buttons)\n" +
            "3. Controls panel (keyboard reference overlay)\n" +
            "4. Renamed groups for clarity\n\n" +
            "The prefab will be modified in place. Use Ctrl+Z to undo if needed.",
            MessageType.Info);

        GUILayout.Space(10);

        //  Check if prefab exists.
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

        if (prefab == null) {

            EditorGUILayout.HelpBox("Could not find AIO Canvas prefab at:\n" + PrefabPath, MessageType.Error);
            return;

        }

        EditorGUILayout.LabelField("Prefab found:", PrefabPath);

        GUILayout.Space(10);

        if (GUILayout.Button("Setup AIO Prefab", GUILayout.Height(40))) {

            SetupAIOPrefab(prefab);

        }

    }

    /// <summary>
    /// Performs the full setup on the AIO canvas prefab.
    /// </summary>
    private static void SetupAIOPrefab(GameObject prefab) {

        //  Open prefab contents for editing.
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

        if (prefabRoot == null) {

            Debug.LogError("[RCC_AIO_Setup] Failed to load prefab contents.");
            return;

        }

        bool success = false;

        try {

            //  Find key objects.
            Transform levelsTransform = prefabRoot.transform.Find("Levels");
            Transform quitTransform = prefabRoot.transform.Find("Quit");

            if (levelsTransform == null) {

                Debug.LogError("[RCC_AIO_Setup] 'Levels' object not found in prefab.");
                return;

            }

            //  Find Background inside Levels.
            Transform background = levelsTransform.Find("Background");

            if (background == null) {

                Debug.LogError("[RCC_AIO_Setup] 'Background' not found inside Levels.");
                return;

            }

            //  Step 1: Rename groups.
            RenameGroups(background);

            //  Step 2: Add category headers to each group.
            AddCategoryHeaders(background);

            //  Step 3: Add RCC_UI_LevelButton to all toggle buttons.
            AddLevelButtons(background);

            //  Step 4: Add tooltip TMP below Background.
            AddTooltip(levelsTransform);

            //  Step 5: Add Controls button and panel.
            AddControlsSystem(prefabRoot.transform, quitTransform);

            //  Save prefab.
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
            success = true;

            Debug.Log("[RCC_AIO_Setup] AIO prefab setup complete!");

        } finally {

            PrefabUtility.UnloadPrefabContents(prefabRoot);

        }

        if (success)
            EditorUtility.DisplayDialog("AIO Setup Complete", "The AIO Canvas prefab has been updated successfully.\n\nChanges:\n- Groups renamed\n- Category headers added\n- Tooltip system added\n- Level button descriptions added\n- Controls panel added", "OK");

    }

    /// <summary>
    /// Renames the 4 group containers inside Background by sibling index.
    /// </summary>
    private static void RenameGroups(Transform background) {

        for (int i = 0; i < background.childCount && i < GroupNames.Length; i++) {

            Transform child = background.GetChild(i);

            //  Skip if already renamed.
            if (child.name == GroupNames[i])
                continue;

            child.name = GroupNames[i];
            Debug.Log("[RCC_AIO_Setup] Renamed group " + i + " to: " + GroupNames[i]);

        }

    }

    /// <summary>
    /// Adds a category header TMP text as the first child of each group.
    /// </summary>
    private static void AddCategoryHeaders(Transform background) {

        for (int i = 0; i < background.childCount && i < GroupHeaders.Length; i++) {

            Transform group = background.GetChild(i);

            //  Skip if header already exists.
            if (group.Find("Header") != null) {

                Debug.Log("[RCC_AIO_Setup] Header already exists in " + group.name + ", skipping.");
                continue;

            }

            //  Create header GameObject.
            GameObject headerObj = new GameObject("Header");
            headerObj.layer = 5; //  UI layer

            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerObj.AddComponent<CanvasRenderer>();
            TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();

            //  Configure text.
            headerText.text = GroupHeaders[i];
            headerText.fontSize = 14;
            headerText.fontStyle = FontStyles.Bold;
            headerText.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.raycastTarget = false;

            //  Parent and move to first child.
            headerRect.SetParent(group, false);
            headerRect.SetAsFirstSibling();

            //  Anchor and size.
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = new Vector2(0, 5);
            headerRect.sizeDelta = new Vector2(0, 25);

            //  Try using the same font as existing TMP texts in the group.
            TextMeshProUGUI existingTMP = group.GetComponentInChildren<TextMeshProUGUI>();

            if (existingTMP != null && existingTMP != headerText)
                headerText.font = existingTMP.font;

            Debug.Log("[RCC_AIO_Setup] Added header '" + GroupHeaders[i] + "' to " + group.name);

        }

    }

    /// <summary>
    /// Adds RCC_UI_LevelButton component to all Toggle buttons found in the hierarchy.
    /// </summary>
    private static void AddLevelButtons(Transform background) {

        Toggle[] toggles = background.GetComponentsInChildren<Toggle>(true);

        foreach (Toggle toggle in toggles) {

            string toggleName = toggle.gameObject.name;

            //  Skip if already has the component.
            if (toggle.GetComponent<RCC_UI_LevelButton>() != null) {

                Debug.Log("[RCC_AIO_Setup] " + toggleName + " already has RCC_UI_LevelButton, skipping.");
                continue;

            }

            //  Add component.
            RCC_UI_LevelButton levelButton = toggle.gameObject.AddComponent<RCC_UI_LevelButton>();

            //  Set description if we have one for this button name.
            if (LevelDescriptions.TryGetValue(toggleName, out string desc))
                levelButton.description = desc;
            else
                Debug.LogWarning("[RCC_AIO_Setup] No description found for toggle: " + toggleName);

            Debug.Log("[RCC_AIO_Setup] Added RCC_UI_LevelButton to: " + toggleName);

        }

    }

    /// <summary>
    /// Adds a tooltip TMP text object below the Background in the Levels panel.
    /// </summary>
    private static void AddTooltip(Transform levelsTransform) {

        //  Skip if already exists.
        if (levelsTransform.Find("Tooltip") != null) {

            Debug.Log("[RCC_AIO_Setup] Tooltip already exists, skipping.");
            return;

        }

        //  Create tooltip GameObject.
        GameObject tooltipObj = new GameObject("Tooltip");
        tooltipObj.layer = 5;

        RectTransform tooltipRect = tooltipObj.AddComponent<RectTransform>();
        tooltipObj.AddComponent<CanvasRenderer>();
        TextMeshProUGUI tooltipText = tooltipObj.AddComponent<TextMeshProUGUI>();
        tooltipObj.AddComponent<RCC_UI_Tooltip>();

        //  Configure text.
        tooltipText.text = "";
        tooltipText.fontSize = 16;
        tooltipText.fontStyle = FontStyles.Italic;
        tooltipText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        tooltipText.alignment = TextAlignmentOptions.Center;
        tooltipText.raycastTarget = false;
        tooltipText.enabled = false;

        //  Try to use the same font as existing TMP texts.
        TextMeshProUGUI existingTMP = levelsTransform.GetComponentInChildren<TextMeshProUGUI>();

        if (existingTMP != null && existingTMP != tooltipText)
            tooltipText.font = existingTMP.font;

        //  Parent to Levels.
        tooltipRect.SetParent(levelsTransform, false);

        //  Anchor to bottom.
        tooltipRect.anchorMin = new Vector2(0, 0);
        tooltipRect.anchorMax = new Vector2(1, 0);
        tooltipRect.pivot = new Vector2(0.5f, 0);
        tooltipRect.anchoredPosition = new Vector2(0, 10);
        tooltipRect.sizeDelta = new Vector2(-40, 40);

        Debug.Log("[RCC_AIO_Setup] Added Tooltip to Levels.");

    }

    /// <summary>
    /// Adds the Controls button (near Quit) and the controls overlay panel.
    /// </summary>
    private static void AddControlsSystem(Transform canvasRoot, Transform quitTransform) {

        //  Skip if already exists.
        if (canvasRoot.Find("ControlsPanel") != null) {

            Debug.Log("[RCC_AIO_Setup] ControlsPanel already exists, skipping.");
            return;

        }

        //  Find a TMP font from existing elements.
        TMP_FontAsset font = null;
        TextMeshProUGUI existingTMP = canvasRoot.GetComponentInChildren<TextMeshProUGUI>(true);

        if (existingTMP != null)
            font = existingTMP.font;

        //  === Create Controls Panel (overlay) ===
        GameObject panelObj = new GameObject("ControlsPanel");
        panelObj.layer = 5;

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelObj.AddComponent<CanvasRenderer>();
        Image panelImage = panelObj.AddComponent<Image>();

        //  Dark semi-transparent background.
        panelImage.color = new Color(0, 0, 0, 0.9f);
        panelImage.raycastTarget = true;

        //  Parent to canvas root.
        panelRect.SetParent(canvasRoot, false);

        //  Center with fixed size.
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(650, 520);

        //  Add title text.
        GameObject titleObj = CreateTMPObject("Title", panelRect, font);
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "KEYBOARD CONTROLS";
        titleText.fontSize = 22;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0, 1, 1, 1); //  Cyan to match RCC style.
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.raycastTarget = false;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -15);
        titleRect.sizeDelta = new Vector2(-40, 35);

        //  Add controls content text.
        GameObject contentObj = CreateTMPObject("Content", panelRect, font);
        TextMeshProUGUI contentText = contentObj.GetComponent<TextMeshProUGUI>();
        contentText.text = ControlsText;
        contentText.fontSize = 15;
        contentText.fontStyle = FontStyles.Normal;
        contentText.color = Color.white;
        contentText.alignment = TextAlignmentOptions.TopLeft;
        contentText.raycastTarget = false;
        contentText.richText = true;
        contentText.enableWordWrapping = false;

        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.offsetMin = new Vector2(30, 20);
        contentRect.offsetMax = new Vector2(-30, -55);

        //  Add close button.
        GameObject closeObj = new GameObject("CloseButton");
        closeObj.layer = 5;

        RectTransform closeRect = closeObj.AddComponent<RectTransform>();
        closeObj.AddComponent<CanvasRenderer>();
        Image closeImage = closeObj.AddComponent<Image>();
        closeImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Button closeButton = closeObj.AddComponent<Button>();

        closeRect.SetParent(panelRect, false);
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-8, -8);
        closeRect.sizeDelta = new Vector2(30, 30);

        //  Close button label.
        GameObject closeLabelObj = CreateTMPObject("Label", closeRect, font);
        TextMeshProUGUI closeLabelText = closeLabelObj.GetComponent<TextMeshProUGUI>();
        closeLabelText.text = "X";
        closeLabelText.fontSize = 18;
        closeLabelText.fontStyle = FontStyles.Bold;
        closeLabelText.color = Color.white;
        closeLabelText.alignment = TextAlignmentOptions.Center;
        closeLabelText.raycastTarget = false;

        RectTransform closeLabelRect = closeLabelObj.GetComponent<RectTransform>();
        closeLabelRect.anchorMin = Vector2.zero;
        closeLabelRect.anchorMax = Vector2.one;
        closeLabelRect.offsetMin = Vector2.zero;
        closeLabelRect.offsetMax = Vector2.zero;

        //  Add RCC_UI_ControlsPanel component to the panel.
        RCC_UI_ControlsPanel controlsPanelComp = panelObj.AddComponent<RCC_UI_ControlsPanel>();
        controlsPanelComp.controlsPanel = panelObj;

        //  Wire close button to close the panel.
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(closeButton.onClick, controlsPanelComp.Close);

        //  Start hidden.
        panelObj.SetActive(false);

        //  === Create Controls Button (near Quit) ===
        if (canvasRoot.Find("ControlsButton") == null) {

            GameObject btnObj = new GameObject("ControlsButton");
            btnObj.layer = 5;

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnObj.AddComponent<CanvasRenderer>();
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0, 1, 1, 1); //  Cyan.
            Button btn = btnObj.AddComponent<Button>();

            //  Set button color block for hover feedback.
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0, 1, 1, 1);
            colors.highlightedColor = new Color(0.5f, 1, 1, 1);
            colors.pressedColor = new Color(0, 0.8f, 0.8f, 1);
            btn.colors = colors;

            btnRect.SetParent(canvasRoot, false);

            //  Position: top-right, offset left from Quit.
            if (quitTransform != null) {

                RectTransform quitRect = quitTransform.GetComponent<RectTransform>();

                if (quitRect != null) {

                    btnRect.anchorMin = quitRect.anchorMin;
                    btnRect.anchorMax = quitRect.anchorMax;
                    btnRect.pivot = quitRect.pivot;
                    btnRect.sizeDelta = quitRect.sizeDelta;

                    //  Place it to the left of Quit button.
                    btnRect.anchoredPosition = quitRect.anchoredPosition + new Vector2(-(quitRect.sizeDelta.x + 10), 0);

                }

            } else {

                //  Fallback position: top-right.
                btnRect.anchorMin = new Vector2(1, 1);
                btnRect.anchorMax = new Vector2(1, 1);
                btnRect.pivot = new Vector2(1, 1);
                btnRect.anchoredPosition = new Vector2(-180, -20);
                btnRect.sizeDelta = new Vector2(150, 50);

            }

            //  Button label.
            GameObject btnLabelObj = CreateTMPObject("Text", btnRect, font);
            TextMeshProUGUI btnLabelText = btnLabelObj.GetComponent<TextMeshProUGUI>();
            btnLabelText.text = "Controls";
            btnLabelText.fontSize = 26;
            btnLabelText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            btnLabelText.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            btnLabelText.alignment = TextAlignmentOptions.Center;
            btnLabelText.raycastTarget = false;

            RectTransform btnLabelRect = btnLabelObj.GetComponent<RectTransform>();
            btnLabelRect.anchorMin = Vector2.zero;
            btnLabelRect.anchorMax = Vector2.one;
            btnLabelRect.offsetMin = Vector2.zero;
            btnLabelRect.offsetMax = Vector2.zero;

            //  Wire button OnClick to toggle the controls panel.
            //  We need to reference the panel that will be active, so we use the component's TogglePanel method.
            //  But since the panel is inactive, we need to reference the component differently.
            //  The button should activate the panel, then the close button deactivates it.
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, controlsPanelComp.TogglePanel);

            Debug.Log("[RCC_AIO_Setup] Added Controls button and panel.");

        }

    }

    /// <summary>
    /// Helper to create a TMP text GameObject with standard UI setup.
    /// </summary>
    private static GameObject CreateTMPObject(string name, RectTransform parent, TMP_FontAsset font) {

        GameObject obj = new GameObject(name);
        obj.layer = 5;

        RectTransform rect = obj.AddComponent<RectTransform>();
        obj.AddComponent<CanvasRenderer>();
        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();

        if (font != null)
            text.font = font;

        rect.SetParent(parent, false);

        return obj;

    }

}
