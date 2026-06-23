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
/// Represents a single waypoint node in an AI driving path. Each waypoint defines a target speed
/// the AI should aim for when approaching it and a radius within which the waypoint is considered reached.
/// Waypoints are children of an <see cref="RCC_AIWaypointsContainer"/>.
/// </summary>
public class RCC_Waypoint : RCC_Core {

    /// <summary>
    /// Target speed (km/h) the AI vehicle should aim for when approaching this waypoint. Lower values cause the AI to brake before reaching the waypoint.
    /// </summary>
    [Tooltip("Target speed (km/h) the AI vehicle should aim for when approaching this waypoint. Lower values cause the AI to brake before reaching the waypoint.")]
    [Min(0f)]
    public float targetSpeed = 240f;

    /// <summary>
    /// Distance (in meters) from this waypoint's position at which the AI considers the waypoint reached and advances to the next one.
    /// </summary>
    [Tooltip("Distance (in meters) from this waypoint's position at which the AI considers the waypoint reached and advances to the next one.")]
    [Min(0f)]
    public float radius = 20f;

}
