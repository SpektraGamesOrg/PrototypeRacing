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
using System.Linq;

/// <summary>
/// Container that holds and manages a list of <see cref="RCC_Waypoint"/> nodes defining the AI driving path.
/// Automatically collects child waypoints on Awake and provides Editor gizmo visualization of the route
/// (waypoint spheres, radii, and connecting lines).
/// </summary>
public class RCC_AIWaypointsContainer : RCC_Core {

    /// <summary>
    /// Ordered list of all waypoints that define the AI driving path. Populated automatically from child GameObjects on Awake.
    /// </summary>
    [Tooltip("Ordered path AI vehicles follow. AI cycles through waypoints in this order. Add child Waypoint GameObjects under this container — they're auto-collected on Awake.")]
    public List<RCC_Waypoint> waypoints = new List<RCC_Waypoint>();

    private void Awake() {

        //  Getting waypoints and adding them to the list.
        GetAllWaypoints();

    }

    /// <summary>
    /// Clears and repopulates the <see cref="waypoints"/> list by collecting all <see cref="RCC_Waypoint"/> components from child GameObjects.
    /// </summary>
    public void GetAllWaypoints() {

        if (waypoints == null)
            waypoints = new List<RCC_Waypoint>();

        waypoints.Clear();

        waypoints = GetComponentsInChildren<RCC_Waypoint>(true).ToList();

    }

    /// <summary>
    /// Draws Editor gizmos to visualize the waypoint path: solid spheres at each waypoint position,
    /// wire spheres showing the pass radius, and green lines connecting sequential waypoints in a loop.
    /// </summary>
    private void OnDrawGizmos() {

        //  If waypoints list is null, return.
        if (waypoints == null)
            return;

        //  Counting all waypoints.
        for (int i = 0; i < waypoints.Count; i++) {

            //  If current waypoint is not null, continue.
            if (waypoints[i] != null) {

                //  Drawing gizmos.
                Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.3f);
                Gizmos.DrawSphere(waypoints[i].transform.position, 2);
                Gizmos.DrawWireSphere(waypoints[i].transform.position, waypoints[i].radius);

                //  If current waypoint is not last waypoint, draw connecting line.
                if (i < waypoints.Count - 1 && waypoints[i + 1]) {

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position);

                }

            }

        }

        //  Draw closing loop line from last to first waypoint.
        if (waypoints.Count >= 2 && waypoints[waypoints.Count - 1] && waypoints[0]) {

            Gizmos.color = Color.green;
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].transform.position, waypoints[0].transform.position);

        }

    }

}
