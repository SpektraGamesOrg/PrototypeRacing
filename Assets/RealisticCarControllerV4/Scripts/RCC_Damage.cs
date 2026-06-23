//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable damage system for RCC vehicles. Handles mesh deformation, wheel damage and detachment,
/// light breakage, and detachable part separation on collision. Embedded as a field on RCC_CarControllerV4.
/// </summary>
[System.Serializable]
public class RCC_Damage {

    /// <summary>
    /// Reference to the parent vehicle controller that owns this damage instance.
    /// </summary>
    [HideInInspector] public RCC_CarControllerV4 carController;

    /// <summary>
    /// If enabled, all meshes, lights, wheels, and detachable parts are automatically collected on initialization.
    /// If disabled, each part must be assigned manually.
    /// </summary>
    [Tooltip("If enabled, all meshes, lights, wheels, and detachable parts are automatically collected on initialization. If disabled, each part must be assigned manually.")]
    public bool automaticInstallation = true;

    /// <summary>
    /// Whether the damage system has been initialized with mesh and component references.
    /// </summary>
    private bool initialized = false;

    // Mesh deformation
    [Space()]
    [Header("Mesh Deformation")]

    /// <summary>
    /// Enables or disables mesh vertex deformation on collision.
    /// </summary>
    [Tooltip("Enables or disables mesh vertex deformation on collision.")]
    public bool meshDeformation = true;

    /// <summary>
    /// Controls whether deformation is applied gradually over time (Accurate) or instantly (Fast).
    /// </summary>
    [Tooltip("Controls whether deformation is applied gradually over time (Accurate) or instantly (Fast).")]
    public DeformationMode deformationMode = DeformationMode.Fast;

    /// <summary>
    /// Deformation quality mode. Accurate lerps vertices over time; Fast snaps them immediately.
    /// </summary>
    public enum DeformationMode { Accurate, Fast }

    /// <summary>
    /// Resolution of the deformation processing. Higher values process more vertices but cost more performance.
    /// </summary>
    [Tooltip("Resolution of the deformation processing. Higher values process more vertices but cost more performance.")]
    [Range(1, 100)] public int damageResolution = 100;

    /// <summary>
    /// LayerMask filter determining which colliding objects can inflict damage on this vehicle.
    /// </summary>
    [Tooltip("LayerMask filter determining which colliding objects can inflict damage on this vehicle.")]
    public LayerMask damageFilter = -1;

    /// <summary>
    /// Radius (in meters) around the contact point within which vertices are affected by deformation.
    /// </summary>
    [Tooltip("Radius (in meters) around the contact point within which vertices are affected by deformation.")]
    [Min(0f)] public float damageRadius = .75f;

    /// <summary>
    /// Multiplier applied to all mesh deformation. Higher values cause more dramatic deformation.
    /// </summary>
    [Tooltip("Multiplier applied to all mesh deformation. Higher values cause more dramatic deformation.")]
    [Min(0f)] public float damageMultiplier = 1f;

    /// <summary>
    /// Maximum vertex displacement distance from the original position. A value of 0 disables the limit.
    /// </summary>
    [Tooltip("Maximum vertex displacement distance from the original position. A value of 0 disables the limit.")]
    [Min(0f)]
    public float maximumDamage = .5f;

    /// <summary>
    /// Minimum collision impulse magnitude required to trigger any damage. Collisions below this threshold are ignored.
    /// </summary>
    private readonly float minimumCollisionImpulse = .5f;

    /// <summary>
    /// Minimum squared distance between original and current vertex positions to consider the mesh still damaged during repair.
    /// </summary>
    private readonly float minimumVertDistanceForDamagedMesh = .002f;

    /// <summary>
    /// Stores the original (undamaged) vertex positions for a single mesh filter.
    /// </summary>
    public struct OriginalMeshVerts { public Vector3[] meshVerts; }

    /// <summary>
    /// Stores the original (undamaged) local position and rotation for a single wheel.
    /// </summary>
    public struct OriginalWheelPos { public Vector3 wheelPosition; public Quaternion wheelRotation; }

    /// <summary>
    /// Stores a collider reference and whether it was dynamically created for damage detection.
    /// </summary>
    public struct MeshCol { public Collider col; public bool created; }

    /// <summary>
    /// Array of original (undamaged) vertex data for each mesh filter, used as the repair target.
    /// </summary>
    [Tooltip("Array of original (undamaged) vertex data for each mesh filter, used as the repair target.")]
    public OriginalMeshVerts[] originalMeshData;

    /// <summary>
    /// Array of currently damaged vertex data for each mesh filter, updated on each collision.
    /// </summary>
    [Tooltip("Array of currently damaged vertex data for each mesh filter, updated on each collision.")]
    public OriginalMeshVerts[] damagedMeshData;

    /// <summary>
    /// Array of original (undamaged) wheel positions and rotations, used as the repair target.
    /// </summary>
    [Tooltip("Array of original (undamaged) wheel positions and rotations, used as the repair target.")]
    public OriginalWheelPos[] originalWheelData;

