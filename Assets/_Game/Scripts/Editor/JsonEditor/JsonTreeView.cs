using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Game.Scripts.Editor.JsonEditor
{
    /// <summary>
    /// A lightweight view-model node built from a <see cref="JToken"/>. Holds display text, per-node expand
    /// state, and search-match flags. Editor-only; only ever built for human-scale JSON, so no virtualization
    /// is needed.
    /// </summary>
    internal sealed class JsonNodeVM
    {
        public string Key;            // property name, array index, or null for the document root
        public JTokenType Type;
        public JToken Token;          // the backing token (for inline editing + fold-state keying)
        public string ValueText;      // display text for leaf values (with quotes for strings)
        public string ValueClass;     // USS class selecting the value color
        public List<JsonNodeVM> Children;
        public bool Expanded;

        // Search state, recomputed on every query change.
        public bool MatchKey;
        public bool MatchValue;
        public bool MatchSubtree;

        public bool Expandable => Children != null && Children.Count > 0;

        public static JsonNodeVM Build(JToken token) => Build(token, null, 0);

        private static JsonNodeVM Build(JToken token, string key, int depth)
        {
            JsonNodeVM node = new JsonNodeVM { Key = key, Type = token.Type, Token = token };

            switch (token.Type)
            {
                case JTokenType.Object:
                    node.Children = new List<JsonNodeVM>();
                    foreach (JProperty p in ((JObject)token).Properties())
                        node.Children.Add(Build(p.Value, p.Name, depth + 1));
                    break;

                case JTokenType.Array:
                    node.Children = new List<JsonNodeVM>();
                    int i = 0;
                    foreach (JToken child in (JArray)token)
                        node.Children.Add(Build(child, i++.ToString(CultureInfo.InvariantCulture), depth + 1));
                    break;

                default:
                    node.ValueText = PrimitiveText(token);
                    node.ValueClass = PrimitiveClass(token.Type);
                    break;
            }

            // Expand the first couple of levels, plus small containers a bit deeper, so the shape is visible
            // at a glance without a wall of rows.
            node.Expanded = depth < 2 || (node.Expandable && node.Children.Count <= 8 && depth < 4);
            return node;
        }

        private static string PrimitiveText(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    return "\"" + (string)token + "\"";
                case JTokenType.Null:
                    return "null";
                case JTokenType.Undefined:
                    return "undefined";
                case JTokenType.Boolean:
                    return (bool)token ? "true" : "false";
                case JTokenType.Integer:
                case JTokenType.Float:
                    object number = ((JValue)token).Value;
                    return number is System.IFormattable f
                        ? f.ToString(null, CultureInfo.InvariantCulture)
                        : token.ToString();
                default:
                    object raw = ((JValue)token).Value;
                    return raw == null ? "null" : raw.ToString();
            }
        }

        private static string PrimitiveClass(JTokenType type)
        {
            switch (type)
            {
                case JTokenType.String:
                    return "json-string";
                case JTokenType.Integer:
                case JTokenType.Float:
                    return "json-number";
                case JTokenType.Boolean:
                    return "json-bool";
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return "json-null";
                default:
                    return "json-other";
            }
        }
    }

    /// <summary>
    /// Collapsible, searchable tree rendering of a parsed JSON document. Leaf values are inline-editable
    /// (edited in JSON form): a committed edit is spliced into the document and the re-serialized whole is
    /// handed back via <see cref="SetOnEdited"/>, so the window's text buffer remains the single source of
    /// truth. Search keeps a node visible when it (or any descendant) matches, force-expands the path to
    /// matches, and tints the matching key/value. Fold state is preserved across rebuilds. Editor-only.
    /// </summary>
    internal sealed class JsonTreeView : VisualElement
    {
        private readonly ScrollView _scroll;
        private JsonNodeVM _root;
        private string _query = "";

        // Raised after an inline value edit, carrying the re-serialized whole document. The window uses it to
        // update its text buffer + validation without the tree owning the source of truth.
        private Action<string> _onEdited;

        public JsonTreeView()
        {
            AddToClassList("json-tree");
            _scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            _scroll.style.flexGrow = 1;
            Add(_scroll);
        }

        public void SetOnEdited(Action<string> callback) => _onEdited = callback;

        public void SetData(JsonNodeVM root)
        {
            // Carry per-node fold state across rebuilds (value edits, Beautify/Minify) so the view doesn't
            // reset to the default expansion every time the document is re-parsed.
            HashSet<string> previouslyExpanded = CaptureExpanded(_root);
            _root = root;
            if (previouslyExpanded != null)
                RestoreExpanded(_root, previouslyExpanded);
            ComputeMatches();
            Rebuild();
        }

        private static HashSet<string> CaptureExpanded(JsonNodeVM node)
        {
            if (node == null)
                return null;
            HashSet<string> set = new HashSet<string>();
            CollectExpanded(node, set);
            return set;
        }

        private static void CollectExpanded(JsonNodeVM node, HashSet<string> set)
        {
            if (node.Expanded && node.Token != null)
                set.Add(node.Token.Path);
            if (node.Children == null)
                return;
            foreach (JsonNodeVM child in node.Children)
                CollectExpanded(child, set);
        }

        private static void RestoreExpanded(JsonNodeVM node, HashSet<string> set)
        {
            if (node == null)
                return;
            if (node.Token != null && node.Children != null && node.Children.Count > 0)
                node.Expanded = set.Contains(node.Token.Path);
            if (node.Children == null)
                return;
            foreach (JsonNodeVM child in node.Children)
                RestoreExpanded(child, set);
        }

        public void SetSearch(string query)
        {
            _query = query ?? "";
            ComputeMatches();
            Rebuild();
        }

        public void ExpandAll()
        {
            SetExpandedRecursive(_root, true);
            Rebuild();
        }

        public void CollapseAll()
        {
            // Collapse everything, including the root, down to a single line. The root's own row is always
            // rendered (RenderNode adds it before checking expansion), so the view is never blank.
            SetExpandedRecursive(_root, false);
            Rebuild();
        }

        private static void SetExpandedRecursive(JsonNodeVM node, bool expanded)
        {
            if (node == null)
                return;
            node.Expanded = expanded;
            if (node.Children == null)
                return;
            foreach (JsonNodeVM child in node.Children)
                SetExpandedRecursive(child, expanded);
        }

        private void ComputeMatches()
        {
            if (_root == null)
                return;
            string q = string.IsNullOrEmpty(_query) ? null : _query.ToLowerInvariant();
            ComputeMatches(_root, q);
        }

        private static bool ComputeMatches(JsonNodeVM node, string query)
        {
            node.MatchKey = false;
            node.MatchValue = false;

            if (query != null)
            {
                if (!string.IsNullOrEmpty(node.Key) && node.Key.ToLowerInvariant().Contains(query))
                    node.MatchKey = true;
                if (!string.IsNullOrEmpty(node.ValueText) && node.ValueText.ToLowerInvariant().Contains(query))
                    node.MatchValue = true;
            }

            bool subtree = node.MatchKey || node.MatchValue;
            if (node.Children != null)
            {
                foreach (JsonNodeVM child in node.Children)
                    subtree |= ComputeMatches(child, query);
            }

            // With no query, everything is "in scope" so the normal expand state drives visibility.
            node.MatchSubtree = query == null || subtree;
            return subtree;
        }

        private void Rebuild()
        {
            _scroll.Clear();
            if (_root == null)
                return;

            bool searching = !string.IsNullOrEmpty(_query);
            if (searching && !_root.MatchSubtree)
            {
                Label none = new Label("No matches.");
                none.AddToClassList("json-tree-empty");
                _scroll.Add(none);
                return;
            }

            RenderNode(_root, 0, searching);
        }

        private void RenderNode(JsonNodeVM node, int depth, bool searching)
        {
            if (searching && !node.MatchSubtree)
                return;

            _scroll.Add(BuildRow(node, depth, searching));

            if (!IsOpen(node, searching))
                return;

            foreach (JsonNodeVM child in node.Children)
                RenderNode(child, depth + 1, searching);
        }

        // A container is open when manually expanded, or (during a search) additionally when it holds a match
        // to reveal. ORing with node.Expanded keeps manual fold / Expand-All / Collapse-All meaningful during
        // a search, and lets a key-only match be expanded to inspect its children.
        private static bool IsOpen(JsonNodeVM node, bool searching)
        {
            if (node.Children == null || node.Children.Count == 0)
                return false;
            if (!searching)
                return node.Expanded;
            return node.Expanded || HasMatchingChild(node);
        }

        private static bool HasMatchingChild(JsonNodeVM node)
        {
            foreach (JsonNodeVM child in node.Children)
            {
                if (child.MatchSubtree)
                    return true;
            }

            return false;
        }

        private VisualElement BuildRow(JsonNodeVM node, int depth, bool searching)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("json-row");
            row.style.paddingLeft = (float)(6 + depth * 14);

            bool isContainer = node.Type == JTokenType.Object || node.Type == JTokenType.Array;
            bool hasChildren = node.Children != null && node.Children.Count > 0;

            if (hasChildren)
            {
                Label arrow = new Label(IsOpen(node, searching) ? "▾" : "▸"); // ▾ / ▸
                arrow.AddToClassList("json-arrow");
                row.Add(arrow);

                row.RegisterCallback<ClickEvent>(_ =>
                {
                    node.Expanded = !node.Expanded;
                    Rebuild();
                });
            }
            else
            {
                VisualElement spacer = new VisualElement();
                spacer.AddToClassList("json-arrow-spacer");
                row.Add(spacer);
            }

            if (node.Key != null)
            {
                Label keyLabel = new Label(node.Key) { enableRichText = false };
                keyLabel.AddToClassList("json-key");
                if (node.MatchKey)
                    keyLabel.AddToClassList("json-hit");
                row.Add(keyLabel);

                // Colon precedes a scalar value; containers show a "{ N }" / "[ N ]" summary instead.
                Label punct = new Label(isContainer ? "" : ":");
                punct.AddToClassList("json-punct");
                row.Add(punct);
            }

            if (isContainer)
            {
                int count = node.Children != null ? node.Children.Count : 0;
                string summary = node.Type == JTokenType.Object ? "{ " + count + " }" : "[ " + count + " ]";
                Label summaryLabel = new Label(summary) { enableRichText = false };
                summaryLabel.AddToClassList("json-summary");
                row.Add(summaryLabel);
            }
            else
            {
                // Leaf values are inline-editable. isDelayed => the change is reported on Enter / focus-out,
                // not per keystroke. The value is edited in JSON form (strings keep their quotes), so any type
                // is handled uniformly and the user can even change a value's type.
                TextField valueField = new TextField
                {
                    value = node.ValueText ?? string.Empty,
                    isDelayed = true,
                    tooltip = "Edit as JSON (keep quotes for strings). Press Enter to apply.",
                };
                valueField.AddToClassList("json-valuefield");
                if (!string.IsNullOrEmpty(node.ValueClass))
                    valueField.AddToClassList(node.ValueClass);
                if (node.MatchValue)
                    valueField.AddToClassList("json-hit");
                valueField.RegisterValueChangedCallback(evt => CommitEdit(node, evt.newValue, valueField));
                row.Add(valueField);
            }

            return row;
        }

        // Applies an inline value edit: parse the field as a JSON value, splice it into the document, then
        // hand the re-serialized whole document back to the window. Invalid input reverts the field.
        private void CommitEdit(JsonNodeVM node, string newText, TextField field)
        {
            JsonValidationResult result = JsonValidation.Validate(newText);
            if (!result.HasToken)
            {
                field.SetValueWithoutNotify(node.ValueText ?? string.Empty);
                Debug.LogError($"[JsonEditor] '{newText}' is not a valid JSON value: {result.Message}");
                return;
            }

            JToken parsed = result.Token;
            string newJson;
            if (node.Token != null && node.Token.Parent != null)
            {
                node.Token.Replace(parsed);
                newJson = _root != null && _root.Token != null
                    ? _root.Token.ToString(Formatting.Indented)
                    : parsed.ToString(Formatting.Indented);
            }
            else
            {
                // Editing the document root itself (a bare scalar document).
                newJson = parsed.ToString(Formatting.Indented);
            }

            _onEdited?.Invoke(newJson);
        }
    }
}
