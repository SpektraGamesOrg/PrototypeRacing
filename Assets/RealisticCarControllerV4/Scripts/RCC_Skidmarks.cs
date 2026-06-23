//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Generates and manages a procedural mesh for skidmark rendering on a specific ground surface type.
/// Each instance handles one material type; the <see cref="RCC_SkidmarksManager"/> creates one per ground material.
/// </summary>
public class RCC_Skidmarks : RCC_Core {

    /// <summary>
    /// MeshFilter component used to display the skidmark mesh.
    /// </summary>
    private MeshFilter meshFilter;

    /// <summary>
    /// The procedural mesh containing all skidmark geometry.
    /// </summary>
    private Mesh mesh;

    /// <summary>
    /// Maximum number of mark sections this instance can store. Older marks are overwritten in a circular buffer.
    /// </summary>
    [Tooltip("Maximum number of mark sections this instance can store. Older marks are overwritten in a circular buffer.")]
    [Min(1)] public int maxMarks = 1024;

    /// <summary>
    /// Height offset above the ground surface to prevent z-fighting, in meters.
    /// </summary>
    [Tooltip("Height offset above the ground surface to prevent z-fighting, in meters.")]
    [Min(0f)]
    public float groundOffset = 0.02f;

    /// <summary>
    /// Minimum squared distance between consecutive marks to avoid overdraw.
    /// </summary>
    [Tooltip("Minimum squared distance between consecutive marks to avoid overdraw.")]
    [Min(0f)] public float minDistance = 0.1f;

    /// <summary>
    /// Current total number of marks added (may exceed maxMarks due to circular buffer).
    /// </summary>
    private int numMarks = 0;

    /// <summary>
    /// Data for a single skidmark section used to generate the procedural mesh quad strip.
    /// </summary>
    public class MarkSection {

        /// <summary>
        /// World position of this mark (with ground offset applied).
        /// </summary>
        [Tooltip("World position of this mark (with ground offset applied).")]
        public Vector3 pos = Vector3.zero;

        /// <summary>
        /// Surface normal at this mark's position.
        /// </summary>
        [Tooltip("Surface normal at this mark's position.")]
        public Vector3 normal = Vector3.zero;

        /// <summary>
        /// Tangent vector for correct mesh shading.
        /// </summary>
        [Tooltip("Tangent vector for correct mesh shading.")]
        public Vector4 tangent = Vector4.zero;

        /// <summary>
        /// Left edge vertex position of the skidmark quad.
        /// </summary>
        [Tooltip("Left edge vertex position of the skidmark quad.")]
        public Vector3 posl = Vector3.zero;

        /// <summary>
        /// Right edge vertex position of the skidmark quad.
        /// </summary>
        [Tooltip("Right edge vertex position of the skidmark quad.")]
        public Vector3 posr = Vector3.zero;

        /// <summary>
        /// Opacity of this mark section (0-1), stored in vertex color alpha.
        /// </summary>
        [Tooltip("Opacity of this mark section (0-1), stored in vertex color alpha.")]
        public float intensity = 0.0f;

        /// <summary>
        /// Index of the previous mark section in the chain (-1 if this is the first).
        /// </summary>
        [Tooltip("Index of the previous mark section in the chain (-1 if this is the first).")]
        public int lastIndex = 0;

    }

    /// <summary>
    /// Circular buffer of all mark sections.
    /// </summary>
    [Tooltip("Circular buffer of all mark sections.")]
    public MarkSection[] skidmarks;

    /// <summary>
    /// True when the mesh needs to be rebuilt in LateUpdate.
    /// </summary>
    private bool updated = false;

    /// <summary>
    /// Initializes the circular buffer of skidmark sections and caches the mesh reference.
    /// </summary>
    private void Awake() {

        transform.position = Vector3.zero;

        skidmarks = new MarkSection[maxMarks];

        for (int i = 0; i < maxMarks; i++)
            skidmarks[i] = new MarkSection();

        if (!TryGetComponent(out meshFilter)) {

#if UNITY_EDITOR
            Debug.LogError(transform.name + " is missing MeshFilter component. Disabling skidmarks.");
#endif
            enabled = false;
            return;

        }

        mesh = meshFilter.mesh;

    }

