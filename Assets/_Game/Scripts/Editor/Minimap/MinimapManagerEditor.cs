using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Minimap.Editor
{
    /// <summary>
    /// Keeps the rich Odin inspector for <see cref="MinimapManager"/> and adds interactive Scene view handles
    /// so the minimap world area (the square the baked map covers) can be framed directly in the Game scene:
    /// a move handle on the centre and a slider on the +X edge to resize. The yellow disc previews the region
    /// that stays visible around the vehicle.
    /// </summary>
    [CustomEditor(typeof(MinimapManager))]
    public sealed class MinimapManagerEditor : OdinEditor
    {
        private void OnSceneGUI()
        {
            SerializedProperty centerProp = serializedObject.FindProperty("areaCenter");
            SerializedProperty sizeProp = serializedObject.FindProperty("areaSize");
            SerializedProperty viewProp = serializedObject.FindProperty("viewWorldDiameter");
            if (centerProp == null || sizeProp == null) return;

            serializedObject.Update();

            Vector3 center = centerProp.vector3Value;
            float size = sizeProp.floatValue;
            float half = size * 0.5f;

            // Baked-map area square (XZ plane).
            Vector3 a = center + new Vector3(-half, 0f, -half);
            Vector3 b = center + new Vector3(half, 0f, -half);
            Vector3 c = center + new Vector3(half, 0f, half);
            Vector3 d = center + new Vector3(-half, 0f, half);
            Handles.color = new Color(0.3f, 0.7f, 1f, 1f);
            Handles.DrawAAPolyLine(3f, a, b, c, d, a);

            // Visible region preview.
            if (viewProp != null)
            {
                Handles.color = new Color(1f, 0.85f, 0.3f, 1f);
                Handles.DrawWireDisc(center, Vector3.up, viewProp.floatValue * 0.5f);
            }

            Handles.Label(center + new Vector3(0f, 0f, half + 2f), $"Minimap Area  ({size:0} m)");

            EditorGUI.BeginChangeCheck();

            Vector3 newCenter = Handles.PositionHandle(center, Quaternion.identity);

            Vector3 edge = center + new Vector3(half, 0f, 0f);
            float capSize = HandleUtility.GetHandleSize(edge) * 0.12f;
            Handles.color = new Color(0.3f, 0.7f, 1f, 1f);
            Vector3 newEdge = Handles.Slider(edge, Vector3.right, capSize, Handles.CubeHandleCap, 0f);
            float newHalf = Mathf.Max(1f, newEdge.x - newCenter.x);

            if (EditorGUI.EndChangeCheck())
            {
                centerProp.vector3Value = newCenter;
                sizeProp.floatValue = newHalf * 2f;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
