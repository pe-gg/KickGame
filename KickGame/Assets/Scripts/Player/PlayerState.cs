using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used as a 'hub' for scripts to reference what state the player is in.
/// </summary>
public class PlayerState : MonoBehaviour
{
    public enum PState
    {
        DEFAULT,
        JUMPING,
        KICKING,
        DIVEKICKING
    }
    public PState currentState;
    private void Awake()
    {
        currentState = PState.DEFAULT;
    }
}
