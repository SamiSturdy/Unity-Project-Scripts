using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeShell : MonoBehaviour
{

    IEnumerator OnTriggerEnter(Collider collider) {

        if (collider.tag == "Ground") {

            yield return new WaitForSeconds(5);

            GetComponent<Rigidbody>().Sleep();

        }

    }

}
