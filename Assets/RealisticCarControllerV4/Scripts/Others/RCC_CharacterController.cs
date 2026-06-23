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
/// Animates Driver Sofie (Credits to 3DMaesen). Simply feeds floats and bools of Sofie's animator component
/// based on the parent vehicle's steering, gear shifting, collision, and reversing states.
/// </summary>
public class RCC_CharacterController : RCC_Core {

    /// <summary>
    /// Animator component used to drive the character's blend tree and state machine.
    /// </summary>
    [Tooltip("Animator component used to drive the character's blend tree and state machine.")]
    public Animator animator;

    /// <summary>
    /// Animator parameter name for the steering float (maps to vehicle steer input).
    /// </summary>
    [Header("Animator Parameters")]
    [Tooltip("Animator parameter name for the steering float (maps to vehicle steer input).")]
    public string driverSteeringParameter;

    /// <summary>
    /// Animator parameter name for the gear-shifting bool (true while the vehicle is changing gears).
    /// </summary>
    [Tooltip("Animator parameter name for the gear-shifting bool (true while the vehicle is changing gears).")]
    public string driverShiftingGearParameter;

    /// <summary>
    /// Animator parameter name for the danger/impact bool (true when a collision occurs).
    /// </summary>
    [Tooltip("Animator parameter name for the danger/impact bool (true when a collision occurs).")]
    public string driverDangerParameter;

    /// <summary>
    /// Animator parameter name for the reversing bool (true when the vehicle is moving backward).
    /// </summary>
    [Tooltip("Animator parameter name for the reversing bool (true when the vehicle is moving backward).")]
    public string driverReversingParameter;

    /// <summary>
    /// Smoothed steering input value fed to the animator each frame. Range: -1 (left) to 1 (right).
    /// </summary>
    [Header("Runtime State")]
    [Tooltip("Smoothed steering input value fed to the animator each frame. Range: -1 (left) to 1 (right).")]
    public float steerInput = 0f;

    /// <summary>
    /// Local-space forward velocity of the vehicle. Negative values indicate backward movement.
    /// </summary>
    [Tooltip("Local-space forward velocity of the vehicle. Negative values indicate backward movement.")]
    public float directionInput = 0f;

    /// <summary>
    /// Whether the vehicle is currently moving in reverse.
    /// </summary>
    [Tooltip("Whether the vehicle is currently moving in reverse.")]
    public bool reversing = false;

    /// <summary>
    /// Collision impact intensity value (0 to 1). Set to 1 on impact, then decays over time.
    /// </summary>
    [Tooltip("Collision impact intensity value (0 to 1). Set to 1 on impact, then decays over time.")]
    [Range(0f, 1f)]
    public float impactInput = 0f;

    /// <summary>
    /// Gear-shifting intensity value (0 to 1). Set to 1 during gear changes, then decays over time.
    /// </summary>
    [Tooltip("Gear-shifting intensity value (0 to 1). Set to 1 during gear changes, then decays over time.")]
    [Range(0f, 1f)]
    public float gearInput = 0f;

    private void Update() {

        if (!CarController || !animator)
            return;

        //  Getting steer input.
        steerInput = Mathf.Lerp(steerInput, CarController.steerInput, Time.deltaTime * 5f);
        directionInput = CarController.transform.InverseTransformDirection(CarController.Rigid.linearVelocity).z;
        impactInput -= Time.deltaTime * 5f;

        //  Clamping impact input.
        if (impactInput < 0)
            impactInput = 0f;
        if (impactInput > 1)
            impactInput = 1f;

        //  If vehicle is going backwards or not.
        if (directionInput <= -2f)
            reversing = true;
        else if (directionInput > -1f)
            reversing = false;

        //  If changing gear.
        if (CarController.changingGear)
            gearInput = 1f;
        else
            gearInput -= Time.deltaTime * 5f;

        //  Clamping gear input.
        if (gearInput < 0)
            gearInput = 0f;
        if (gearInput > 1)
            gearInput = 1f;

        //  If reversing.
        if (!reversing)
            animator.SetBool(driverReversingParameter, false);
        else
            animator.SetBool(driverReversingParameter, true);

        //  If impact is high enough, animate collision animation by setting bool.
        if (impactInput > .5f)
            animator.SetBool(driverDangerParameter, true);
        else
            animator.SetBool(driverDangerParameter, false);

        //  If changing gear, animate change gear animation by setting bool.
        if (gearInput > .5f)
            animator.SetBool(driverShiftingGearParameter, true);
        else
            animator.SetBool(driverShiftingGearParameter, false);

        //  Setting steer input of the animator by setting float.
        animator.SetFloat(driverSteeringParameter, steerInput);

    }

    private void OnCollisionEnter(Collision col) {

        //  If collision is not high enough, return.
        if (col.relativeVelocity.magnitude < 2.5f)
            return;

        //  Setting impact to 1 on collisions.
        impactInput = 1f;

    }

}
