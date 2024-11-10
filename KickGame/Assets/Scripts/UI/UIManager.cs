using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region arm variables
    [SerializeField] public GameObject armOnUI;
    [SerializeField] public GameObject armOffUI;
    [SerializeField] public PlayerArm arm;

    [SerializeField] public PlayerAim playerAim;
    #endregion

    #region menu variables
    [SerializeField] public GameObject pauseMenu;
    [SerializeField] public GameObject settingsMenu;
    public bool isPaused;
    #endregion

    #region volume variables
    [SerializeField] public Slider bgmVolume;
    [SerializeField] public Slider sfxVolume;

    [SerializeField] float currentBGMVolume;
    [SerializeField] float currentSFXVolume;

    [SerializeField] public AudioSource bgmSource;
    [SerializeField] public AudioSource sfxSource;
    #endregion 
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
    public void SetVolumeBGM(float volumeFl)
    {
        bgmSource.volume = bgmVolume.value;
        currentBGMVolume = volumeFl;
    }
    public void SetVolumeSFX(float volumeFl)
    {
        sfxSource.volume = sfxVolume.value;
        currentSFXVolume = volumeFl;
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
