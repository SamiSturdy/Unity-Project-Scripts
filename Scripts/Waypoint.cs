using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {

    public bool isCoverWayPoint;

    //When gizmos (used for debugging) are turned on in the unity editor
    //draw a sphere at the world location of the waypoint object.
    //The sphere should be red if a standard waypoint and yellow if a cover waypoint
    void OnDrawGizmos() {

        if (isCoverWayPoint) {
            Gizmos.color = Color.yellow;
        } else {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawSphere(transform.position, 0.3f);

    }

}
