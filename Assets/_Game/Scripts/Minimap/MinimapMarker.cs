using Sirenix.OdinInspector;
using UnityEngine;

namespace Minimap
{
    /// <summary>
    /// World-space pointer that shows up as an icon on the minimap. Drop this on any GameObject (a pickup, a
    /// destination, a point of interest) and it tracks that object's world position. It self-registers with
    /// <see cref="MinimapManager"/> while enabled, so there is no scene scanning and no per-frame lookup.
    ///
    /// Registration is driven by OnEnable/OnDisable, so adding (Instantiate / AddComponent / SetActive(true))
    /// and removing (Destroy / SetActive(false) / disabling the component) a marker is reflected immediately,
    /// at runtime in play mode. <see cref="ExecuteAlways"/> keeps that working in the editor (edit mode) too.
    /// </summary>
    [ExecuteAlways]
    public sealed class MinimapMarker : MonoBehaviour
    {
        [Title("Icon")]
        [Tooltip("Sprite drawn on the minimap for this marker (your art).")]
        [SerializeField, PreviewField(48)] private Sprite icon;

        [Tooltip("Icon size on the minimap, in canvas pixels. Lets each marker use its own resolution.")]
        [SerializeField] private Vector2 size = new Vector2(28f, 28f);

        [Tooltip("Icon tint.")]
        [SerializeField] private Color color = Color.white;

        [Tooltip("When the marker is outside the visible circle, clamp it to the rim instead of hiding it, " +
                 "so it never leaves the minimap border and still shows direction.")]
        [SerializeField] private bool clampToEdge = false;

        [Tooltip("Keep the icon upright (always looks normal). Turn OFF to make the icon spin together with " +
                 "the rotating map.")]
        [SerializeField] private bool keepUpright = true;

        [Tooltip("Gizmo colour used to preview this marker in the Scene view.")]
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.8f, 1f, 1f);

        public Sprite Icon => icon;
        public Vector2 Size => size;
        public Color IconColor => color;
        public bool ClampToEdge => clampToEdge;
        public bool KeepUpright => keepUpright;

        private void OnEnable() => MinimapManager.RegisterMarker(this);
        private void OnDisable() => MinimapManager.UnregisterMarker(this);

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, 2f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 6f);
        }
#endif
    }
}
