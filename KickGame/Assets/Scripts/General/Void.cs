using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Void : MonoBehaviour
{
    [SerializeField] private GameOverMenu gameOverMenu;

    


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

