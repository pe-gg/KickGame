using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton
{
    public Vector3 SpawnPoint;
    public bool CheckpointTriggered = false;
    public bool InitialSpawnSet = false;
    private string _levelName;
    private int check; //debug purposes
    private GameObject _player;

    private void Awake()
    {
        _player = GameObject.Find("Player");
        _levelName = SceneManager.GetActiveScene().name;
    }

    private void OnLevelWasLoaded(int level)
    {
        if(_levelName == SceneManager.GetActiveScene().name)
        {
            //same scene
            _player = GameObject.Find("Player");
            Invoke("TeleportPlayer", 0.01f);
            check++;
            Debug.Log("Same scene loaded! " + check);
        }
        else
        {
            //different scene
            InitialSpawnSet = false;
            CheckpointTriggered = false;
            _player = GameObject.Find("Player");
            check = 0;
            _levelName = SceneManager.GetActiveScene().name;
        }
    }

    public void UpdateSpawnPoint(Vector3 spawnPoint)
    {
        if (CheckpointTriggered)
            return;
        SpawnPoint = spawnPoint;
    }

    private void TeleportPlayer()
    {
        _player.transform.position = SpawnPoint;
    }
}