    /// <summary>
    /// Array of currently damaged wheel positions and rotations, updated on each collision.
    /// </summary>
    [Tooltip("Array of currently damaged wheel positions and rotations, updated on each collision.")]
    public OriginalWheelPos[] damagedWheelData;

    [Space()]

    /// <summary>
    /// When set to true, the vehicle begins restoring all deformed meshes and wheels to their original state.
    /// </summary>
    [Tooltip("When set to true, the vehicle begins restoring all deformed meshes and wheels to their original state.")]
    public bool repairNow = false;

    /// <summary>
    /// Returns true when the vehicle has been fully restored to its undamaged state.
    /// </summary>
    [Tooltip("Returns true when the vehicle has been fully restored to its undamaged state.")]
    public bool repaired = true;

    /// <summary>
    /// Whether the damage system is currently applying deformation to meshes over time.
    /// </summary>
    private bool deformingNow = false;

    /// <summary>
    /// Returns true when all mesh deformations have been fully applied after a collision.
    /// </summary>
    private bool deformed = true;

    /// <summary>
    /// Timer tracking how long the deformation process has been running (used in Accurate mode).
    /// </summary>
    private float deformationTime = 0f;

    [Space()]

    /// <summary>
    /// If true, normals are recalculated after each deformation or repair step for correct lighting.
    /// </summary>
    [Tooltip("If true, normals are recalculated after each deformation or repair step for correct lighting.")]
    public bool recalculateNormals = true;

    /// <summary>
    /// If true, mesh bounds are recalculated after each deformation or repair step for correct culling.
    /// </summary>
    [Tooltip("If true, mesh bounds are recalculated after each deformation or repair step for correct culling.")]
    public bool recalculateBounds = true;

    // Wheel deformation
    [Space()]
    [Header("Wheel Deformation")]

    /// <summary>
    /// Enables collision-based displacement of wheel collider positions.
    /// </summary>
    [Tooltip("Enables collision-based displacement of wheel collider positions.")]
    public bool wheelDamage = true;

    /// <summary>
    /// Radius (in meters) around the contact point within which wheels are affected by damage.
    /// </summary>
    [Tooltip("Radius (in meters) around the contact point within which wheels are affected by damage.")]
    [Min(0f)] public float wheelDamageRadius = 1f;

    /// <summary>
    /// Multiplier applied to wheel position displacement on collision.
    /// </summary>
    [Tooltip("Multiplier applied to wheel position displacement on collision.")]
    [Min(0f)] public float wheelDamageMultiplier = 1f;

    /// <summary>
    /// If true, wheels can be fully detached from the vehicle when damage exceeds maximumDamage.
    /// </summary>
    [Tooltip("If true, wheels can be fully detached from the vehicle when damage exceeds maximumDamage.")]
    public bool wheelDetachment = true;

    // Light deformation
    [Space()]
    [Header("Light Deformation")]

    /// <summary>
    /// Enables collision-based breakage of vehicle lights.
    /// </summary>
    [Tooltip("Enables collision-based breakage of vehicle lights.")]
    public bool lightDamage = true;

    /// <summary>
    /// Radius (in meters) around the contact point within which lights are affected by damage.
    /// </summary>
    [Tooltip("Radius (in meters) around the contact point within which lights are affected by damage.")]
    [Min(0f)] public float lightDamageRadius = .5f;

    /// <summary>
    /// Multiplier applied to light damage intensity on collision.
    /// </summary>
    [Tooltip("Multiplier applied to light damage intensity on collision.")]
    [Min(0f)] public float lightDamageMultiplier = 1f;

    // Part deformation
    [Space()]
    [Header("Part Deformation")]

    /// <summary>
    /// Enables collision-based damage and detachment of body parts (hood, bumpers, doors, etc.).
    /// </summary>
    [Tooltip("Enables collision-based damage and detachment of body parts (hood, bumpers, doors, etc.).")]
    public bool partDamage = true;

    /// <summary>
    /// Radius (in meters) around the contact point within which detachable parts are affected by damage.
    /// </summary>
    [Tooltip("Radius (in meters) around the contact point within which detachable parts are affected by damage.")]
    [Min(0f)] public float partDamageRadius = 1f;

    /// <summary>
    /// Multiplier applied to detachable part damage intensity on collision.
    /// </summary>
    [Tooltip("Multiplier applied to detachable part damage intensity on collision.")]
    [Min(0f)] public float partDamageMultiplier = 1f;

    [Space()]

    /// <summary>
    /// All mesh filters on the vehicle that participate in deformation (excludes wheel meshes and non-readable meshes).
    /// </summary>
    [Tooltip("All mesh filters on the vehicle that participate in deformation (excludes wheel meshes and non-readable meshes).")]
    public MeshFilter[] meshFilters;

    /// <summary>
    /// All detachable parts on the vehicle (hood, doors, bumpers, etc.).
    /// </summary>
    [Tooltip("All detachable parts on the vehicle (hood, doors, bumpers, etc.).")]
    public RCC_DetachablePart[] detachableParts;

    /// <summary>
    /// All RCC_Light components on the vehicle that can be damaged.
    /// </summary>
    [Tooltip("All RCC_Light components on the vehicle that can be damaged.")]
    public RCC_Light[] lights;

