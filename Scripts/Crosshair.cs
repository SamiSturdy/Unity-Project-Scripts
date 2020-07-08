using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Make the mouse cursor invisible when the game begins
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Set the crosshair's position on the screen to the position of the mouse cursor
        transform.position = Input.mousePosition;
    }
}
