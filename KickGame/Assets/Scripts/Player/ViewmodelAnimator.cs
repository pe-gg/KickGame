using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewmodelAnimator : MonoBehaviour
{
    private Animator _legAnim;
    private Animator _armAnim;
    private void Awake()
    {
        _legAnim = GetComponentInChildren<LegAnim>().GetComponent<Animator>();
    }

    public void GroundKick()
    {
        _legAnim.SetBool("GroundKick", true);
        Invoke("GroundKickEnd", 0.5f);
    }

    private void GroundKickEnd()
    {
        _legAnim.SetBool("GroundKick", false);
    }

    public void DiveKickStart()
    {
        _legAnim.SetBool("DiveKick", true);
    }
    public void DiveKickEnd()
    {
        _legAnim.SetBool("DiveKick", false);
    }
}
