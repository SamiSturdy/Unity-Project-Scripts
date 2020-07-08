using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteraction : MonoBehaviour {

    private Animator animator;
    private bool isOpen;
    private bool activeDoor;
    public bool isShallowDoor;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("Open", false);
        animator.SetBool("Shallow Door", isShallowDoor);
        isOpen = false;
        activeDoor = false;
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetButtonDown("Interact") && activeDoor) {

            if (!isOpen) {

                Debug.Log("Open");
                animator.SetBool("Open", true);
                isOpen = true;

            } else {

                animator.SetBool("Open", false);
                isOpen = false;

            }

        }

    }

    private void OnTriggerEnter(Collider other) {
        activeDoor = true;

        if (other.gameObject.CompareTag("Enemy")) {
            animator.SetBool("Open", true);
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        activeDoor = false;
    }

}
