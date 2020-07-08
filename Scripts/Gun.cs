using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (AudioSource))]
public class Gun : MonoBehaviour {

    public int gunID;
    public float roundsPerMinute;
    public float damage;
    public float shotForce;
    public float noiseTravel;
    public ParticleSystem bloodSplatter;

    public Transform barrelTip;
    public Transform chamber;
    public Rigidbody shell;

    private LineRenderer tracer;
    private Camera cam;

    private float shotDelay;
    private float nextPossibleShootTime;

    private void Start() {
        shotDelay = 60 / roundsPerMinute;

        tracer = GetComponent<LineRenderer>();

        cam = Camera.main;
    }

    public void Shoot() {

        if (CanShoot()) {

            Vector3 mousePos = Input.mousePosition;
            mousePos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.transform.position.y - barrelTip.position.y));

            Vector3 shotDirection = (mousePos - barrelTip.position);

            Ray ray = new Ray(barrelTip.position, shotDirection);
            RaycastHit hit;

            float shotDistance = 20;

            if (Physics.Raycast(ray, out hit, shotDistance)) {

                shotDistance = hit.distance;

                Enemy enemy = hit.collider.GetComponent<Enemy>();

                if (enemy) {
                    
                    ParticleSystem blood = Instantiate(bloodSplatter, hit.point, Quaternion.FromToRotation(Vector3.forward, -shotDirection));

                    Destroy(blood, 0.3f);

                    enemy.TakeDamage(damage);

                    enemy.ChangeStress(0.1f);

                    enemy.BeginInvestigation();

                    Collider[] hitColliders = Physics.OverlapSphere(hit.transform.position, 1f);

                    foreach (Collider collidedWith in hitColliders) {

                        Rigidbody rigidbody = collidedWith.GetComponent<Rigidbody>();

                        if (rigidbody != null) {

                            rigidbody.AddForce(shotDirection * shotForce);

                        }

                    }

                }

                Collider[] noiseColliders = Physics.OverlapSphere(transform.position, noiseTravel);

                foreach (Collider collidedWith in noiseColliders) {

                    Enemy alertedEnemy = collidedWith.GetComponent<Enemy>();

                    if (alertedEnemy != null) {

                        alertedEnemy.Alerted();

                    }

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

        //Render the tracer within the world
        tracer.enabled = true;

        //Set the origin of the tracer to the tip of the barrel
        tracer.SetPosition(0, barrelTip.position);

        //Set the end of the tracer to the point at which the last shot hit
        tracer.SetPosition(1, barrelTip.position + hitPoint);

        //Effectively waits for one frame
        yield return null;

        //Stop rendering the tracer
        tracer.enabled = false;
    }

}
