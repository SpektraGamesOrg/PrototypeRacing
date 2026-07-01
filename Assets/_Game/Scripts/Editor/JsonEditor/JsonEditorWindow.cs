using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Game.Scripts.Editor.JsonEditor
{
    /// <summary>
    /// A modern, editor-only JSON editor window (UI Toolkit). Provides a collapsible/searchable tree view and
    /// a raw text view for editing or pasting whole documents, live validation with line/column errors, and
    /// beautify/minify. It edits an arbitrary <see cref="Object"/> string field addressed by a serialized
    /// property path, so writes go through <see cref="SerializedObject"/> for correct Undo + dirtying. Bind it
    /// via <see cref="Open(Object,string,string)"/>. Everything here is under an Editor assembly, so it never
    /// ships in a build and has no runtime cost.
    ///
    /// The single editable source of truth is the text buffer (<c>_text</c>); the tree is a rendered view of
    /// the last successful parse. Binding fields are serialized so the window rebinds correctly across domain
    /// reloads.
    /// </summary>
    public sealed class JsonEditorWindow : EditorWindow
    {
        private const string UssPath = "Assets/_Game/Scripts/Editor/JsonEditor/JsonEditorWindow.uss";

        private enum ViewMode
        {
            Tree,
            Text,
        }

        // --- Serialized binding + buffer (survive domain reloads) -------------------------------------
        [SerializeField] private Object _owner;
        [SerializeField] private string _propertyPath;
        [SerializeField] private string _label = "JSON";
        [SerializeField] private string _text = "";
        [SerializeField] private string _lastApplied = "";
        [SerializeField] private ViewMode _mode = ViewMode.Tree;
        [SerializeField] private bool _scratch;
        [SerializeField] private string _searchQuery = "";

        // Optional identity guard: before writing, the string at _verifyPath must still equal _verifyValue.
        // Protects against the indexed _propertyPath resolving to a DIFFERENT entry after the list is edited.
        [SerializeField] private string _verifyPath;
        [SerializeField] private string _verifyValue;

        // --- UI references (rebuilt in CreateGUI) -----------------------------------------------------
        private Label _titleLabel;
        private Button _treeModeButton;
        private Button _textModeButton;
        private Button _expandButton;
        private Button _collapseButton;
        private Button _beautifyButton;
        private Button _minifyButton;
        private ToolbarSearchField _search;

        private VisualElement _statusBar;
        private Label _statusIcon;
        private Label _statusText;

        private VisualElement _treePane;
        private JsonTreeView _tree;
        private VisualElement _treeInvalidOverlay;
        private Label _treeInvalidLabel;

        private ScrollView _textPane;
        private TextField _textField;

        private Label _footerInfo;
        private Button _revertButton;
        private Button _applyButton;
        private Button _applyCloseButton;

        private bool _suppressTextEvent;

        /// <summary>
        /// Opens (or re-targets) the editor for a string field on <paramref name="owner"/> addressed by the
        /// serialized <paramref name="propertyPath"/> (e.g. <c>fallbacks.Array.data[2].fallbackJson</c>).
        /// Prompts before discarding unsaved changes when re-targeting an already-open window.
        ///
        /// <paramref name="verifyPropertyPath"/> / <paramref name="verifyValue"/> are an optional identity
        /// guard: if set, the string at that path must still equal the expected value at write time, so an
        /// indexed path can't silently write to a different entry after the list is reordered/edited.
        /// </summary>
        public static void Open(Object owner, string propertyPath, string label,
            string verifyPropertyPath = null, string verifyValue = null)
        {
            JsonEditorWindow window = GetWindow<JsonEditorWindow>(false, "JSON Editor", true);

            bool built = window.rootVisualElement != null && window.rootVisualElement.childCount > 0;
            bool retargeting = window._owner != owner || window._propertyPath != propertyPath;
            if (built && retargeting && window.hasUnsavedChanges)
            {
                bool discard = EditorUtility.DisplayDialog(
                    "JSON Editor",
                    $"You have unsaved changes to \"{window._label}\".\nDiscard them and open \"{label}\"?",
                    "Discard & Open", "Cancel");
                if (!discard)
                {
                    window.Focus();
                    return;
                }
            }

            window._owner = owner;
            window._propertyPath = propertyPath;
            window._verifyPath = verifyPropertyPath;
            window._verifyValue = verifyValue;
            window._scratch = false;
            window._label = string.IsNullOrEmpty(label) ? "JSON" : label;
            window._searchQuery = "";
            window._text = ReadFrom(owner, propertyPath) ?? "";
            window._lastApplied = window._text;
            window.minSize = new Vector2(560, 420);

            if (built)
                window.RefreshAll(true);

            window.Show();
            window.Focus();
        }

        [MenuItem("Tools/JSON Editor")]
        public static void OpenScratch()
        {
            JsonEditorWindow window = GetWindow<JsonEditorWindow>(false, "JSON Editor", true);

            bool built = window.rootVisualElement != null && window.rootVisualElement.childCount > 0;
            // Don't silently convert a bound window with pending edits into a scratch buffer (that would drop
            // the asset binding so the edits could never be applied). Mirror Open()'s guard.
            if (built && !window._scratch && window.hasUnsavedChanges)
            {
                bool discard = EditorUtility.DisplayDialog(
                    "JSON Editor",
                    $"You have unsaved changes to \"{window._label}\".\nDiscard them and open a scratch editor?",
                    "Discard & Open", "Cancel");
                if (!discard)
                {
                    window.Focus();
                    return;
                }
            }

            window._owner = null;
            window._propertyPath = null;
            window._verifyPath = null;
            window._verifyValue = null;
            window._scratch = true;
            window._label = "Scratch";
            window._lastApplied = window._text ?? "";
            window._text ??= "";
            window.minSize = new Vector2(560, 420);

            if (built)
                window.RefreshAll(true);

            window.Show();
            window.Focus();
        }

        private void CreateGUI()
        {
            saveChangesMessage = "This JSON has unsaved changes. Apply them to the asset?";

            VisualElement root = rootVisualElement;
            root.Clear();
            root.AddToClassList("json-root");
            root.style.flexGrow = 1;
            root.style.backgroundColor = new Color(0.12f, 0.12f, 0.13f); // fallback if the USS fails to load

            StyleSheet uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (uss)
                root.styleSheets.Add(uss);

            BuildToolbar(root);
            BuildStatusBar(root);
            BuildBody(root);
            BuildFooter(root);

            RefreshAll(true);
        }

        // --- UI construction --------------------------------------------------------------------------

        private void BuildToolbar(VisualElement root)
        {
            VisualElement toolbar = new VisualElement();
            toolbar.AddToClassList("json-toolbar");

            _titleLabel = new Label(_label) { enableRichText = false };
            _titleLabel.AddToClassList("json-title");
            toolbar.Add(_titleLabel);

            // Tree / Text segmented control.
            VisualElement segment = new VisualElement();
            segment.AddToClassList("json-seg");
            _treeModeButton = MakeSegButton("Tree", () => SetMode(ViewMode.Tree));
            _textModeButton = MakeSegButton("Text", () => SetMode(ViewMode.Text));
            segment.Add(_treeModeButton);
            segment.Add(_textModeButton);
            toolbar.Add(segment);

            _beautifyButton = MakeToolButton("Beautify", "Pretty-print (indent) the JSON", Beautify);
            _minifyButton = MakeToolButton("Minify", "Collapse the JSON to a single line", Minify);
            _expandButton = MakeToolButton("Expand All", "Expand every tree node", () => { _tree?.ExpandAll(); });
            _collapseButton = MakeToolButton("Collapse All", "Collapse every tree node", () => { _tree?.CollapseAll(); });
            toolbar.Add(_beautifyButton);
            toolbar.Add(_minifyButton);
            toolbar.Add(_expandButton);
            toolbar.Add(_collapseButton);

            VisualElement spacer = new VisualElement();
            spacer.AddToClassList("json-spacer");
            toolbar.Add(spacer);

            _search = new ToolbarSearchField();
            _search.AddToClassList("json-search");
            _search.RegisterValueChangedCallback(evt =>
            {
                _searchQuery = evt.newValue;
                OnSearchChanged();
            });
            toolbar.Add(_search);

            root.Add(toolbar);
        }

        private Button MakeSegButton(string text, System.Action onClick)
        {
            Button b = new Button(onClick) { text = text };
            b.AddToClassList("json-segbtn");
            return b;
        }

        private Button MakeToolButton(string text, string tooltip, System.Action onClick)
        {
            Button b = new Button(onClick) { text = text, tooltip = tooltip };
            b.AddToClassList("json-tbtn");
            return b;
        }

        private void BuildStatusBar(VisualElement root)
        {
            _statusBar = new VisualElement();
            _statusBar.AddToClassList("json-status");

            _statusIcon = new Label();
            _statusIcon.AddToClassList("json-status__icon");
            _statusText = new Label { enableRichText = false };
            _statusText.AddToClassList("json-status__text");

            _statusBar.Add(_statusIcon);
            _statusBar.Add(_statusText);
            root.Add(_statusBar);
        }

        private void BuildBody(VisualElement root)
        {
            VisualElement body = new VisualElement();
            body.AddToClassList("json-body");
            body.style.flexGrow = 1;

            // Tree pane (tree + an overlay shown when the current text is invalid).
            _treePane = new VisualElement();
            _treePane.AddToClassList("json-treepane");
            _treePane.style.flexGrow = 1;

            _tree = new JsonTreeView();
            _tree.SetOnEdited(OnTreeEdited);
            _treePane.Add(_tree);

            _treeInvalidOverlay = new VisualElement();
            _treeInvalidOverlay.AddToClassList("json-tree-invalid");
            _treeInvalidLabel = new Label("Fix the errors to view the tree.") { enableRichText = false };
            _treeInvalidOverlay.Add(_treeInvalidLabel);
            _treeInvalidOverlay.style.display = DisplayStyle.None;
            _treePane.Add(_treeInvalidOverlay);

            body.Add(_treePane);

            // Text pane.
            _textPane = new ScrollView(ScrollViewMode.Vertical);
            _textPane.AddToClassList("json-textpane");
            _textPane.style.flexGrow = 1;
            _textField = new TextField { multiline = true };
            _textField.AddToClassList("json-textfield");
            _textField.style.flexGrow = 1;
            _textField.RegisterValueChangedCallback(OnTextChanged);
            _textPane.Add(_textField);
            body.Add(_textPane);

            root.Add(body);
        }

        private void BuildFooter(VisualElement root)
        {
            VisualElement footer = new VisualElement();
            footer.AddToClassList("json-footer");

            _footerInfo = new Label { enableRichText = false };
            _footerInfo.AddToClassList("json-footer__info");
            footer.Add(_footerInfo);

            VisualElement spacer = new VisualElement();
            spacer.AddToClassList("json-spacer");
            footer.Add(spacer);

            _revertButton = new Button(Revert) { text = "Revert", tooltip = "Discard edits since the last apply" };
            _revertButton.AddToClassList("json-btn");
            footer.Add(_revertButton);

            _applyButton = new Button(() => Apply(false)) { text = "Apply" };
            _applyButton.AddToClassList("json-apply");
            footer.Add(_applyButton);

            _applyCloseButton = new Button(() => Apply(true)) { text = "Apply & Close" };
            _applyCloseButton.AddToClassList("json-apply");
            footer.Add(_applyCloseButton);

            root.Add(footer);
        }

        // --- State / refresh --------------------------------------------------------------------------

        private void RefreshAll(bool syncTextField)
        {
            if (_textField == null)
                return; // UI not built yet

            if (syncTextField)
            {
                _suppressTextEvent = true;
                _textField.SetValueWithoutNotify(_text ?? "");
                _suppressTextEvent = false;
            }

            _search?.SetValueWithoutNotify(_searchQuery ?? "");

            UpdateModePanes();

            JsonValidationResult result = JsonValidation.Validate(_text);
            UpdateStatus(result);
            if (_mode == ViewMode.Tree)
                SyncTree(result);
            UpdateFooter(result);
            UpdateButtons(result);

            hasUnsavedChanges = _scratch ? false : (_text ?? "") != (_lastApplied ?? "");
            UpdateTitle();
        }

        private void UpdateModePanes()
        {
            bool tree = _mode == ViewMode.Tree;
            _treePane.style.display = tree ? DisplayStyle.Flex : DisplayStyle.None;
            _textPane.style.display = tree ? DisplayStyle.None : DisplayStyle.Flex;

            _treeModeButton.EnableInClassList("json-segbtn--active", tree);
            _textModeButton.EnableInClassList("json-segbtn--active", !tree);

            _expandButton.SetEnabled(tree);
            _collapseButton.SetEnabled(tree);

            // Search runs on the tree; disable (rather than silently no-op) the field in Text mode.
            _search.SetEnabled(tree);
            _search.tooltip = tree ? "Search keys and values" : "Switch to Tree view to search";
        }

        private void SyncTree(JsonValidationResult result)
        {
            if (result.HasToken)
            {
                _tree.style.display = DisplayStyle.Flex;
                _treeInvalidOverlay.style.display = DisplayStyle.None;
                _tree.SetData(JsonNodeVM.Build(result.Token));
                _tree.SetSearch(_searchQuery);
            }
            else
            {
                _tree.style.display = DisplayStyle.None;
                _treeInvalidOverlay.style.display = DisplayStyle.Flex;
                _treeInvalidLabel.text = result.Status == JsonStatus.Empty
                    ? "Empty - switch to Text to add JSON."
                    : "Fix the errors to view the tree.";
            }
        }

        private void UpdateStatus(JsonValidationResult result)
        {
            _statusBar.RemoveFromClassList("json-status--ok");
            _statusBar.RemoveFromClassList("json-status--warn");
            _statusBar.RemoveFromClassList("json-status--error");
            _statusBar.RemoveFromClassList("json-status--empty");

            switch (result.Status)
            {
                case JsonStatus.Valid:
                    _statusBar.AddToClassList("json-status--ok");
                    _statusIcon.text = "✓";
                    _statusText.text = result.Message;
                    break;
                case JsonStatus.Warning:
                    _statusBar.AddToClassList("json-status--warn");
                    _statusIcon.text = "⚠";
                    _statusText.text = result.Message;
                    break;
                case JsonStatus.Invalid:
                    _statusBar.AddToClassList("json-status--error");
                    _statusIcon.text = "✕";
                    _statusText.text = result.Line > 0
                        ? $"Line {result.Line}, Col {result.Column}:  {result.Message}"
                        : result.Message;
                    break;
                default:
                    _statusBar.AddToClassList("json-status--empty");
                    _statusIcon.text = "∅";
                    _statusText.text = result.Message;
                    break;
            }
        }

        private void UpdateFooter(JsonValidationResult result)
        {
            string text = _text ?? "";
            int chars = text.Length;
            int lines = chars == 0 ? 0 : CountLines(text);
            string nodeInfo = result.HasToken ? $"  •  {JsonValidation.CountNodes(result.Token)} nodes" : "";
            _footerInfo.text = $"{chars} chars  •  {lines} lines{nodeInfo}";
        }

        private void UpdateButtons(JsonValidationResult result)
        {
            _beautifyButton.SetEnabled(result.HasToken);
            _minifyButton.SetEnabled(result.HasToken);

            bool dirty = (_text ?? "") != (_lastApplied ?? "");
            _revertButton.SetEnabled(dirty);

            bool canApply = result.CanApply && (_scratch || _owner);
            _applyButton.SetEnabled(canApply);
            _applyCloseButton.SetEnabled(canApply);
            _applyButton.text = _scratch ? "Copy JSON" : "Apply";
            _applyCloseButton.text = _scratch ? "Copy & Close" : "Apply & Close";
        }

        private void UpdateTitle()
        {
            bool dirty = !_scratch && (_text ?? "") != (_lastApplied ?? "");
            titleContent = new GUIContent((dirty ? "● " : "") + "JSON Editor");
            if (_titleLabel != null)
                _titleLabel.text = _scratch ? "Scratch" : _label;
        }

        // --- Interaction handlers ---------------------------------------------------------------------

        private void SetMode(ViewMode mode)
        {
            _mode = mode;
            RefreshAll(false);
        }

        private void OnTextChanged(ChangeEvent<string> evt)
        {
            if (_suppressTextEvent)
                return;

            _text = evt.newValue;

            JsonValidationResult result = JsonValidation.Validate(_text);
            UpdateStatus(result);
            UpdateFooter(result);
            UpdateButtons(result);
            hasUnsavedChanges = !_scratch && (_text ?? "") != (_lastApplied ?? "");
            UpdateTitle();
        }

        // Invoked by the tree after an inline value edit, with the re-serialized document. Defer the rebuild
        // so we don't tear down the editing field inside its own change callback.
        private void OnTreeEdited(string newJson)
        {
            _text = newJson;
            rootVisualElement.schedule.Execute(() => RefreshAll(true));
        }

        private void OnSearchChanged()
        {
            // The search field is only interactive in Tree mode (see UpdateModePanes), so applying to the tree
            // is always the right target here.
            if (_mode == ViewMode.Tree)
                _tree.SetSearch(_searchQuery);
        }

        private void Beautify()
        {
            string formatted = JsonValidation.Beautify(_text, out string error);
            if (formatted == null)
            {
                ShowTransientError(error);
                return;
            }

            _text = formatted;
            RefreshAll(true);
        }

        private void Minify()
        {
            string minified = JsonValidation.Minify(_text, out string error);
            if (minified == null)
            {
                ShowTransientError(error);
                return;
            }

            _text = minified;
            RefreshAll(true);
        }

        private void Revert()
        {
            _text = _lastApplied ?? "";
            _searchQuery = "";
            RefreshAll(true);
        }

        // Returns true only when the JSON was actually persisted (or copied, in scratch mode).
        private bool Apply(bool close)
        {
            JsonValidationResult result = JsonValidation.Validate(_text);
            if (!result.CanApply)
            {
                ShowTransientError("Cannot apply invalid JSON - fix the errors first.");
                return false;
            }

            if (_scratch)
            {
                EditorGUIUtility.systemCopyBuffer = _text ?? "";
                ShowInfo("Copied JSON to the clipboard.");
                if (close)
                    Close();
                return true;
            }

            if (!WriteTo(_text, out string error))
            {
                ShowTransientError(error);
                return false;
            }

            _lastApplied = _text;
            hasUnsavedChanges = false;
            RefreshAll(false);

            if (close)
                Close();
            return true;
        }

        // Invoked when the user picks "Save" on the close-time unsaved-changes prompt. Only clear the dirty
        // flag (base.SaveChanges) when the write actually succeeded; otherwise never silently drop the edits -
        // stash them on the clipboard and tell the user, so nothing is lost.
        public override void SaveChanges()
        {
            if (Apply(false))
            {
                base.SaveChanges();
                return;
            }

            EditorGUIUtility.systemCopyBuffer = _text ?? "";
            EditorUtility.DisplayDialog(
                "JSON Editor",
                "This JSON could not be saved (it is invalid, or the target entry changed).\n\n" +
                "Your text has been copied to the clipboard so you can paste it back after fixing it.",
                "OK");
        }

        // --- Read / write through SerializedObject (Undo + dirty) -------------------------------------

        private static string ReadFrom(Object owner, string path)
        {
            if (!owner || string.IsNullOrEmpty(path))
                return null;

            SerializedObject so = new SerializedObject(owner);
            SerializedProperty prop = so.FindProperty(path);
            return prop != null && prop.propertyType == SerializedPropertyType.String ? prop.stringValue : null;
        }

        private bool WriteTo(string json, out string error)
        {
            error = null;
            if (!_owner || string.IsNullOrEmpty(_propertyPath))
            {
                error = "No target bound.";
                return false;
            }

            SerializedObject so = new SerializedObject(_owner);
            so.Update();
            SerializedProperty prop = so.FindProperty(_propertyPath);
            if (prop == null || prop.propertyType != SerializedPropertyType.String)
            {
                error = "Target property no longer exists (the list may have changed). Copy your JSON and reopen the editor.";
                return false;
            }

            // Identity guard: the indexed path may now resolve to a DIFFERENT entry if the list was reordered
            // or an earlier item removed. Refuse to write when the sibling identity no longer matches.
            if (!string.IsNullOrEmpty(_verifyPath))
            {
                SerializedProperty idProp = so.FindProperty(_verifyPath);
                string actual = idProp != null && idProp.propertyType == SerializedPropertyType.String
                    ? idProp.stringValue
                    : null;
                if (actual != _verifyValue)
                {
                    error = $"Target entry changed (expected \"{_verifyValue}\", found \"{actual}\"). " +
                            "The list was edited - copy your JSON and reopen the editor.";
                    return false;
                }
            }

            prop.stringValue = json;
            so.ApplyModifiedProperties(); // records Undo and marks the object dirty
            AssetDatabase.SaveAssets();
            return true;
        }

        // --- Small UI helpers -------------------------------------------------------------------------

        private void ShowTransientError(string message)
        {
            _statusBar.RemoveFromClassList("json-status--ok");
            _statusBar.RemoveFromClassList("json-status--warn");
            _statusBar.RemoveFromClassList("json-status--empty");
            _statusBar.AddToClassList("json-status--error");
            _statusIcon.text = "✕";
            _statusText.text = string.IsNullOrEmpty(message) ? "Something went wrong." : message;
        }

        private void ShowInfo(string message)
        {
            _statusBar.RemoveFromClassList("json-status--warn");
            _statusBar.RemoveFromClassList("json-status--error");
            _statusBar.RemoveFromClassList("json-status--empty");
            _statusBar.AddToClassList("json-status--ok");
            _statusIcon.text = "✓";
            _statusText.text = message;
        }

        private static int CountLines(string text)
        {
            int lines = 1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                    lines++;
            }

            return lines;
        }
    }
}
