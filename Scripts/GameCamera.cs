using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour {

    private Vector3 cameraTarget;
    private Transform target;
    private bool isCameraZoomedOut;
    private float cameraHeightTarget;

    // Start is called before the first frame update
    void Start() {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        isCameraZoomedOut = false;
        cameraHeightTarget = 15f;
    }

    // Update is called once per frame
    void Update() {

        //Check if camera should zoom out
        ZoomCamera();

        //Find the position of player in the world
        cameraTarget = new Vector3(target.position.x, cameraHeightTarget, target.position.z);

        //Progressively move the camera towards the player's current position
        transform.position = Vector3.Lerp(transform.position, cameraTarget, Time.deltaTime * 16);

    }

    void ZoomCamera() {

        //If player presses z, toggle the distance of the camera from the player
        if (Input.GetKeyDown("z") && !isCameraZoomedOut) {

            cameraHeightTarget = 30f;
            isCameraZoomedOut = true;

        } else if (Input.GetKeyDown("z") && isCameraZoomedOut) {

            cameraHeightTarget = 15f;
            isCameraZoomedOut = false;
        }

    }

}
