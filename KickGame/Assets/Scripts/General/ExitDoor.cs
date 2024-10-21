using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{

    [SerializeField] private string sceneToload = "SceneName"; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            SceneManager.LoadScene(sceneToload);
        }
    }
}
