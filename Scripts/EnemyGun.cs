using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (AudioSource))]
public class EnemyGun : MonoBehaviour {

    public int gunID;
    public float roundsPerMinute;
    public float damage;
    public float shotForce;
    public ParticleSystem bloodSplatter;

    public Transform barrelTip;
    public Transform chamber;
    public Rigidbody shell;

    private LineRenderer tracer;
    private GameObject player;

    private float shotDelay;
    private float nextPossibleShootTime;

    private void Start() {
        shotDelay = 60 / roundsPerMinute;
        if (GetComponent<LineRenderer>()) {
            tracer = GetComponent<LineRenderer>();
        }

        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void Shoot(float inaccuracy) {

        if (CanShoot()) {

            Vector3 playerPos = player.transform.position;
            playerPos.y += 1;
            Vector3 shotDirection = (playerPos - barrelTip.position);

            shotDirection.x += Random.Range(-inaccuracy, inaccuracy);
            shotDirection.z += Random.Range(-inaccuracy, inaccuracy);

            Ray ray = new Ray(barrelTip.position, shotDirection);
            RaycastHit hit;

            float shotDistance = 20;

            if (Physics.Raycast(ray, out hit, shotDistance)) {

                shotDistance = hit.distance;

                if (hit.collider.GetComponent<PlayerController>()) {
                    
                    ParticleSystem blood = Instantiate(bloodSplatter, hit.point, Quaternion.FromToRotation(Vector3.forward, -shotDirection));

                    Destroy(blood, 0.3f);

                    hit.collider.GetComponent<PlayerController>().TakeDamage(damage);

                }

            }

            nextPossibleShootTime = Time.time + shotDelay;

            GetComponent<AudioSource>().Play();

            if (tracer) {
                StartCoroutine("RenderTracer", ray.direction * shotDistance);
            }

            Rigidbody newShell = Instantiate(shell, chamber.position, Quaternion.Euler(Random.Range(70f,90f),Random.Range(-10f, 10f), Random.Range(-10f, 10f))) as Rigidbody;
            newShell.AddForce((chamber.forward * Random.Range(150f, 200f)) + (barrelTip.forward * Random.Range(-10f, 10f)));

        }
    }

    private bool CanShoot() {

        bool canShoot = true;

        if (Time.time < nextPossibleShootTime) {
            canShoot = false;
        }

        return canShoot;
    }

    IEnumerator RenderTracer(Vector3 hitPoint) {
        tracer.enabled = true;
        tracer.SetPosition(0, barrelTip.position);
        tracer.SetPosition(1, barrelTip.position + hitPoint);
        yield return null;
        tracer.enabled = false;
    }

}
