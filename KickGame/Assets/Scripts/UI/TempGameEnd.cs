using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempGameEnd : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Restart()
    {
        SceneManager.LoadScene("PrototypeScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
