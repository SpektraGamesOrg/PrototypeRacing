//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A ScriptableObject that stores the default wheel friction and suspension settings for RCC vehicles,
/// along with various Rigidbody parameters. These settings serve as a baseline for newly created RCC vehicles
/// and can be tweaked to achieve different handling characteristics.
/// </summary>
[System.Serializable]
public class RCC_InitialSettings : ScriptableObject {

    #region Singleton

    /// <summary>
    /// Cached singleton reference to RCC_InitialSettings. 
    /// Automatically loads from "RCC Assets/RCC_InitialSettings" if not already set.
    /// </summary>
    private static RCC_InitialSettings instance;

    /// <summary>
    /// Public accessor for the singleton instance of RCC_InitialSettings.
    /// </summary>
    public static RCC_InitialSettings Instance {

        get {

            if (instance == null)
                instance = Resources.Load("RCC Assets/RCC_InitialSettings") as RCC_InitialSettings;

            return instance;

        }

    }

    #endregion

    // Wheel friction settings for forward motion.
    /// <summary>
    /// The slip value at which maximum friction is reached for forward motion. Lower values can trigger slides or spins earlier.
    /// </summary>
    [Tooltip("The slip value at which maximum friction is reached for forward motion. Lower values can trigger slides or spins earlier.")]
    [Header("Wheel Frictions Forward")]
    [Min(0f)]
    public float forwardExtremumSlip = .375f;

    /// <summary>
    /// The friction force value at the extremum slip point for forward motion.
    /// </summary>
    [Tooltip("The friction force value at the extremum slip point for forward motion.")]
    [Min(0f)]
    public float forwardExtremumValue = 1f;

    /// <summary>
    /// The slip value at which friction starts to fall off for forward motion.
    /// </summary>
    [Tooltip("The slip value at which friction starts to fall off for forward motion.")]
    [Min(0f)]
    public float forwardAsymptoteSlip = .8f;

    /// <summary>
    /// The friction force value at the asymptote slip point for forward motion.
    /// </summary>
    [Tooltip("The friction force value at the asymptote slip point for forward motion.")]
    [Min(0f)]
    public float forwardAsymptoteValue = .5f;

    /// <summary>
    /// Multiplies the overall grip level in the forward direction.
    /// </summary>
    [Tooltip("Multiplies the overall grip level in the forward direction.")]
    [Min(0f)]
    public float forwardStiffness = 1.25f;

    // Wheel friction settings for sideways/lateral motion.
    /// <summary>
    /// The slip value at which maximum friction is reached for sideways motion.
    /// </summary>
    [Tooltip("The slip value at which maximum friction is reached for sideways motion.")]
    [Header("Wheel Frictions Sideways")]
    [Min(0f)]
    public float sidewaysExtremumSlip = .275f;

    /// <summary>
    /// The friction force value at the extremum slip point for sideways motion.
    /// </summary>
    [Tooltip("The friction force value at the extremum slip point for sideways motion.")]
    [Min(0f)]
    public float sidewaysExtremumValue = 1f;

    /// <summary>
    /// The slip value at which friction starts to fall off for sideways motion.
    /// </summary>
    [Tooltip("The slip value at which friction starts to fall off for sideways motion.")]
    [Min(0f)]
    public float sidewaysAsymptoteSlip = .5f;

    /// <summary>
    /// The friction force value at the asymptote slip point for sideways motion.
    /// </summary>
    [Tooltip("The friction force value at the asymptote slip point for sideways motion.")]
    [Min(0f)]
    public float sidewaysAsymptoteValue = .75f;

    /// <summary>
    /// Multiplies the overall grip level in the lateral/sideways direction.
    /// </summary>
    [Tooltip("Multiplies the overall grip level in the lateral/sideways direction.")]
    [Min(0f)]
    public float sidewaysStiffness = 1.25f;

    // Suspension settings for wheels.
    /// <summary>
    /// The spring force applied to each wheel. Higher values result in stiffer suspension.
    /// </summary>
    [Tooltip("The spring force applied to each wheel. Higher values result in stiffer suspension.")]
    [Header("Wheel Suspensions")]
    [Min(0f)]
    public float suspensionSpring = 45000f;

    /// <summary>
    /// The damping force to control oscillations in the suspension. 
    /// Higher values reduce bounciness.
    /// </summary>
    [Tooltip("The damping force to control oscillations in the suspension. Higher values reduce bounciness.")]
    [Min(0f)]
    public float suspensionDamping = 2500f;

    /// <summary>
    /// The vertical distance the suspension can travel. Higher values create more suspension travel.
    /// </summary>
    [Tooltip("The vertical distance the suspension can travel. Higher values create more suspension travel.")]
    [Min(0f)]
    public float suspensionDistance = .2f;

    /// <summary>
    /// The distance from the wheel’s center at which the suspension forces are applied. 
    /// A larger value can help stabilize the vehicle in certain conditions.
    /// </summary>
    [Tooltip("The distance from the wheel’s center at which the suspension forces are applied. A larger value can help stabilize the vehicle in certain conditions.")]
    [Min(0f)]
    public float forceAppPoint = .1f;

    // Rigidbody settings for vehicles.
    /// <summary>
    /// The mass of the vehicle's main Rigidbody (in kilograms).
    /// </summary>
    [Tooltip("The mass of the vehicle's main Rigidbody (in kilograms).")]
    [Header("Rigidbody")]
    [Min(0f)]
    public float mass = 1500;

    /// <summary>
    /// The linear drag applied to the vehicle's Rigidbody. 
    /// Higher values cause the vehicle to slow down faster without input.
    /// </summary>
    [Tooltip("The linear drag applied to the vehicle's Rigidbody. Higher values cause the vehicle to slow down faster without input.")]
    [Min(0f)]
    public float drag = .01f;

    /// <summary>
    /// The rotational drag applied to the vehicle's Rigidbody. 
    /// Higher values reduce angular momentum more quickly.
    /// </summary>
    [Tooltip("The rotational drag applied to the vehicle's Rigidbody. Higher values reduce angular momentum more quickly.")]
    [Min(0f)]
    public float angularDrag = .5f;

    /// <summary>
    /// The interpolation mode of the vehicle's Rigidbody (e.g., None, Interpolate, Extrapolate).
    /// </summary>
    [Tooltip("The interpolation mode of the vehicle's Rigidbody (e.g., None, Interpolate, Extrapolate).")]
    public RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;

    /// <summary>
    /// The collision detection mode (e.g., Discrete, Continuous, ContinuousDynamic) used by the vehicle's Rigidbody.
    /// </summary>
    [Tooltip("The collision detection mode (e.g., Discrete, Continuous, ContinuousDynamic) used by the vehicle's Rigidbody.")]
    public CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Discrete;

}
