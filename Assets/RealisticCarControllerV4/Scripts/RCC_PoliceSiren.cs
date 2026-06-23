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
/// Alternating red and blue police siren lights. Automatically activates when attached to an AI vehicle
/// that is in chase mode. Can also be manually toggled via <see cref="SetSiren"/>.
/// </summary>
public class RCC_PoliceSiren : RCC_Core {

    /// <summary>
    /// Reference to the AI car controller, if this siren is on an AI vehicle.
    /// </summary>
    private RCC_AICarController AI;

    /// <summary>
    /// Current siren state (On or Off).
    /// </summary>
    [Tooltip("Current siren state (On or Off).")]
    public SirenMode sirenMode = SirenMode.Off;

    /// <summary>
    /// Siren operating modes.
    /// </summary>
    public enum SirenMode { Off, On }

    /// <summary>
    /// Array of red siren Light components.
    /// </summary>
    [Tooltip("Array of red siren Light components.")]
    public Light[] redLights;

    /// <summary>
    /// Array of blue siren Light components.
    /// </summary>
    [Tooltip("Array of blue siren Light components.")]
    public Light[] blueLights;

    private void Awake() {

        //  Getting AI controller if attached.
        AI = GetComponentInParent<RCC_AICarController>(true);

    }

    private void Update() {

        //  If AI found, enable or disable the siren lights with chase mode. If AI is chasing a target, siren will be enabled.
        if (AI) {

            if (AI.targetChase != null)
                sirenMode = SirenMode.On;
            else
                sirenMode = SirenMode.Off;

        }

        //  If siren mode is set to off, set all intensity of the lights to 0. Otherwise, set to 1 with timer.
        switch (sirenMode) {

            case SirenMode.Off:

                if (redLights != null) {
                    for (int i = 0; i < redLights.Length; i++) {
                        if (redLights[i] != null)
                            redLights[i].intensity = Mathf.Lerp(redLights[i].intensity, 0f, Time.deltaTime * 50f);
                    }
                }

                if (blueLights != null) {
                    for (int i = 0; i < blueLights.Length; i++) {
                        if (blueLights[i] != null)
                            blueLights[i].intensity = Mathf.Lerp(blueLights[i].intensity, 0f, Time.deltaTime * 50f);
                    }
                }

                break;

            case SirenMode.On:

                if (Mathf.Approximately((int)(Time.time) % 2, 0) && Mathf.Approximately((int)(Time.time * 20) % 3, 0)) {

                    if (redLights != null) {
                        for (int i = 0; i < redLights.Length; i++) {
                            if (redLights[i] != null)
                                redLights[i].intensity = Mathf.Lerp(redLights[i].intensity, 1f, Time.deltaTime * 50f);
                        }
                    }

                } else {

                    if (redLights != null) {
                        for (int i = 0; i < redLights.Length; i++) {
                            if (redLights[i] != null)
                                redLights[i].intensity = Mathf.Lerp(redLights[i].intensity, 0f, Time.deltaTime * 10f);
                        }
                    }

                    if (Mathf.Approximately((int)(Time.time * 20) % 3, 0)) {

                        if (blueLights != null) {
                            for (int i = 0; i < blueLights.Length; i++) {
                                if (blueLights[i] != null)
                                    blueLights[i].intensity = Mathf.Lerp(blueLights[i].intensity, 1f, Time.deltaTime * 50f);
                            }
                        }

                    } else {

                        if (blueLights != null) {
                            for (int i = 0; i < blueLights.Length; i++) {
                                if (blueLights[i] != null)
                                    blueLights[i].intensity = Mathf.Lerp(blueLights[i].intensity, 0f, Time.deltaTime * 10f);
                            }
                        }

                    }

                }

                break;

        }

    }

    /// <summary>
    /// Enables or disables the siren lights.
    /// </summary>
    /// <param name="state">True to activate siren, false to deactivate.</param>
    public void SetSiren(bool state) {

        if (state)
            sirenMode = SirenMode.On;
        else
            sirenMode = SirenMode.Off;

    }

}
