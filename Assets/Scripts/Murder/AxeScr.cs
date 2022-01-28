using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class AxeScr : MonoBehaviourPun
{
    public Transform axePos;
    public BoxCollider axeBox;
    MurderScr murderScr;

    public bool isAttack;
    [SerializeField] float gapDis = 0;
    void Start()
    {
        axeBox = GetComponent<BoxCollider>();
        axeBox.enabled = false;
        murderScr = transform.root.GetComponent<MurderScr>();
    }
    
    void Update()
    {
        //transform.position = axePos.position + transform.forward * gapDis;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
        {
            if (other.CompareTag("SURVIVOR") && other.GetComponentInChildren<SurvivorN>().sLife > 1)
            {
                axeBox.enabled = false;
                murderScr.AttackSuccessCheck();
                other.GetComponentInChildren<SurvivorN>().CallMaster("DamageCall");
            }
        }
    }
}
