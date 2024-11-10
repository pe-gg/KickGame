using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Void : MonoBehaviour
{
    private GameOverMenu gameOverMenu;

    void Start()
    {

        gameOverMenu = FindObjectOfType<GameOverMenu>();
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            gameOverMenu.Setup();
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}