    /// <summary>
    /// All RCC_WheelCollider components on the vehicle that can be damaged or detached.
    /// </summary>
    [Tooltip("All RCC_WheelCollider components on the vehicle that can be damaged or detached.")]
    public RCC_WheelCollider[] wheels;

    /// <summary>
    /// The averaged contact point from the most recent collision, used as the center of damage application.
    /// </summary>
    private Vector3 contactPoint = Vector3.zero;

    /// <summary>
    /// Array of individual contact points from the most recent collision.
    /// </summary>
    private Vector3[] contactPoints;

    /// <summary>
    /// Octree spatial data structures for each mesh filter, used for fast nearest-vertex lookups during deformation.
    /// </summary>
    [Tooltip("Octree spatial data structures for each mesh filter, used for fast nearest-vertex lookups during deformation.")]
    public RCC_Octree[] octrees;

    /// <summary>
    /// Initializes the damage system by collecting all deformable meshes, lights, wheels, and detachable parts from the vehicle.
    /// Must be called before any damage processing can occur.
    /// </summary>
    /// <param name="_carController">The parent vehicle controller that owns this damage instance.</param>
    public void Initialize(RCC_CarControllerV4 _carController) {

        // Getting the main car controller
        carController = _carController;

        if (automaticInstallation) {

            if (meshDeformation)
                CollectProperMeshFilters();

            if (lightDamage)
                GetLights(carController.GetComponentsInChildren<RCC_Light>());

            if (partDamage)
                GetParts(carController.GetComponentsInChildren<RCC_DetachablePart>());

            if (wheelDamage)
                GetWheels(carController.GetComponentsInChildren<RCC_WheelCollider>());

        }

        initialized = true;

    }

    /// <summary>
    /// Collects all proper mesh filters excluding non-readable meshes and wheel meshes.
    /// </summary>
    private void CollectProperMeshFilters() {

        MeshFilter[] allMeshFilters = carController.gameObject.GetComponentsInChildren<MeshFilter>(true);
        List<MeshFilter> properMeshFilters = new List<MeshFilter>();

        // Create a HashSet for faster exclusion of wheel transforms
        HashSet<Transform> wheelTransforms = new HashSet<Transform> {

        carController.FrontLeftWheelTransform,
        carController.FrontRightWheelTransform,
        carController.RearLeftWheelTransform,
        carController.RearRightWheelTransform

    };

        // Track non-readable meshes for a summarized error message
        List<string> nonReadableMeshes = new List<string>();

        foreach (MeshFilter mf in allMeshFilters) {

            if (mf.mesh != null) {

                // If the mesh is not readable, store the error message and skip
                if (!mf.mesh.isReadable) {

                    nonReadableMeshes.Add(mf.transform.name);
                    continue;

                }

                // Exclude any meshes that belong to the wheel transforms
                if (!wheelTransforms.Contains(mf.transform) && !IsDescendantOfAny(mf.transform, wheelTransforms))
                    properMeshFilters.Add(mf);

            }

        }

        // Log a summarized error message for non-readable meshes, if any
#if UNITY_EDITOR
        if (nonReadableMeshes.Count > 0)
            Debug.LogError("The following meshes are not deformable due to Read/Write being disabled: " + string.Join(", ", nonReadableMeshes));
#endif

        GetMeshes(properMeshFilters.ToArray());

    }

    /// <summary>
    /// Checks if the transform is a descendant of any transform in the given set.
    /// </summary>
    private bool IsDescendantOfAny(Transform transform, HashSet<Transform> potentialParents) {

        foreach (var parent in potentialParents) {

            if (transform.IsChildOf(parent))
                return true;

        }

        return false;

    }

    /// <summary>
    /// Assigns the mesh filters that will participate in collision deformation.
    /// </summary>
    /// <param name="allMeshFilters">Array of mesh filters to use for deformation.</param>
    public void GetMeshes(MeshFilter[] allMeshFilters) {

        meshFilters = allMeshFilters;

    }

    /// <summary>
    /// Assigns the vehicle lights that will participate in collision damage.
    /// </summary>
    /// <param name="allLights">Array of RCC_Light components to track for damage.</param>
    public void GetLights(RCC_Light[] allLights) {

        lights = allLights;

    }

    /// <summary>
    /// Assigns the detachable parts that will participate in collision damage and detachment.
    /// </summary>
    /// <param name="allParts">Array of RCC_DetachablePart components to track for damage.</param>
    public void GetParts(RCC_DetachablePart[] allParts) {

        detachableParts = allParts;

    }

    /// <summary>
    /// Assigns the wheel colliders that will participate in collision damage and detachment.
    /// </summary>
    /// <param name="allWheels">Array of RCC_WheelCollider components to track for damage.</param>
    public void GetWheels(RCC_WheelCollider[] allWheels) {

        wheels = allWheels;

    }