    /// <summary>
    /// Adds a new skidmark section at the given position. Called by skidding wheels each frame.
    /// </summary>
    /// <param name="pos">World position of the contact point.</param>
    /// <param name="normal">Surface normal at the contact point.</param>
    /// <param name="intensity">Opacity of the mark (0-1). Values below 0 are rejected.</param>
    /// <param name="width">Width of the skidmark in meters.</param>
    /// <param name="lastIndex">Index of the previous mark in this chain, or -1 for a new chain.</param>
    /// <returns>The index of the newly added mark, or -1 if rejected.</returns>
    public int AddSkidMark(Vector3 pos, Vector3 normal, float intensity, float width, int lastIndex) {

        if (intensity > 1f)
            intensity = 1f;
        if (intensity < 0f)
            return -1;

        if (pos == Vector3.zero)
            return -1;
        if (normal == Vector3.zero)
            return -1;

        if (lastIndex > 0) {

            float sqrDistance = (pos - skidmarks[lastIndex % maxMarks].pos).sqrMagnitude;

            if (sqrDistance < minDistance)
                return lastIndex;

        }

        MarkSection curr = skidmarks[numMarks % maxMarks];
        curr.pos = pos + normal * groundOffset;
        curr.normal = normal;
        curr.intensity = intensity;
        curr.lastIndex = lastIndex;

        if (lastIndex != -1) {

            MarkSection last = skidmarks[lastIndex % maxMarks];
            Vector3 dir = (curr.pos - last.pos);
            Vector3 xDir = Vector3.Cross(dir, normal).normalized;

            curr.posl = curr.pos + xDir * width * 0.5f;
            curr.posr = curr.pos - xDir * width * 0.5f;
            curr.tangent = new Vector4(xDir.x, xDir.y, xDir.z, 1);

            if (last.lastIndex == -1) {

                last.tangent = curr.tangent;
                last.posl = curr.pos + xDir * width * 0.5f;
                last.posr = curr.pos - xDir * width * 0.5f;

            }

        }

        numMarks++;
        updated = true;

        return numMarks - 1;

    }

    // If the mesh needs to be updated, i.e. a new section has been added,
    // the current mesh is removed, and a new mesh for the skidmarks is generated.
    private void LateUpdate() {

        if (!updated)
            return;

        updated = false;

        mesh.Clear();

        int segmentCount = 0;

        for (int j = 0; j < numMarks && j < maxMarks; j++) {

            if (skidmarks[j].lastIndex != -1 && skidmarks[j].lastIndex > numMarks - maxMarks)
                segmentCount++;

        }

        Vector3[] vertices = new Vector3[segmentCount * 4];
        Vector3[] normals = new Vector3[segmentCount * 4];
        Vector4[] tangents = new Vector4[segmentCount * 4];
        Color[] colors = new Color[segmentCount * 4];
        Vector2[] uvs = new Vector2[segmentCount * 4];

        int[] triangles = new int[segmentCount * 6];
        segmentCount = 0;

        for (int i = 0; i < numMarks && i < maxMarks; i++) {

            if (skidmarks[i].lastIndex != -1 && skidmarks[i].lastIndex > numMarks - maxMarks) {

                MarkSection curr = skidmarks[i];
                MarkSection last = skidmarks[curr.lastIndex % maxMarks];

                if (last.pos != Vector3.zero && last.normal != Vector3.zero && Vector3.Distance(curr.pos, last.pos) < 10f) {

                    //  Safety check: ensure segmentCount hasn't exceeded allocated array bounds.
                    if (segmentCount * 4 + 3 >= vertices.Length || segmentCount * 6 + 5 >= triangles.Length)
                        break;

                    vertices[segmentCount * 4 + 0] = last.posl;
                    vertices[segmentCount * 4 + 1] = last.posr;
                    vertices[segmentCount * 4 + 2] = curr.posl;
                    vertices[segmentCount * 4 + 3] = curr.posr;

                    normals[segmentCount * 4 + 0] = last.normal;
                    normals[segmentCount * 4 + 1] = last.normal;
                    normals[segmentCount * 4 + 2] = curr.normal;
                    normals[segmentCount * 4 + 3] = curr.normal;

                    tangents[segmentCount * 4 + 0] = last.tangent;
                    tangents[segmentCount * 4 + 1] = last.tangent;
                    tangents[segmentCount * 4 + 2] = curr.tangent;
                    tangents[segmentCount * 4 + 3] = curr.tangent;

                    colors[segmentCount * 4 + 0] = new Color(0, 0, 0, last.intensity);
                    colors[segmentCount * 4 + 1] = new Color(0, 0, 0, last.intensity);
                    colors[segmentCount * 4 + 2] = new Color(0, 0, 0, curr.intensity);
                    colors[segmentCount * 4 + 3] = new Color(0, 0, 0, curr.intensity);

                    uvs[segmentCount * 4 + 0] = new Vector2(0, 0);
                    uvs[segmentCount * 4 + 1] = new Vector2(1, 0);
                    uvs[segmentCount * 4 + 2] = new Vector2(0, 1);
                    uvs[segmentCount * 4 + 3] = new Vector2(1, 1);

                    triangles[segmentCount * 6 + 0] = segmentCount * 4 + 0;
                    triangles[segmentCount * 6 + 2] = segmentCount * 4 + 1;
                    triangles[segmentCount * 6 + 1] = segmentCount * 4 + 2;

                    triangles[segmentCount * 6 + 3] = segmentCount * 4 + 2;
                    triangles[segmentCount * 6 + 5] = segmentCount * 4 + 1;
                    triangles[segmentCount * 6 + 4] = segmentCount * 4 + 3;

                    segmentCount++;

                }

            }

        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.tangents = tangents;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.uv = uvs;

    }

    /// <summary>
    /// Resets the mark counter and triggers a mesh rebuild, effectively clearing all visible skidmarks.
    /// </summary>
    public void Clean() {

        numMarks = 0;
        updated = true;

    }

}