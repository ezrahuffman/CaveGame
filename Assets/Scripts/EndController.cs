using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndController : MonoBehaviour
{
    private GameController gameCont;

    // Start is called before the first frame update
    void Start()
    {
        gameCont = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided with " + collision.name);
        Debug.Log("Tag = " + collision.tag);
        //Level complete when the player reaches the end
        if(collision.tag == "Player")
        {
            gameCont.LevelComplete();
        }

    }
}
