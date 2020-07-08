using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayHealth : MonoBehaviour
{

    public PlayerController player;
    public Text healthText;

    void Update()
    {
        //Each frame, set the text representing the player's health to the player's current health value
        healthText.text = player.health.ToString();
    }

}