    /// <summary>
    /// Initializes the original and damaged mesh vertex data arrays by copying current vertex positions
    /// from all tracked mesh filters. Must be called before deformation or repair can be processed.
    /// </summary>
    private void CheckMeshData() {

        originalMeshData = new OriginalMeshVerts[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++) {

            if (meshFilters[i] != null && meshFilters[i].mesh != null) {

                Vector3[] sourceVerts = meshFilters[i].mesh.vertices;
                originalMeshData[i].meshVerts = new Vector3[sourceVerts.Length];
                System.Array.Copy(sourceVerts, originalMeshData[i].meshVerts, sourceVerts.Length);

            }

        }

        damagedMeshData = new OriginalMeshVerts[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++) {

            if (meshFilters[i] != null && meshFilters[i].mesh != null) {

                Vector3[] sourceVerts = meshFilters[i].mesh.vertices;
                damagedMeshData[i].meshVerts = new Vector3[sourceVerts.Length];
                System.Array.Copy(sourceVerts, damagedMeshData[i].meshVerts, sourceVerts.Length);

            }

        }

    }

    /// <summary>
    /// Initializes the original and damaged wheel position/rotation data arrays by capturing current
    /// local transforms from all tracked wheels. Must be called before wheel damage can be processed.
    /// </summary>
    private void CheckWheelData() {

        originalWheelData = new OriginalWheelPos[wheels.Length];

        for (int i = 0; i < wheels.Length; i++) {

            if (wheels[i] == null)
                continue;

            originalWheelData[i].wheelPosition = wheels[i].transform.localPosition;
            originalWheelData[i].wheelRotation = wheels[i].transform.localRotation;

        }

        damagedWheelData = new OriginalWheelPos[wheels.Length];

        for (int i = 0; i < wheels.Length; i++) {

            if (wheels[i] == null)
                continue;

            damagedWheelData[i].wheelPosition = wheels[i].transform.localPosition;
            damagedWheelData[i].wheelRotation = wheels[i].transform.localRotation;

        }

    }

    /// <summary>
    /// Restores deformed mesh vertices and displaced wheels toward their original positions.
    /// Called each frame from RCC_CarControllerV4.Update() when damage repair is active.
    /// In Accurate mode, vertices lerp smoothly; in Fast mode, they snap instantly.
    /// </summary>
    public void UpdateRepair() {

        if (!carController)
            return;

        if (!initialized)
            return;

        //  If vehicle is not repaired completely, and repairNow is enabled, restore all deformed meshes to their original structions.
        if (!repaired && repairNow) {

            if (originalMeshData == null || originalMeshData.Length < 1)
                CheckMeshData();

            int k;
            repaired = true;

            //  If deformable mesh is still exists, get all verticies of the mesh first. And then move all single verticies to the original positions. If verticies are close enough to the original
            //  position, repaired = true;
            for (k = 0; k < meshFilters.Length; k++) {

                MeshFilter currentMeshFilter = meshFilters[k];

                if (currentMeshFilter != null && currentMeshFilter.mesh != null) {

                    if (originalMeshData[k].meshVerts == null || damagedMeshData[k].meshVerts == null)
                        continue;

                    //  Get all verticies of the mesh first.
                    Vector3[] vertices = currentMeshFilter.mesh.vertices;

                    for (int i = 0; i < vertices.Length; i++) {

                        Vector3 originalMeshVertex = originalMeshData[k].meshVerts[i];

                        //  And then move all single verticies to the original positions
                        if (deformationMode == DeformationMode.Accurate)
                            vertices[i] += (originalMeshVertex - vertices[i]) * (Time.deltaTime * 5f);
                        else
                            vertices[i] = (originalMeshVertex);

                        //  If verticies are close enough to their original positions, repaired = true;
                        if ((originalMeshVertex - vertices[i]).sqrMagnitude >= (minimumVertDistanceForDamagedMesh * minimumVertDistanceForDamagedMesh))
                            repaired = false;

                    }

                    //  We were using the variable named "vertices" above, therefore we need to set the new verticies to the damaged mesh data.
                    //  Damaged mesh data also restored while repairing with this proccess.
                    System.Array.Copy(vertices, damagedMeshData[k].meshVerts, vertices.Length);

                    //  Setting new verticies to the all meshes. Recalculating normals and bounds, and then optimizing. This proccess can be heavy for high poly meshes.
                    //  You may want to disable last three lines.
                    currentMeshFilter.mesh.SetVertices(vertices);

                    if (recalculateNormals)
                        currentMeshFilter.mesh.RecalculateNormals();

                    if (recalculateBounds)
                        currentMeshFilter.mesh.RecalculateBounds();

                }

            }

            for (k = 0; k < wheels.Length; k++) {

                if (wheels[k] != null) {

                    //  Get all verticies of the mesh first.
                    Vector3 wheelPos = wheels[k].transform.localPosition;

                    //  And then move all single verticies to the original positions
                    if (deformationMode == DeformationMode.Accurate)
                        wheelPos += (originalWheelData[k].wheelPosition - wheelPos) * (Time.deltaTime * 5f);
                    else
                        wheelPos += (originalWheelData[k].wheelPosition - wheelPos);

                    //  If verticies are close enough to their original positions, repaired = true;
                    if ((originalWheelData[k].wheelPosition - wheelPos).sqrMagnitude >= (minimumVertDistanceForDamagedMesh * minimumVertDistanceForDamagedMesh))
                        repaired = false;

                    //  We were using the variable named "vertices" above, therefore we need to set the new verticies to the damaged mesh data.
                    //  Damaged mesh data also restored while repairing with this proccess.
                    damagedWheelData[k].wheelPosition = wheelPos;

                    wheels[k].transform.localPosition = wheelPos;
                    wheels[k].transform.localRotation = Quaternion.identity;

                    if (!wheels[k].gameObject.activeSelf)
                        wheels[k].gameObject.SetActive(true);

                    carController.ESPBroken = false;

                    wheels[k].Inflate();

                }

            }

            //  Repairing and restoring all detachable parts of the vehicle.
            for (int i = 0; i < detachableParts.Length; i++) {

                if (detachableParts[i] != null)
                    detachableParts[i].OnRepair();

            }

            //  Repairing and restoring all lights of the vehicle.
            for (int i = 0; i < lights.Length; i++) {

                if (lights[i] != null)
                    lights[i].OnRepair();

            }

            //  If all meshes are completely restored, make sure repairing now is false.
            if (repaired)
                repairNow = false;

        }

    }

