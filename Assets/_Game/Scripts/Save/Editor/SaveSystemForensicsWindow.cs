using System.IO;
using UnityEditor;
using UnityEngine;

namespace Save.Editor
{
    /// <summary>
    /// Viewer for the silent <see cref="SaveSystemForensics"/> recorder. Open it after reproducing the
    /// save-reset bug to read the full forensic report (live state, best-ever high-watermark, detected
    /// incidents with a per-incident diagnosis, and the recent timeline) and copy it to the clipboard in one
    /// click. Nothing here runs during your tests - it only reads the recorded files on demand.
    /// </summary>
    public class SaveSystemForensicsWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _report = string.Empty;

        [MenuItem("Tools/Save System/Forensics")]
        private static void Open()
        {
            SaveSystemForensicsWindow window = GetWindow<SaveSystemForensicsWindow>("Save Forensics");
            window.minSize = new Vector2(560, 400);
            window.Refresh();
            window.Show();
        }

        private void OnEnable() => Refresh();

        private void Refresh() => _report = SaveSystemForensics.BuildReport();

        private void OnGUI()
        {
            int incidents = SaveSystemForensics.IncidentCount();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                    Refresh();

                if (GUILayout.Button("Capture Now", EditorStyles.toolbarButton, GUILayout.Width(90)))
                {
                    SaveSystemForensics.CaptureNow();
                    Refresh();
                }

                if (GUILayout.Button("Copy Full Report", EditorStyles.toolbarButton, GUILayout.Width(120)))
                {
                    EditorGUIUtility.systemCopyBuffer = _report;
                    ShowNotification(new GUIContent("Report copied to clipboard"));
                }

                if (GUILayout.Button("Reveal Files", EditorStyles.toolbarButton, GUILayout.Width(90)))
                    RevealFolder();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Clear History", EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    if (EditorUtility.DisplayDialog("Clear forensic history",
                            "Delete all recorded snapshots, incidents, and the high-watermark? " +
                            "Do this once when you START a fresh test run.", "Clear", "Cancel"))
                    {
                        SaveSystemForensics.ClearHistory();
                        Refresh();
                    }
                }
            }

            // Prominent banner so a reproduced bug is impossible to miss.
            Color previous = GUI.color;
            GUI.color = incidents > 0 ? new Color(1f, 0.5f, 0.5f) : new Color(0.6f, 1f, 0.6f);
            EditorGUILayout.HelpBox(
                incidents > 0
                    ? $"⚠ {incidents} save-loss incident(s) recorded. Copy the full report below and send it over."
                    : "No save-loss incident recorded yet. Keep testing; if the bug hits, it will show here.",
                incidents > 0 ? MessageType.Error : MessageType.Info);
            GUI.color = previous;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            // Selectable, read-only text area so any part of the report can be hand-copied too.
            EditorGUILayout.SelectableLabel(_report, EditorStyles.textArea,
                GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndScrollView();
        }

        private static void RevealFolder()
        {
            string dir = SaveSystemForensics.ForensicsDirectory;
            if (!Directory.Exists(dir))
            {
                EditorUtility.DisplayDialog("Save Forensics", "No forensic data recorded yet.", "OK");
                return;
            }

            // EditorUtility.RevealInFinder is the portable way to open the folder in the OS file browser.
            EditorUtility.RevealInFinder(dir);
        }
    }
}
