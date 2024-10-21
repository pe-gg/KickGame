using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void SwitchState(int state)
    {
        currentState = (PState)state;
    }

    public bool CompareState(int state)
    {
        if (currentState == (PState)state)
            return true;
        else
            return false;
    }
}