    /// <summary>
    /// Applies pending mesh deformation by moving vertices toward their damaged target positions.
    /// Called each frame from RCC_CarControllerV4.Update() after a collision triggers deformation.
    /// In Accurate mode, vertices lerp smoothly over ~1 second; in Fast mode, they snap instantly.
    /// </summary>
    public void UpdateDamage() {

        if (!carController)
            return;

        if (!initialized)
            return;

        if (originalMeshData == null || originalMeshData.Length < 1)
            CheckMeshData();

        //  If vehicle is not deformed completely, and deforming is enabled, deform all meshes to their damaged structions.
        if (!deformed && deformingNow) {

            int k;
            deformed = true;
            deformationTime += Time.deltaTime;

            //  If deformable mesh is still exists, get all verticies of the mesh first. And then move all single verticies to the damaged positions. If verticies are close enough to the original
            //  position, deformed = true;
            for (k = 0; k < meshFilters.Length; k++) {

                MeshFilter currentMeshFilter = meshFilters[k];

                if (currentMeshFilter != null && currentMeshFilter.mesh != null) {

                    if (damagedMeshData[k].meshVerts == null)
                        continue;

                    //  Get all verticies of the mesh first.
                    Vector3[] vertices = currentMeshFilter.mesh.vertices;

                    //  And then move all single verticies to the damaged positions.
                    for (int i = 0; i < vertices.Length; i++) {

                        Vector3 targetPosition = damagedMeshData[k].meshVerts[i];

                        if (deformationMode == DeformationMode.Accurate)
                            vertices[i] += (targetPosition - vertices[i]) * (Time.deltaTime * 5f);
                        else
                            vertices[i] = (targetPosition);

                        //// If any vertex is not yet close to the damaged position, mark as not deformed
                        //if (Vector3.SqrMagnitude(targetPosition - vertices[i]) > 0.001f)
                        //    deformed = false;

                    }

                    //  Setting new verticies to the all meshes. Recalculating normals and bounds, and then optimizing. This proccess can be heavy for high poly meshes.
                    currentMeshFilter.mesh.SetVertices(vertices);

                    if (recalculateNormals)
                        currentMeshFilter.mesh.RecalculateNormals();

                    if (recalculateBounds)
                        currentMeshFilter.mesh.RecalculateBounds();

                }

            }

            for (k = 0; k < wheels.Length; k++) {

                Transform wheelTransform = wheels[k].transform;

                if (wheelTransform != null) {

                    Vector3 currentLocalPosition = wheelTransform.localPosition;
                    Vector3 targetPosition = damagedWheelData[k].wheelPosition;

                    if (deformationMode == DeformationMode.Accurate)
                        currentLocalPosition += (targetPosition - currentLocalPosition) * (Time.deltaTime * 5f);
                    else
                        currentLocalPosition += (targetPosition - currentLocalPosition);

                    wheelTransform.localPosition = currentLocalPosition;
                    //wheelTransform.localRotation = Quaternion.Euler(vertices);

                }

            }

            //  Make sure deforming proccess takes only 1 second.
            if (deformationMode == DeformationMode.Accurate && deformationTime <= 1f)
                deformed = false;

            //  If all meshes are completely deformed, make sure deforming is false and timer is set to 0.
            if (deformed) {

                deformingNow = false;
                deformationTime = 0f;

            }

        }

    }

