using System.Collections.Generic;
using System.Linq;
using Core;
using TMPro;
using UIManager;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using EventUI = UI; // the project's UI namespace (event overlays live here)
using EventType = Events.EventType; // disambiguate from UnityEngine.EventType inside this editor file

namespace Events.Editor
{
    /// <summary>
    /// One-shot generator that builds all the Unity-side content for the event system: spawnable-piece prefabs,
    /// the palette + level containers, sample levels, the 5 event overlays (wired into GameUIManager), and the
    /// scene-side EventManager + sample event areas. Placeholder visuals only - meant to make the whole loop
    /// playable immediately; restyle the overlays/pieces afterwards.
    ///
    /// Idempotent-ish: prefabs/assets are overwritten; overlays are skipped if already present so a re-run does
    /// not duplicate them. Editor-only; namespace is descriptive (never "Editor") per project rules.
    /// </summary>
    public static class EventSystemSetup
    {
        private const string PrefabFolder = "Assets/_Game/Prefabs/EventObjects";
        private const string MatFolder = "Assets/_Game/Prefabs/EventObjects/Materials";
        private const string ResourcesFolder = "Assets/_Game/Data/Resources";
        private const string LevelsFolder = "Assets/_Game/Data/Resources/Levels";
        private const string RampSourcePrefab = "Assets/_Game/Art/3D/StuntRacingPack/Prefabs/Ramps01.prefab";
        private const string StarterScene = "Assets/_Game/Scenes/Starter.unity";
        private const string GameScene = "Assets/_Game/Scenes/Game.unity";

        [MenuItem("Tools/EventSystem/Setup Event System (Generate Content)")]
        public static void RunAll()
        {
            EnsureFolder(PrefabFolder);
            EnsureFolder(MatFolder);
            EnsureFolder(ResourcesFolder);
            EnsureFolder(LevelsFolder);

            // 1) Spawnable-piece prefabs.
            LevelObject start = CreateBoxPiece("EventObj_Start", new Vector3(3f, 0.15f, 3f),
                new Color(0.3f, 0.9f, 0.4f), LevelObjectRole.Start, keepCollider: false);
            LevelObject finish = CreateBoxPiece("EventObj_Finish", new Vector3(8f, 5f, 0.4f),
                new Color(0.3f, 0.7f, 1f), LevelObjectRole.Finish, keepCollider: true);
            LevelObject obstacle = CreateBoxPiece("EventObj_Obstacle", new Vector3(2.5f, 1.5f, 0.6f),
                new Color(0.95f, 0.3f, 0.25f), LevelObjectRole.Obstacle, keepCollider: true);
            LevelObject ramp = CreateRampPiece();

            var presets = new List<LevelObject> { start, finish, obstacle };
            if (ramp) presets.Add(ramp);

            // 2) Palette container.
            EventObjectContainer palette = CreateOrLoadAsset<EventObjectContainer>(ResourcesFolder + "/EventObjectContainer.asset");
            SetObjectList(palette, "presets", presets.Cast<Object>().ToList());
            EditorUtility.SetDirty(palette);

            // 3) Sample levels.
            LevelData jump = CreateJumpLevel(start, ramp, obstacle, finish);
            LevelData timeTrial = CreateTimeTrialLevel(start, finish);

            // 4) Level container.
            EventLevelContainer levels = CreateOrLoadAsset<EventLevelContainer>(ResourcesFolder + "/EventLevelContainer.asset");
            SetObjectList(levels, "jumpChallengeLevels", new List<Object> { jump });
            SetObjectList(levels, "timeTrialLevels", new List<Object> { timeTrial });
            EditorUtility.SetDirty(levels);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 5) Event-area prefab.
            GameObject areaPrefab = CreateEventAreaPrefab();

            // 6) Overlays (Starter scene, under GameUIManager) + 7) EventManager & sample areas (Game scene).
            BuildOverlaysInStarter();
            BuildSceneContentInGame(areaPrefab);

            AssetDatabase.SaveAssets();
            Debug.Log("[EventSystemSetup] Done. Palette, levels, prefabs, overlays, EventManager and sample areas generated.");
        }

        // -----------------------------------------------------------------
        // Prefabs
        // -----------------------------------------------------------------

        private static LevelObject CreateBoxPiece(string name, Vector3 scale, Color color, LevelObjectRole role, bool keepCollider)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            temp.name = name;
            temp.transform.localScale = scale;

