using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    private GameObject _player;
    private GameManager _gm;
    private void Awake()
    {
        _gm = FindAnyObjectByType<GameManager>();
        _player = GameObject.Find("Player");
        Invoke("SetSpawn", 0.1f);
    }

    private void SetSpawn()
    {
        if (_gm.InitialSpawnSet)
            return;
        _gm.InitialSpawnSet = true;
        this.gameObject.transform.position = _player.transform.position;
        _gm.SpawnPoint = this.transform.position;
    }
}
