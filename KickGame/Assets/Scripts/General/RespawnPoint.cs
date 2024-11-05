using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    private PlayerController _pc;
    private GameManager _gm;
    private void Awake()
    {
        _gm = FindObjectOfType<GameManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_gm.CheckpointTriggered == true || !other.CompareTag("Player"))
            return;
        _gm.UpdateSpawnPoint(this.transform.position);
        _gm.CheckpointTriggered = true;
    }
}
