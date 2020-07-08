using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent (typeof (CharacterController))]
public class PlayerController : MonoBehaviour {

    public float rotationSpeed = 750;
    public float speed = 5;
    public float health = 20;

    private Quaternion rotationToMouse;
    private Vector3 incrementedInput;

    public Transform handPosition;
    public List<Gun> weapons;
    private Gun equippedWeapon;
    private CharacterController controller;
    private Camera cam;
    private Animator animator;

    // Start is called before the first frame update
    private void Start() {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cam = Camera.main;

        EquipWeapon(0);

    }

    // Update is called once per frame
    private void Update() {

        Movement();

        LookAtMouse();

        //If the player hits the shoot button, shoot with the currently equipped weapon
        if (Input.GetButtonDown("Shoot")) {
            equippedWeapon.Shoot();
        } 


        //If the player presses 1 equip the rifle, if they press 2, equip the pistol
        if (Input.GetKeyDown("1")) {

            EquipWeapon(0);

        } else if (Input.GetKeyDown("2")) {

            
            EquipWeapon(1);

        }

        //If the player hits escape, quit the game
        if (Input.GetKeyDown(KeyCode.Escape)) {

            Application.Quit();

        }

        //Boost the players health to 1000 (intended for demonstration purposes)
        if (Input.GetButtonDown("Health Boost")) {
            health = 1000;
        }

        //Restart the game
        if (Input.GetButtonDown("Restart")) {

            SceneManager.LoadScene("MainLevel");

        }

    }

    //Rotates player character to face position of mouse on screen
    private void LookAtMouse() {

        //Find the position of the mouse on the screen
        Vector3 mousePos = Input.mousePosition;

        //Find the position of the mouse in the 3D world
        mousePos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.transform.position.y - transform.position.y));

        //Using the position of the mouse in the 3D world, find the rotation adjustment necessary to look at it
        rotationToMouse = Quaternion.LookRotation(mousePos - new Vector3(transform.position.x, 0, transform.position.z));

        //Using euler angles (x,y,z rotations) rotate the player on the y axis towards the mouse
        transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, rotationToMouse.eulerAngles.y, rotationSpeed * Time.deltaTime);

    }

    private void Movement() {

        //Record current movement input as a vector
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        //Normalise magnitude of inputs to avoid diagonal speed increase
        input = input.normalized;

        //Adjust input by incrementing towards goal input to simulate acceleration
        //incrementedInput = Vector3.MoveTowards(incrementedInput, input, acceleration * Time.deltaTime);

        //Vector3 velocity = incrementedInput;
        Vector3 velocity = input;

        //Set movement speed multiplier dependent on run button
        //velocity *= (Input.GetButton("Run")) ? runSpeed : walkSpeed;

        //Multiply normalised velocity by walking speed multiplier
        velocity *= speed;

        //Basic gravity
        velocity += Vector3.up * -8;

        //Move object
        controller.Move(velocity * Time.deltaTime);

        //Find vector representing the direction the player is facing
        float facingAngle = transform.eulerAngles.y * Mathf.Deg2Rad;
        Vector3 facingVector = new Vector3(Mathf.Sin(facingAngle), 0, Mathf.Cos(facingAngle));

        //Find the dot product of the direction vectors of player movement and direction player is facing
        //which indicates if player is moving forward or backwards relative to the direction they're facing
        float dot = Vector3.Dot(facingVector, input);

        //Find the Y value of the cross product of the direction vectors of player movement and direction player is facing
        //which indicates if player is moving left or right relative to the direction they're facing
        float crossY = Vector3.Cross(facingVector, input).y;

        //Pass data required to play correct animations to animator object
        animator.SetFloat("Dot Product", dot);

        animator.SetFloat("Cross Y", crossY);

        if (Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z) > 0) {

            animator.SetBool("Moving", true);

        } else {

            animator.SetBool("Moving", false);

        }

    }


    private void EquipWeapon (int i) {

        //If the player currently has an equipped weapon, destroy it
        if (equippedWeapon) {
            Destroy(equippedWeapon.gameObject);
        }

        //Equip the player with their chosen weapon
        equippedWeapon = Instantiate(weapons[i], handPosition.position, handPosition.rotation);
        equippedWeapon.transform.parent = handPosition;
        animator.SetInteger("Weapon ID", equippedWeapon.gunID);

    }

    public void TakeDamage(float damage) {

        //Reduce health by the damage value of the weapon hit by
        health -= damage;

        //If player's health is 0 or less, die.
        if (health <= 0) {
            Die();
        }

    }

    private void Die() {

        //Restart the game
        SceneManager.LoadScene("MainLevel");

    }
    
}