    /// <summary>
    /// Calculates and applies vertex displacement to all tracked mesh filters based on the collision contact point.
    /// Uses an octree for fast nearest-vertex lookup and applies damage falloff based on distance from the contact.
    /// </summary>
    /// <param name="impulse">Normalized collision impulse magnitude (0-10) determining deformation severity.</param>
    private void DamageMesh(float impulse) {

        if (!carController || !initialized)
            return;

        if (meshFilters == null || (meshFilters != null && meshFilters.Length < 1))
            return;

        if (originalMeshData == null || originalMeshData.Length < 1)
            CheckMeshData();

        Transform carTransform = carController.transform;  // Cache the car's transform

        Vector3 localContactPointRelativeToRoot = carTransform.InverseTransformPoint(contactPoint);

        // Calculate the collision direction in local space and reverse it
        Vector3 collisionDirection = -(localContactPointRelativeToRoot).normalized;

        if (octrees == null || (octrees != null && octrees.Length < 1) || octrees.Length != meshFilters.Length)
            octrees = new RCC_Octree[meshFilters.Length];

        //  We will be checking all mesh filters with these contact points. If contact point is close enough to the mesh, deformation will be applied.
        for (int i = 0; i < meshFilters.Length; i++) {

            MeshFilter currentMeshFilter = meshFilters[i];

            if (currentMeshFilter == null || currentMeshFilter.mesh == null || !currentMeshFilter.gameObject.activeSelf)
                continue;

            // Create an Octree with bounds in world space
            if (octrees[i] == null) {

                octrees[i] = new RCC_Octree(currentMeshFilter);

                foreach (var vertex in currentMeshFilter.mesh.vertices)
                    octrees[i].Insert(vertex); // Insert the local-space vertex into the Octree.

            }

            Vector3 localContactPointRelativeToMesh = currentMeshFilter.transform.InverseTransformPoint(contactPoint);

            //  Getting closest point to the mesh.
            Vector3 nearestVert = NearestVertexWithOctree(i, localContactPointRelativeToMesh, currentMeshFilter);

            // Distance.
            float distance = (nearestVert - localContactPointRelativeToMesh).sqrMagnitude;

            //  If distance between contact point and closest point of the mesh is in range...
            if (distance <= (damageRadius * damageRadius)) {

                // All vertices of the mesh
                Vector3[] vertices = damagedMeshData[i].meshVerts;

                for (int k = 0; k < vertices.Length; k++) {

                    float distanceToVertSqr = (localContactPointRelativeToMesh - vertices[k]).sqrMagnitude;

                    if (distanceToVertSqr <= (damageRadius * damageRadius)) {

                        //  Calculate damage based on the impulse and distance
                        float damage = impulse * (1f - Mathf.Clamp01(Mathf.Sqrt(distanceToVertSqr) / damageRadius));

                        // Apply deformation in the local space of the mesh (in the reverse direction)
                        vertices[k] += collisionDirection * damage * (damageMultiplier / 10f);

                        //  If deformation exceeds limits, apply limits
                        if (maximumDamage > 0f && (vertices[k] - originalMeshData[i].meshVerts[k]).sqrMagnitude > (maximumDamage * maximumDamage)) {

                            vertices[k] = originalMeshData[i].meshVerts[k] + (vertices[k] - originalMeshData[i].meshVerts[k]).normalized * maximumDamage;

                        }

                    }

                }

            }

        }

    }

    /// <summary>
    /// Displaces wheel collider local positions based on collision impact. If damage exceeds
    /// the maximum threshold and wheelDetachment is enabled, the wheel is fully detached.
    /// </summary>
    /// <param name="impulse">Normalized collision impulse magnitude (0-10) determining displacement severity.</param>
    private void DamageWheel(float impulse) {

        if (!carController || !initialized)
            return;

        if (originalWheelData == null || originalWheelData.Length < 1)
            CheckWheelData();

        Transform carTransform = carController.transform;  // Cache the car's transform

        // Pre-calculate the collision direction outside the loop
        Vector3 collisionDirection = -((contactPoint - carTransform.position).normalized);

        for (int i = 0; i < wheels.Length; i++) {

            if (wheels[i] != null && wheels[i].gameObject.activeSelf) {

                Vector3 wheelPos = damagedWheelData[i].wheelPosition;

                // Calculate the closest point on the wheel collider
                Vector3 closestPoint = wheels[i].WheelCollider.ClosestPointOnBounds(contactPoint);
                float distanceSqr = (closestPoint - contactPoint).sqrMagnitude;  // Use squared magnitude for optimization

                // If the distance between the contact point and the closest point on the wheel collider is within range
                if (distanceSqr < (wheelDamageRadius * wheelDamageRadius)) {

                    float damage = (impulse * wheelDamageMultiplier) / 30f;

                    // Decrease damage based on the distance from the contact point
                    damage -= damage * Mathf.Clamp01(distanceSqr / (wheelDamageRadius * wheelDamageRadius)) * .5f;

                    Vector3 vW = carTransform.TransformPoint(wheelPos);
                    vW += (collisionDirection * damage);
                    wheelPos = carTransform.InverseTransformPoint(vW);

                    // If damage exceeds the maximum allowed, either cap the damage or detach the wheel
                    if (maximumDamage > 0 && (wheelPos - originalWheelData[i].wheelPosition).sqrMagnitude > (maximumDamage * maximumDamage)) {

                        // Uncomment this if you want to limit the damage instead of detaching
                        // wheelPos = originalWheelData[i].wheelPosition + (wheelPos - originalWheelData[i].wheelPosition).normalized * maximumDamage;

                        if (wheelDetachment && wheels[i].gameObject.activeSelf)
                            DetachWheel(wheels[i]);  // Detach the wheel if it's active and damage is over the threshold

                    }

                    // Update the damaged wheel position
                    damagedWheelData[i].wheelPosition = wheelPos;
                }
            }
        }
    }


