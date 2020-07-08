using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {

    //Parameters Affected By Stress
    [Header("Stress-Based Behaviour Attributes")]
    [Range(0f, 1f)]
    public float stressLevel;
    private float initalStressLevel;
    public float detectionDistance;
    public float inaccuracy;
    public float rotationSpeed;
    public float reactionTime;

    //Behaviour State Parameters
    [Header("Behaviour Attributes")]
    public float health;
    public float defensiveHealthLimit;
    public float desiredDistanceToPlayer;
    public float communicationDistance;
    private float alertedTimeCounter;
    private float distanceToPlayer;
    private bool isTargetAcquired;
    private bool isAlerted;
    private bool isEngaging;
    private bool isInvestigating;

    //Patrol Behaviour Parameters
    [Header("Patrol Behaviour")]
    public List<Waypoint> patrolWaypoints;
    public Waypoint coverWaypoint;
    private bool isMovingBetweenWaypoints;
    private bool isWaitingAtWaypoint;
    private bool isFirstPatrol;
    private int waypointIndex;
    private float waypointWaitTime;
    private float waitTimeCounter;

    //Referenced Game Objects
    [Header("Game Objects")]
    public EnemyGun equippedWeapon;
    public Transform handPosition;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private GameObject player;
    private Vector3 playerLastKnownPosition;
    

    // Start is called before the first frame update
    private void Start() {

        //Attach components to variables
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");

        //Equip the enemy with their weapon
        equippedWeapon = Instantiate(equippedWeapon, handPosition.position, handPosition.rotation);
        equippedWeapon.transform.parent = handPosition;

        //Set variables to default values
        SetRagdollRigidBodies(true);
        SetRagdollColliders(false);
        isTargetAcquired = false;
        isAlerted = false;
        isEngaging = false;
        isFirstPatrol = true;
        isMovingBetweenWaypoints = false;
        isWaitingAtWaypoint = false;
        waypointIndex = 0;
        waypointWaitTime = 3;

        //Randomly select a baseline stress level
        stressLevel = Random.Range(0f, 1f);

        //Initially set stress-related variables to their correct values
        SimulateStress();

        //Record the initial stress level
        initalStressLevel = stressLevel;

    }

    // Update is called once per frame
    private void Update() {

        //Find how far the enemy is from the player
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        //If the enemy is alive:
        if (navMeshAgent.enabled == true) {

            //If the enemy has been alerted:
            if (isAlerted) {

                //If the enemy is currently engaging the player use the engagement behaviour
                //otherwise check if enemy has reason to investigate
                if (isEngaging) {

                    EngagementBehavior();

                //If the enemy has reason to investigate the players last known location use engagement behaviour
                //otherwise use standard alerted behaviour by looking towards player's last known position and
                //continuing the patrol
                } else if (isInvestigating) {

                    //If the enemy has most of their health and they are not excessively stressed
                    //investigate the player's last known position, otherwise look towards it.
                    if (health > defensiveHealthLimit && stressLevel < 0.9f) {

                        navMeshAgent.SetDestination(playerLastKnownPosition);
                        LookAtPlayerLastKnownPosition();

                    } else {

                        LookAtPlayerLastKnownPosition();

                    }           

                } else {

                    LookAtPlayerLastKnownPosition();
                    PatrolBehaviour();

                }

                //Counter will increase over time and reset to 0 if alerted again.
                //If enemy is not alerted for 6 seconds, revert to the idle state.
                alertedTimeCounter += Time.deltaTime;

                if (alertedTimeCounter > 6f) {
                    isAlerted = false;
                    isEngaging = false;
                    isInvestigating = false;
                    isTargetAcquired = false;

                }

            } else {

                //Patrol the designated area. This is the enemy's initial
                //and idle state.
                PatrolBehaviour();

            }

            //If player is close to the enemy and there is not some obstacle between them
            //enter the alerted state.
            if (distanceToPlayer < detectionDistance && IsPlayerInPotentialLOS()) {

                Alerted();

            }

            //If the enemy sees the player, alert nearby allies and engage the player.
            if (IsPlayerInLOS()) {

                Alerted();
                AlertNearbyAllies();
                isEngaging = true;

            } 

            //Pass animator the relevant parameters to correctly animate movement
            SetAnimatorParameters();

            //Adjust the value of the stress-related parameters and the stress level as appropriate
            SimulateStress();

        }

    }

    void EngagementBehavior() {

        //If enemy sees the player, track them and shoot as soon as possible
        //otherwise look at the last known position of the player
        if (IsPlayerInLOS()) {

            LookAtPlayer();

            if (isTargetAcquired) {

                equippedWeapon.Shoot(inaccuracy);

            } else {

                StartCoroutine("AcquireTarget");

            }

        } else {

            LookAtPlayerLastKnownPosition();

        }

        //If enemy is overly stressed or has low health, take cover
        //Otherwise if the player is in the enemy's line of sight chase and engage them
        //Otherwise move to the players last know location
        if (health < defensiveHealthLimit || stressLevel > 0.9f) {

            navMeshAgent.SetDestination(coverWaypoint.transform.position);

        } else if (IsPlayerInLOS()) {

            MoveToPlayer();

        } else {

            navMeshAgent.SetDestination(playerLastKnownPosition);

        }

    }

    void PatrolBehaviour() {

        //If enemy gets close to their current waypoint, start waiting and reset wait counter
        if (isMovingBetweenWaypoints && navMeshAgent.remainingDistance < 1f) {

            isWaitingAtWaypoint = true;

            isMovingBetweenWaypoints = false;

            waitTimeCounter = 0f;

        }

        //If the enemy is waiting at their waypoint or starting their very first patrol:
        if (isWaitingAtWaypoint || isFirstPatrol) {

            //Iterate the wait counter by the frame time
            waitTimeCounter += Time.deltaTime;

            //If the enemy has finished waiting or is starting their very first patrol:
            if (waitTimeCounter > waypointWaitTime || isFirstPatrol) {

                //Set the enemy's destination to the currently waiting patrol waypoint
                navMeshAgent.SetDestination(patrolWaypoints[waypointIndex].transform.position);
                
                //Prepare the next patrol waypoint by iterating index
                waypointIndex = (waypointIndex + 1) % patrolWaypoints.Count;

                //Indicate enemy is no longer waiting and is moving between patrol waypoints
                isFirstPatrol = false;

                isWaitingAtWaypoint = false;

                isMovingBetweenWaypoints = true;

            }

        }
      
    }

    //Enter alert state, reset alert counter, record last known position of player
    public void Alerted() {

        isAlerted = true;

        alertedTimeCounter = 0f;

        playerLastKnownPosition = player.transform.position;

    }

    //Enter alert state and then investigation state if not engaging
    public void BeginInvestigation() {

        Alerted();

        isInvestigating = true;

    }

    public void AlertNearbyAllies() {

        Collider[] colliders = Physics.OverlapSphere(transform.position, communicationDistance);

        foreach (Collider collidedWith in colliders) {

            Enemy nearbyEnemy = collidedWith.GetComponent<Enemy>();

            if (nearbyEnemy != null) {

                nearbyEnemy.BeginInvestigation();

            }

        }
    }

    void MoveToPlayer() {

        if (distanceToPlayer > desiredDistanceToPlayer) {

            navMeshAgent.SetDestination(player.transform.position);

        } else {

            navMeshAgent.ResetPath();

        }

    }

    void SimulateStress() {

        //Represents curve y = 4(x-x^2) which models loose approximation of Yerkes-Dodson Law.
        //Stress modifier represents to what extent certain attributes should be adjusted from their baseline levels
        //as a result of the agent's stress level.
        //The closer stress modifier is to 1, the more ideal the performance.
        float stressModifier = 4f * (stressLevel - (stressLevel * stressLevel));

        //A better performing agent will detect an enemy from further away
        detectionDistance = 5f + (5f * stressModifier);

        //A better performing agent will be less inaccurate when shooting
        inaccuracy = 3f - (2.75f * stressModifier);

        //A better performing agent will look turn more quickly when alerted and
        //be more effective at tracking an enemy during combat
        rotationSpeed = 300 + (200f * stressModifier);

        //A better performing agent will take less time to initially find and prepare to fight an enemy during combat
        reactionTime = 0.3f - (0.12f * stressModifier);

        if (isAlerted == true) {

            ChangeStress(0.005f * Time.deltaTime);

        } else {

            ChangeStress(-0.005f * Time.deltaTime);

        }

    }

    public void TakeDamage(float damage) {

        health -= damage;

        if (health <= 0) {
            Die();
        }

    }

    public void ChangeStress(float stressChange) {

        if (stressLevel + stressChange >= 1f) {

            stressLevel = 1f;

        } else if (stressLevel + stressChange <= initalStressLevel) {

            stressLevel = initalStressLevel;

        } else {

            stressLevel += stressChange;

        }
        
    }

    public void Die() {

        //Destroy(gameObject, 5f);
        animator.enabled = false;
        navMeshAgent.enabled = false;
        SetRagdollRigidBodies(false);
        SetRagdollColliders(true);

    }

    bool IsPlayerInLOS() {

        Vector3 enemyPos = transform.position;
        enemyPos.y += 1;

        Ray ray = new Ray(enemyPos, this.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {

            if (hit.collider.GetComponent<PlayerController>()) {

                return true;
                 
            } else {

                return false;

            }

        } else {

            return false;

        }

    }

    bool IsPlayerInPotentialLOS() {

        Vector3 playerPos = player.transform.position;
        playerPos.y += 1;
        Vector3 enemyPos = transform.position;
        enemyPos.y += 1;

        Ray ray = new Ray(enemyPos, playerPos - enemyPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {

            if (hit.collider.GetComponent<PlayerController>()) {

                return true;

            } else {

                return false;

            }

        } else {

            return false;

        }

    }

    void LookAtPlayer() {
        Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);
    }

    void LookAtPlayerLastKnownPosition() {
        Quaternion targetRotation = Quaternion.LookRotation(playerLastKnownPosition - transform.position);
        transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);
    }

    

    IEnumerator AcquireTarget() {

        yield return new WaitForSeconds(reactionTime);

        isTargetAcquired = true;

    }

    void SetRagdollRigidBodies(bool isRagdollActive) {

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rigidbody in rigidbodies) {
            rigidbody.isKinematic = isRagdollActive;
        }

    }

    void SetRagdollColliders(bool isColliderActive) {

        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders) {
            collider.enabled = isColliderActive;
        }

        GetComponent<Collider>().enabled = !isColliderActive;

    }

    void SetAnimatorParameters() {

        if (Mathf.Sqrt(navMeshAgent.velocity.x * navMeshAgent.velocity.x + navMeshAgent.velocity.z * navMeshAgent.velocity.z) > 0) {

            animator.SetBool("Moving", true);

        } else {

            animator.SetBool("Moving", false);

        }

        Vector3 movementVector = navMeshAgent.velocity.normalized;
        float facingAngle = transform.eulerAngles.y * Mathf.Deg2Rad;
        Vector3 facingVector = new Vector3(Mathf.Sin(facingAngle), 0, Mathf.Cos(facingAngle));

        animator.SetFloat("Dot Product", Vector3.Dot(facingVector, movementVector));
        animator.SetFloat("Cross Y", Vector3.Cross(facingVector, movementVector).y);

    }

}
