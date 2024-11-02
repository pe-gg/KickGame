using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] public GameObject armOnUI;
    [SerializeField] public GameObject armOffUI;

    [SerializeField] public PlayerArm arm;

    private void Awake()
    {
        armOnUI.SetActive(true);
        armOffUI.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        ArmUI();
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