    /// <summary>
    /// Applies damage to detachable body parts (hood, bumpers, doors) near the collision contact point.
    /// Damage decreases with distance from the contact and is multiplied by partDamageMultiplier.
    /// </summary>
    /// <param name="impulse">Normalized collision impulse magnitude (0-10) determining damage severity.</param>
    private void DamagePart(float impulse) {

        if (!carController)
            return;

        if (!initialized)
            return;

        if (detachableParts != null && detachableParts.Length >= 1) {

            for (int i = 0; i < detachableParts.Length; i++) {

                if (detachableParts[i] != null && detachableParts[i].gameObject.activeSelf) {

                    if (detachableParts[i].partCollider != null) {

                        Vector3 closestPoint = detachableParts[i].partCollider.ClosestPointOnBounds(contactPoint);

                        float distance = (closestPoint - contactPoint).sqrMagnitude;
                        float damage = impulse * partDamageMultiplier;

                        // The damage should decrease with distance from the contact point.
                        damage -= damage * Mathf.Clamp01(distance / (partDamageRadius * partDamageRadius)) * .5f;

                        if (distance <= (partDamageRadius * partDamageRadius))
                            detachableParts[i].OnCollision(damage);

                    } else {

                        if ((contactPoint - detachableParts[i].transform.position).sqrMagnitude < (partDamageRadius * partDamageRadius))
                            detachableParts[i].OnCollision(impulse);

                    }

                }

            }

        }

    }

    /// <summary>
    /// Applies damage to vehicle lights near the collision contact point.
    /// Damage decreases with distance from the contact and is multiplied by lightDamageMultiplier.
    /// </summary>
    /// <param name="impulse">Normalized collision impulse magnitude (0-10) determining damage severity.</param>
    private void DamageLight(float impulse) {

        if (!carController)
            return;

        if (!initialized)
            return;

        if (lights != null && lights.Length >= 1) {

            for (int i = 0; i < lights.Length; i++) {

                if (lights[i] != null && lights[i].gameObject.activeSelf) {

                    if ((contactPoint - lights[i].transform.position).sqrMagnitude < (lightDamageRadius * lightDamageRadius)) {

                        float distance = (lights[i].transform.position - contactPoint).sqrMagnitude;
                        float damage = impulse * lightDamageMultiplier;

                        // The damage should decrease with distance from the contact point.
                        damage -= damage * Mathf.Clamp01(distance / (lightDamageRadius * lightDamageRadius)) * .5f;

                        if (distance <= (lightDamageRadius * lightDamageRadius))
                            lights[i].OnCollision(damage);

                    }

                }

            }

        }

    }

    /// <summary>
    /// Detaches the target wheel.
    /// </summary>
    /// <param name="wheelCollider"></param>
    public void DetachWheel(RCC_WheelCollider wheelCollider) {

        if (!carController)
            return;

        if (!initialized)
            return;

        if (!wheelCollider)
            return;

        if (!wheelCollider.gameObject.activeSelf)
            return;

        wheelCollider.gameObject.SetActive(false);
        Transform wheelModel = wheelCollider.wheelModel;

        GameObject clonedWheel = GameObject.Instantiate(wheelModel.gameObject, wheelModel.transform.position, wheelModel.transform.rotation, null);
        clonedWheel.SetActive(true);
        Rigidbody clonedWheelRB = clonedWheel.AddComponent<Rigidbody>();
        clonedWheelRB.mass = 20f;
        clonedWheelRB.linearDamping = .01f;
        clonedWheelRB.angularDamping = .1f;
        clonedWheelRB.interpolation = RigidbodyInterpolation.Interpolate;

        GameObject clonedMeshCollider = new GameObject("Mesh Collider");
        clonedMeshCollider.transform.SetParent(clonedWheel.transform, false);
        clonedMeshCollider.transform.position = RCC_GetBounds.GetBoundsCenter(clonedWheel.transform);

        MeshFilter biggestMesh = RCC_GetBounds.GetBiggestMesh(clonedWheel.transform);

        if (biggestMesh) {

            MeshCollider mc = clonedMeshCollider.AddComponent<MeshCollider>();
            mc.sharedMesh = biggestMesh.mesh;
            mc.convex = true;

        }

        carController.ESPBroken = true;

    }

