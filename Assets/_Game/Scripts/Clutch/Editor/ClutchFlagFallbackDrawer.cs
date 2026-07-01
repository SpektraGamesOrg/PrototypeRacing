using System.Collections.Generic;
using _Game.Scripts.Editor.JsonEditor;
using Newtonsoft.Json;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace _Game.Scripts.Clutch.Editor
{
    /// <summary>
    /// Odin drawer for each <see cref="global::Clutch.ClutchConfig.ClutchFlagFallback"/> entry. Replaces the
    /// raw, hard-to-read JSON text area with a compact card: the flag key, a live validity badge, a one-line
    /// minified preview, and an "Edit JSON" button that opens the full <see cref="JsonEditorWindow"/> (tree
    /// fold/search, text edit/paste, live validation) bound to this entry's <c>fallbackJson</c>. The raw text
    /// area is still available under a "Raw JSON" foldout for quick inline edits.
    ///
    /// Editor-only: lives in the editor assembly, so the runtime <see cref="global::Clutch.ClutchConfig"/> asset
    /// needs no changes and never references the editor window.
    /// </summary>
    public class ClutchFlagFallbackDrawer : OdinValueDrawer<global::Clutch.ClutchConfig.ClutchFlagFallback>
    {
        // Per-property "Raw JSON" foldout state, keyed by owner instance id + Odin property path so two
        // different assets (or a duplicated ClutchConfig) don't share one open/closed bit.
        private static readonly Dictionary<string, bool> RawExpandedByPath = new Dictionary<string, bool>();

        private static GUIStyle _badgeStyle;
        private static GUIStyle _previewStyle;

        private static readonly GUIContent EditButtonContent =
            new GUIContent("✎ Edit JSON", "Open the JSON editor for this entry");

        // Per-drawer parse cache so the full document is not re-parsed on every IMGUI repaint; recomputed only
        // when the JSON string changes. Routes through JsonValidation so the badge matches the editor window.
        private bool _cacheValid;
        private string _cachedJson;
        private string _badgeText;
        private Color _badgeColor;
        private string _previewText;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            global::Clutch.ClutchConfig.ClutchFlagFallback entry = ValueEntry.SmartValue;
            string json = entry != null ? entry.fallbackJson : null;
            RefreshCache(json);

            SirenixEditorGUI.BeginBox();
            {
                SirenixEditorGUI.BeginBoxHeader();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        // Layout: [badge] fixed on the left, [key field] flexes to fill the middle, [Edit JSON]
                        // fixed on the right. Nothing reserves a large fixed width, so the row never forces the
                        // whole Inspector wider than its panel (which would push the top buttons off-screen).
                        // Writes go back through the key child's ValueEntry so Odin still handles serialize+undo.
                        DrawBadge();
                        GUILayout.Space(6);

                        EditorGUI.BeginChangeCheck();
                        string newKey = EditorGUILayout.TextField(entry != null ? entry.key : string.Empty,
                            GUILayout.ExpandWidth(true), GUILayout.MinWidth(40));
                        if (EditorGUI.EndChangeCheck())
                        {
                            InspectorProperty keyChild = Property.Children.Get("key");
                            if (keyChild?.ValueEntry != null)
                                keyChild.ValueEntry.WeakSmartValue = newKey;
                        }

                        GUILayout.Space(6);
                        if (GUILayout.Button(EditButtonContent, GUILayout.Height(20), GUILayout.Width(96)))
                            OpenEditor(entry);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                SirenixEditorGUI.EndBoxHeader();

                string foldoutKey = FoldoutKey();
                RawExpandedByPath.TryGetValue(foldoutKey, out bool expanded);
                bool nextExpanded = SirenixEditorGUI.Foldout(expanded, "Raw JSON");
                if (nextExpanded != expanded)
                    RawExpandedByPath[foldoutKey] = nextExpanded;

                if (nextExpanded)
                {
                    InspectorProperty jsonChild = Property.Children.Get("fallbackJson");
                    if (jsonChild != null)
                        jsonChild.Draw(GUIContent.none);
                }
                else
                {
                    DrawPreview();
                }
            }
            SirenixEditorGUI.EndBox();
        }

        private void OpenEditor(global::Clutch.ClutchConfig.ClutchFlagFallback entry)
        {
            Object owner = Property.Tree.WeakTargets.Count > 0 ? Property.Tree.WeakTargets[0] as Object : null;
            if (!owner)
            {
                Debug.LogError("[ClutchFlagFallbackDrawer] Could not resolve the owning ClutchConfig asset.");
                return;
            }

            global::Clutch.ClutchConfig config = owner as global::Clutch.ClutchConfig;
            int index = -1;
            if (config != null)
            {
                IReadOnlyList<global::Clutch.ClutchConfig.ClutchFlagFallback> list = config.Fallbacks;
                for (int i = 0; i < list.Count; i++)
                {
                    if (ReferenceEquals(list[i], entry))
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index < 0)
            {
                Debug.LogError("[ClutchFlagFallbackDrawer] Could not locate this entry in ClutchConfig.Fallbacks.");
                return;
            }

            string valuePath = $"fallbacks.Array.data[{index}].fallbackJson";
            string keyPath = $"fallbacks.Array.data[{index}].key";
            string title = entry != null && !string.IsNullOrEmpty(entry.key) ? entry.key : "(unkeyed)";

            // Pass the key + its path as an identity guard so a write can't land on the wrong entry if the
            // list is reordered/edited while the window is open.
            JsonEditorWindow.Open(owner, valuePath, title, keyPath, entry != null ? entry.key : null);
        }

        private string FoldoutKey()
        {
            // GetHashCode() on a UnityEngine.Object returns its instance id (unique per live object) and,
            // unlike GetInstanceID(), is not deprecated in Unity 6. Transient UI state, so per-session identity
            // is all we need.
            int ownerId = Property.Tree.WeakTargets.Count > 0 && Property.Tree.WeakTargets[0] is Object o
                ? o.GetHashCode()
                : 0;
            return ownerId + ":" + Property.Path;
        }

        private void RefreshCache(string json)
        {
            if (_cacheValid && _cachedJson == json)
                return;

            _cacheValid = true;
            _cachedJson = json;

            JsonValidationResult result = JsonValidation.Validate(json);
            switch (result.Status)
            {
                case JsonStatus.Empty:
                    _badgeText = "∅ empty";
                    _badgeColor = new Color(0.60f, 0.61f, 0.64f);
                    _previewText = "(no JSON — click Edit JSON to add)";
                    break;
                case JsonStatus.Invalid:
                    _badgeText = "✕ invalid";
                    _badgeColor = new Color(0.92f, 0.46f, 0.46f);
                    _previewText = OneLine(json);
                    break;
                case JsonStatus.Warning:
                    _badgeText = "⚠ dupes";
                    _badgeColor = new Color(0.91f, 0.75f, 0.38f);
                    _previewText = result.Token != null ? result.Token.ToString(Formatting.None) : OneLine(json);
                    break;
                default:
                    _badgeText = "✓ valid";
                    _badgeColor = new Color(0.46f, 0.80f, 0.52f);
                    _previewText = result.Token != null ? result.Token.ToString(Formatting.None) : OneLine(json);
                    break;
            }

            if (_previewText != null && _previewText.Length > 180)
                _previewText = _previewText.Substring(0, 180) + " …";
        }

        private void DrawBadge()
        {
            EnsureStyles();
            Color prev = _badgeStyle.normal.textColor;
            _badgeStyle.normal.textColor = _badgeColor;
            GUILayout.Label(_badgeText, _badgeStyle, GUILayout.Width(64));
            _badgeStyle.normal.textColor = prev;
        }

        private void DrawPreview()
        {
            EnsureStyles();
            // Reserve an available-width rect (never the text's full preferred width) so a long one-line
            // preview clips instead of forcing a horizontal scrollbar on the whole Inspector.
            Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, _previewStyle);
            GUI.Label(rect, _previewText ?? string.Empty, _previewStyle);
        }

        private static string OneLine(string json)
        {
            return string.IsNullOrEmpty(json) ? "" : json.Replace('\n', ' ').Replace('\r', ' ');
        }

        private static void EnsureStyles()
        {
            if (_badgeStyle == null)
            {
                _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                };
            }

            if (_previewStyle == null)
            {
                _previewStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    wordWrap = false,
                    clipping = TextClipping.Clip,
                    normal = { textColor = new Color(0.60f, 0.62f, 0.66f) },
                };
            }
        }
    }
}
