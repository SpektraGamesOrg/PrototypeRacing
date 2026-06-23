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
/// Represents a destructible environment prop (e.g., traffic cones, barriers, crates).
/// Automatically assigns itself to the configured prop layer and destroys itself after a
/// sufficiently strong collision impact.
/// </summary>
public class RCC_Prop : RCC_Core {

    /// <summary>
    /// Time in seconds after a strong collision before this prop is destroyed. Set to 0 or below to disable auto-destruction.
    /// </summary>
    [Tooltip("Time in seconds after a strong collision before this prop is destroyed. Set to 0 or below to disable auto-destruction.")]
    public float destroyAfterCollision = 3f;

    /// <summary>
    /// Assigns the prop to the configured prop layer on enable and puts the rigidbody to sleep to save physics overhead.
    /// </summary>
    private void OnEnable() {

        if (Settings.setLayers && Settings.PropLayer != "")
            gameObject.layer = LayerMask.NameToLayer(Settings.PropLayer);

        if (TryGetComponent(out Rigidbody rigid))
            rigid.Sleep();

    }

    /// <summary>
    /// Editor callback that assigns the prop layer and configures layer exclusions when the component is first added or reset.
    /// </summary>
    private void Reset() {

        if (Settings.setLayers && Settings.PropLayer != "")
            gameObject.layer = LayerMask.NameToLayer(Settings.PropLayer);

#if UNITY_2022_2_OR_NEWER
        IgnoreLayers();
#endif

    }

#if UNITY_2022_2_OR_NEWER
    /// <summary>
    /// Configures all child colliders to exclude the WheelCollider layer, preventing unwanted physics interactions.
    /// </summary>
    private void IgnoreLayers() {

        Collider[] partColliders = GetComponentsInChildren<Collider>(true);

        LayerMask curLayerMask = -1;

        foreach (Collider collider in partColliders) {

            curLayerMask = collider.excludeLayers;
            curLayerMask |= (1 << LayerMask.NameToLayer(Settings.WheelColliderLayer));
            collider.excludeLayers = curLayerMask;

        }

    }
#endif

    /// <summary>
    /// Schedules destruction of this prop if the collision impulse is strong enough and destroyAfterCollision is positive.
    /// </summary>
    /// <param name="collision">The collision data from the physics engine.</param>
    private void OnCollisionEnter(Collision collision) {

        if (destroyAfterCollision <= 0 || collision.impulse.magnitude < 100)
            return;

        Destroy(gameObject, destroyAfterCollision);

    }

}
