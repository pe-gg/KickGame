using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    private string currentSceneName;
    [SerializeField] public PlayerAim playerAim;
    public void Setup()
    {
        Time.timeScale = 0f;
        playerAim.enabled = false;
        this.gameObject.SetActive(true);
    }
    public void Restart()
    {
        SceneManager.LoadScene(currentSceneName);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