    /// <summary>
    /// Processes a physics collision event, calculating impulse and applying damage to meshes, wheels,
    /// lights, and detachable parts within range of the contact points. Called from RCC_CarControllerV4.OnCollisionEnter().
    /// </summary>
    /// <param name="collision">The Unity Collision data containing contact points and impulse information.</param>
    public void OnCollision(Collision collision) {

        if (!carController)
            return;

        if (!initialized)
            return;

        if (!carController.useDamage)
            return;

        if (((1 << collision.gameObject.layer) & damageFilter) != 0) {

            float impulse = collision.impulse.magnitude / 10000f;

            if (collision.rigidbody)
                impulse *= collision.rigidbody.mass / 1000f;

            if (impulse < minimumCollisionImpulse)
                impulse = 0f;

            if (impulse > 10f)
                impulse = 10f;

            if (impulse > 0f) {

                deformingNow = true;
                deformed = false;

                repairNow = false;
                repaired = false;

                //  First, we are getting all contact points.
                ContactPoint[] contacts = collision.contacts;
                contactPoints = new Vector3[contacts.Length];

                for (int i = 0; i < contactPoints.Length; i++)
                    contactPoints[i] = contacts[i].point;

                contactPoint = ContactPointsMagnitude(contactPoints);

                if (meshFilters != null && meshFilters.Length >= 1 && meshDeformation)
                    DamageMesh(impulse);

                if (wheels != null && wheels.Length >= 1 && wheelDamage)
                    DamageWheel(impulse);

                if (detachableParts != null && detachableParts.Length >= 1 && partDamage)
                    DamagePart(impulse);

                if (lights != null && lights.Length >= 1 && lightDamage)
                    DamageLight(impulse);

            }

        }

    }

    /// <summary>
    /// Processes damage from a raycast hit rather than a physics collision. Useful for applying
    /// external damage (e.g., projectile impacts) without a full Unity Collision event.
    /// </summary>
    /// <param name="hit">The RaycastHit data containing the point of impact.</param>
    /// <param name="impulse">The damage impulse magnitude to apply (will be clamped to 0-10).</param>
    public void OnCollisionWithRay(RaycastHit hit, float impulse) {

        if (!carController)
            return;

        if (!initialized)
            return;

        if (!carController.useDamage)
            return;

        if (impulse < minimumCollisionImpulse)
            impulse = 0f;

        if (impulse > 10f)
            impulse = 10f;

        if (impulse > 0f) {

            deformingNow = true;
            deformed = false;

            repairNow = false;
            repaired = false;

            //  First, we are getting all contact points.
            contactPoint = hit.point;

            if (meshFilters != null && meshFilters.Length >= 1 && meshDeformation)
                DamageMesh(impulse);

            if (wheels != null && wheels.Length >= 1 && wheelDamage)
                DamageWheel(impulse);

            if (detachableParts != null && detachableParts.Length >= 1 && partDamage)
                DamagePart(impulse);

            if (lights != null && lights.Length >= 1 && lightDamage)
                DamageLight(impulse);

        }

    }

    /// <summary>
    /// Calculates the average (centroid) of the given contact points to produce a single representative impact point.
    /// </summary>
    /// <param name="givenContactPoints">Array of world-space contact points from the collision.</param>
    /// <returns>The averaged world-space position of all contact points.</returns>
    private Vector3 ContactPointsMagnitude(Vector3[] givenContactPoints) {

        Vector3 magnitude = Vector3.zero;

        if (givenContactPoints.Length == 0)
            return magnitude;

        for (int i = 0; i < givenContactPoints.Length; i++)
            magnitude += givenContactPoints[i];

        magnitude /= givenContactPoints.Length;

        return magnitude;

    }

    /// <summary>
    /// Finds the closest vertex to the given contact point using the octree spatial index for the specified mesh.
    /// </summary>
    /// <param name="meshIndex">Index into the octrees and meshFilters arrays identifying which mesh to search.</param>
    /// <param name="contactPoint">The contact point in the mesh's local space to find the nearest vertex to.</param>
    /// <param name="meshFilter">The mesh filter whose vertices are being searched.</param>
    /// <returns>The local-space position of the nearest vertex, or Vector3.zero if the octree is invalid.</returns>
    public Vector3 NearestVertexWithOctree(int meshIndex, Vector3 contactPoint, MeshFilter meshFilter) {

        if (meshIndex < 0 || meshIndex >= octrees.Length || octrees[meshIndex] == null) {

#if UNITY_EDITOR
            Debug.LogWarning("Invalid Octree or mesh index.");
#endif
            return Vector3.zero;

        }

        return octrees[meshIndex].FindNearestVertex(contactPoint, meshFilter);

    }

    /// <summary>
    /// Cleans up octrees and other data structures, resets all states to default values.
    /// </summary>
    public void Cleanup() {

        // Stop any ongoing processes
        deformingNow = false;
        deformed = true;
        repairNow = false;
        repaired = true;
        deformationTime = 0f;

        // Reset contact points
        contactPoint = Vector3.zero;
        contactPoints = null;

        // Clean up octrees
        if (octrees != null) {

            for (int i = 0; i < octrees.Length; i++) {

                if (octrees[i] != null) {

                    octrees[i].Clear();
                    octrees[i] = null;

                }

            }

            octrees = null;

        }

        // Clean up any detached wheels that might have been created
        // Note: You might want to tag detached wheels for easier cleanup
        // For now, we'll just ensure references are cleared

        // Reset arrays
        originalMeshData = null;
        damagedMeshData = null;
        originalWheelData = null;
        damagedWheelData = null;

        // Reset component references
        meshFilters = null;
        detachableParts = null;
        lights = null;
        wheels = null;

        // Reset the car controller reference
        carController = null;

        // Reset initialization flag
        initialized = false;

    }

}