            Collider col = temp.GetComponent<Collider>();
            if (!keepCollider && col)
                Object.DestroyImmediate(col);

            var renderer = temp.GetComponent<MeshRenderer>();
            if (renderer)
                renderer.sharedMaterial = MakeMaterial(name + "_Mat", color);

            var lo = temp.AddComponent<LevelObject>();
            SetEnum(lo, "role", (int)role);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, PrefabFolder + "/" + name + ".prefab");
            Object.DestroyImmediate(temp);
            return prefab.GetComponent<LevelObject>();
        }

        private static LevelObject CreateRampPiece()
        {
            var rampSrc = AssetDatabase.LoadAssetAtPath<GameObject>(RampSourcePrefab);
            if (!rampSrc)
            {
                Debug.LogError($"[EventSystemSetup] Ramp source prefab not found at '{RampSourcePrefab}'. Skipping ramp.");
                return null;
            }

            var temp = (GameObject)PrefabUtility.InstantiatePrefab(rampSrc);
            PrefabUtility.UnpackPrefabInstance(temp, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            temp.name = "EventObj_Ramp";

            // Ensure the ramp is drivable: give every mesh a collider if it lacks one.
            foreach (MeshFilter mf in temp.GetComponentsInChildren<MeshFilter>())
            {
                if (mf.sharedMesh && !mf.GetComponent<Collider>())
                {
                    var mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                }
            }

            var lo = temp.AddComponent<LevelObject>();
            SetEnum(lo, "role", (int)LevelObjectRole.Prop);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, PrefabFolder + "/EventObj_Ramp.prefab");
            Object.DestroyImmediate(temp);
            return prefab.GetComponent<LevelObject>();
        }

        private static GameObject CreateEventAreaPrefab()
        {
            var temp = new GameObject("EventArea");
            var box = temp.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, 2f, 0f);
            box.size = new Vector3(8f, 4f, 8f);
            temp.AddComponent<EventArea>();
            temp.AddComponent<Minimap.MinimapMarker>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, PrefabFolder + "/EventArea.prefab");
            Object.DestroyImmediate(temp);
            return prefab;
        }

        // -----------------------------------------------------------------
        // Levels
        // -----------------------------------------------------------------

        private static LevelData CreateJumpLevel(LevelObject start, LevelObject ramp, LevelObject obstacle, LevelObject finish)
        {
            LevelData level = CreateOrLoadAsset<LevelData>(LevelsFolder + "/Jump_01.asset");
            SetEnum(level, "eventType", (int)EventType.JumpChallenge);
            SetInt(level, "levelNumber", 1);
            SetInt(level, "winRewardGold", 2000);

            var placements = new List<LevelPlacement>
            {
                new LevelPlacement(start, new Vector3(0f, 0f, 0f), Vector3.zero, Vector3.one),
                new LevelPlacement(obstacle, new Vector3(0f, 0.75f, 26f), Vector3.zero, Vector3.one),
                new LevelPlacement(finish, new Vector3(0f, 2.5f, 34f), Vector3.zero, Vector3.one),
            };
            if (ramp)
                placements.Insert(1, new LevelPlacement(ramp, new Vector3(0f, 0f, 14f), Vector3.zero, Vector3.one));

            level.EditorSetPlacements(placements);
            EditorUtility.SetDirty(level);
            return level;
        }

        private static LevelData CreateTimeTrialLevel(LevelObject start, LevelObject finish)
        {
            LevelData level = CreateOrLoadAsset<LevelData>(LevelsFolder + "/TimeTrial_01.asset");
            SetEnum(level, "eventType", (int)EventType.TimeTrial);
            SetInt(level, "levelNumber", 1);
            SetInt(level, "winRewardGold", 2000);
            SetFloat(level, "timeLimitSeconds", 20f);

            var placements = new List<LevelPlacement>
            {
                new LevelPlacement(start, new Vector3(0f, 0f, 0f), Vector3.zero, Vector3.one),
                new LevelPlacement(finish, new Vector3(0f, 2.5f, 60f), Vector3.zero, Vector3.one),
            };
            level.EditorSetPlacements(placements);
            EditorUtility.SetDirty(level);
            return level;
        }

        // -----------------------------------------------------------------
        // Overlays (Starter scene)
        // -----------------------------------------------------------------

        private static void BuildOverlaysInStarter()
        {
            EnsureSceneOpen(StarterScene);

            var uim = Object.FindFirstObjectByType<GameUIManager>(FindObjectsInactive.Include);
            if (!uim)
            {
                Debug.LogError("[EventSystemSetup] GameUIManager not found in Starter; cannot add overlays.");
                return;
            }

            // Parent new overlays alongside the existing ones so GameUIManager.RefreshReferences finds them.
            OverlayBase anyOverlay = Object.FindObjectsByType<OverlayBase>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
            Transform parent = anyOverlay ? anyOverlay.transform.parent : uim.transform;

            BuildEntryOverlay(parent);
            BuildCountdownOverlay(parent);
            BuildHudOverlay(parent);
            BuildResultOverlay(parent);
            BuildFadeOverlay(parent);

            uim.RefreshReferences();
            EditorSceneManager.MarkSceneDirty(uim.gameObject.scene);
            EditorSceneManager.SaveScene(uim.gameObject.scene);
        }

        private static void BuildEntryOverlay(Transform parent)
        {
            if (Exists(parent, "Overlay_EventEntry")) return;
            var (view, content) = MakeOverlay<EventUI.EventEntryOverlay>(parent, "Overlay_EventEntry");

            MakePanel(content, "Panel_Entry", new Vector2(0.5f, 0f), new Vector2(720f, 260f), new Vector2(0f, 220f));
            Transform panel = content.Find("Panel_Entry");

            TextMeshProUGUI title = MakeText(panel, "Text_Title", "EVENT", 46, new Vector2(0f, 70f), new Vector2(680f, 80f));
            var start = MakeButton(panel, "Button_Start", "START", new Vector2(-170f, -60f), new Vector2(300f, 100f));
            var close = MakeButton(panel, "Button_Close", "CLOSE", new Vector2(170f, -60f), new Vector2(300f, 100f));

            Wire(view, "titleText", title);
            Wire(view, "startButton", start.button);
            Wire(view, "closeButton", close.button);
            Wire(view, "startButtonLabel", start.label);
            HideOverlay(view);
        }

        private static void BuildCountdownOverlay(Transform parent)
        {
            if (Exists(parent, "Overlay_EventCountdown")) return;
            var (view, content) = MakeOverlay<EventUI.EventCountdownOverlay>(parent, "Overlay_EventCountdown");

            TextMeshProUGUI count = MakeText(content, "Text_Countdown", "3", 200, Vector2.zero, new Vector2(600f, 400f));
            Wire(view, "countdownText", count);
            HideOverlay(view);
        }

        private static void BuildHudOverlay(Transform parent)
        {
            if (Exists(parent, "Overlay_EventHud")) return;
            var (view, content) = MakeOverlay<EventUI.EventHudOverlay>(parent, "Overlay_EventHud");

            TextMeshProUGUI timer = MakeText(content, "Text_Timer", "0:20", 72, new Vector2(0f, -120f), new Vector2(400f, 120f));
            AnchorTop(timer.rectTransform);
            TextMeshProUGUI prompt = MakeText(content, "Text_Prompt", "REACH THE FINISH!", 44, new Vector2(0f, -120f), new Vector2(900f, 120f));
            AnchorTop(prompt.rectTransform);

            Wire(view, "timerText", timer);
            Wire(view, "promptText", prompt);
            HideOverlay(view);
        }

        private static void BuildResultOverlay(Transform parent)
        {
            if (Exists(parent, "Overlay_LevelResult")) return;
            var (view, content) = MakeOverlay<EventUI.LevelResultOverlay>(parent, "Overlay_LevelResult");

            MakePanel(content, "Panel_Result", new Vector2(0.5f, 0.5f), new Vector2(760f, 460f), Vector2.zero);
            Transform panel = content.Find("Panel_Result");

            TextMeshProUGUI title = MakeText(panel, "Text_Title", "COMPLETE!", 60, new Vector2(0f, 140f), new Vector2(700f, 100f));
            TextMeshProUGUI reward = MakeText(panel, "Text_Reward", "2,000", 80, new Vector2(0f, 10f), new Vector2(700f, 120f));
            var claim = MakeButton(panel, "Button_Claim", "CLAIM", new Vector2(0f, -140f), new Vector2(360f, 110f));

            Wire(view, "titleText", title);
            Wire(view, "rewardText", reward);
            Wire(view, "claimButton", claim.button);
            HideOverlay(view);
        }

        private static void BuildFadeOverlay(Transform parent)
        {
            if (Exists(parent, "Overlay_EventFade")) return;
            var (view, content) = MakeOverlay<EventUI.EventFadeOverlay>(parent, "Overlay_EventFade");

            var img = new GameObject("Image_Black", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            img.transform.SetParent(content, false);
            Stretch(img.rectTransform);
            img.color = Color.black;
            HideOverlay(view);
        }

        // -----------------------------------------------------------------
        // Scene content (Game scene)
        // -----------------------------------------------------------------

        private static void BuildSceneContentInGame(GameObject areaPrefab)
        {
            EnsureSceneOpen(GameScene);

            if (!Object.FindFirstObjectByType<EventManager>(FindObjectsInactive.Include))
            {
                var managerGo = new GameObject("EventManager");
                managerGo.AddComponent<EventManager>();
            }

            var gm = Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
            Vector3 spawn = gm && gm.VehicleSpawnPoint ? gm.VehicleSpawnPoint.position : Vector3.zero;

            SpawnArea(areaPrefab, "EventArea_Jump", spawn + new Vector3(0f, 0f, 25f), EventType.JumpChallenge);
            SpawnArea(areaPrefab, "EventArea_TimeTrial", spawn + new Vector3(40f, 0f, 25f), EventType.TimeTrial);
            SpawnArea(areaPrefab, "EventArea_WatchEarn", spawn + new Vector3(-25f, 0f, 25f), EventType.WatchAndEarn);

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void SpawnArea(GameObject prefab, string name, Vector3 position, EventType type)
        {
            if (GameObject.Find(name))
                return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.position = position;

            var area = instance.GetComponent<EventArea>();
            SetEnum(area, "eventType", (int)type);
        }

        // -----------------------------------------------------------------
        // UI construction helpers
        // -----------------------------------------------------------------

        private static (T view, RectTransform content) MakeOverlay<T>(Transform parent, string name) where T : OverlayBase
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Stretch(go.GetComponent<RectTransform>());

            T view = go.AddComponent<T>();

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(CanvasGroup));
            contentGo.transform.SetParent(go.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            Stretch(contentRt);

            var so = new SerializedObject(view);
            so.FindProperty("content").objectReferenceValue = contentRt;
            so.FindProperty("canvasGroup").objectReferenceValue = contentGo.GetComponent<CanvasGroup>();
            so.ApplyModifiedPropertiesWithoutUndo();

            return (view, contentRt);
        }

        private static void HideOverlay(OverlayBase view)
        {
            // Match the runtime-hidden state so nothing shows/blocks in the editor or before Awake runs.
            if (view.CanvasGroup)
            {
                view.CanvasGroup.alpha = 0f;
                view.CanvasGroup.blocksRaycasts = false;
            }
            if (view.Content)
                view.Content.gameObject.SetActive(false);
        }

        private static void MakePanel(Transform parent, string name, Vector2 anchor, Vector2 size, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        }

        private static TextMeshProUGUI MakeText(Transform parent, string name, string text, float size, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            var rt = tmp.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;
            return tmp;
        }

        private static (EnhancedButton button, TextMeshProUGUI label) MakeButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var img = go.GetComponent<Image>();
            img.color = new Color(0.2f, 0.5f, 0.9f, 1f);

            var button = go.AddComponent<EnhancedButton>();
            button.targetGraphic = img;

            TextMeshProUGUI text = MakeText(go.transform, "Text_Label", label, 40, Vector2.zero, size);
            return (button, text);
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void AnchorTop(RectTransform rt)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        }

        private static void Wire(Object view, string field, Object reference)
        {
            var so = new SerializedObject(view);
            SerializedProperty prop = so.FindProperty(field);
            if (prop != null)
                prop.objectReferenceValue = reference;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool Exists(Transform parent, string childName)
        {
            return parent && parent.Find(childName);
        }

        // -----------------------------------------------------------------
        // Asset / serialization helpers
        // -----------------------------------------------------------------

        private static T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (!asset)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { color = color };
            string path = MatFolder + "/" + name + ".mat";
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void SetObjectList(Object target, string field, List<Object> values)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(field);
            prop.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEnum(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).enumValueIndex = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void EnsureSceneOpen(string scenePath)
        {
            if (EditorSceneManager.GetActiveScene().path != scenePath)
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }
}
