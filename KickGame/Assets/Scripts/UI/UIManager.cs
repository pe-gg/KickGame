using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] public GameObject armOnUI;
    [SerializeField] public GameObject armOffUI;
    [SerializeField] public PlayerArm arm;

    [SerializeField] public GameObject pauseMenu;
    [SerializeField] public GameObject settingsMenu;
    public bool isPaused;
    [SerializeField] public PlayerAim playerAim;

    [SerializeField] public Slider volumeSlider;
    [SerializeField] float currentVolume;
    [SerializeField] public AudioSource audioMixer;

    private void Awake()
    {
        armOnUI.SetActive(true);
        armOffUI.SetActive(false);
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        ArmUI();
    }
    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
        Time.timeScale = 0f;
        playerAim.enabled = false;
        isPaused = true;
    }
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        playerAim.enabled = true;
        Time.timeScale = 1f;
        isPaused = false;
    }
    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void SettingsMenu()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
        Time.timeScale = 0f;
        playerAim.enabled = false;
        isPaused = true;
    }
    public void SetVolume(float volume)
    {
        audioMixer.volume = volumeSlider.value;
        currentVolume = volume;
    }
    public void ArmUI()
    {
        if (arm.HasArm == true)
        {
            armOnUI.SetActive(true);
            armOffUI.SetActive(false);
        }
        else
        {
            armOnUI.SetActive(false);
            armOffUI.SetActive(true);
        }
    }
}
