using System.IO;
using UnityEngine;

namespace Vehicles.CarDeformationSystem
{
    [RequireComponent(typeof(MeshFilter))]
    public class VertexDamagePainter : MonoBehaviour
    {
        [Header("Painting")]
        [SerializeField, Tooltip("Impact radius in local space.")]
        private float radius = 0.4f;

        [SerializeField, Tooltip("On: damage accumulates. Off: only the hardest impact is kept.")]
        private bool accumulate = true;

        [Header("Saving")]
        [SerializeField, Tooltip("UNIQUE save name for this panel (must differ per panel, e.g. car01_body).")]
        private string saveKey = "panel";

        [SerializeField, Tooltip("Mesh filter that owns the paintable mesh. Auto-wired in the editor.")]
        private MeshFilter meshFilter;

        // Runtime state (not serialized).
        private Mesh mesh;        // runtime instance, the shared mesh asset stays untouched
        private Vector3[] verts;  // vertices cached in local space
        private Color[] colors;   // R channel = damage amount (0 = undamaged)
        private float invScale;   // compensates for the transform's uniform scale
        private bool colorsDirty; // colors changed since the last FlushColors

        /// <summary>True while there are painted changes that have not been uploaded/persisted yet.</summary>
        public bool HasPendingChanges => colorsDirty;

        private string SavePath => Path.Combine(Application.persistentDataPath, $"dmg_{saveKey}.bytes");

        private void Awake()
        {
            // Editor wiring is the source of truth; this fallback only runs if the field is empty.
            if (!meshFilter)
                meshFilter = GetComponent<MeshFilter>();

            mesh = meshFilter.mesh;           // runtime copy, leaves the shared asset intact
            verts = mesh.vertices;            // local space
            colors = new Color[verts.Length]; // all black = no damage (R = 0)
            mesh.colors = colors;

            // Compensate for the transform's uniform scale so the radius stays world-consistent.
            invScale = 1f / Mathf.Max(0.0001f, transform.lossyScale.x);

            LoadDamage(); // restore previously saved damage
        }

        /// <summary>
        /// Paints damage into a sphere around a world-space contact point. The mesh upload is
        /// deferred to <see cref="FlushColors"/> so a single collision with many contacts uploads
        /// the vertex colors only once per frame instead of once per contact.
        /// </summary>
        /// <param name="worldPoint">Contact point in world space.</param>
        /// <param name="amount">Damage strength in [0, 1].</param>
        public void ApplyDamage(Vector3 worldPoint, float amount)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            float r = radius * invScale;
            float sqrR = r * r;

            for (int i = 0; i < verts.Length; i++)
            {
                float sqrD = (verts[i] - local).sqrMagnitude; // no square root yet
                if (sqrD > sqrR) continue;

                float d = Mathf.Sqrt(sqrD);        // only for vertices inside the radius
                float add = amount * (1f - d / r); // stronger toward the center

                float cur = colors[i].r;
                float newVal = accumulate
                    ? Mathf.Clamp01(cur + add)
                    : Mathf.Max(cur, add);

                if (newVal != cur)
                {
                    colors[i].r = newVal;
                    colorsDirty = true;
                }
            }
        }

        /// <summary>Uploads pending vertex color changes to the mesh. No-op when nothing changed.</summary>
        public void FlushColors()
        {
            if (!colorsDirty) return;

            mesh.colors = colors;
            colorsDirty = false;
        }

        /// <summary>Writes only the R channel to disk (4 bytes per vertex).</summary>
        public void SaveDamage()
        {
            using (var fs = new FileStream(SavePath, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(colors.Length);
                for (int i = 0; i < colors.Length; i++)
                    bw.Write(colors[i].r);
            }
        }

        public void LoadDamage()
        {
            if (!File.Exists(SavePath)) return;

            using (var fs = new FileStream(SavePath, FileMode.Open))
            using (var br = new BinaryReader(fs))
            {
                int count = br.ReadInt32();
                // A mismatching vertex count means the mesh changed, so the save is no longer valid.
                if (count != colors.Length) return;

                for (int i = 0; i < count; i++)
                    colors[i].r = br.ReadSingle();
            }

            mesh.colors = colors;
            colorsDirty = false;
        }

        /// <summary>Clears all damage and deletes the saved file.</summary>
        public void ResetDamage()
        {
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.black;

            mesh.colors = colors;
            colorsDirty = false;

            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (meshFilter != GetComponent<MeshFilter>())
            {
                meshFilter = GetComponent<MeshFilter>();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
