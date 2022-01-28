using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LeverManager : MonoBehaviourPun
{
    [SerializeField] private Lever[] levers = new Lever[4];
    [SerializeField] private Lever realLever = null;

    [SerializeField] private GeneratorManager genManager = null;

    void Start()
    {
        genManager.leverCallback = CanUseLeverBack;

        if (PhotonNetwork.IsMasterClient)
        {
            int randNum = Random.Range(0, levers.Length);
            photonView.RPC("SetRealLever", RpcTarget.All, randNum);
            //realLever.
        }
    }

    [PunRPC]
    public void SetRealLever(int _num)
    {
        realLever = levers[_num];
    }

    public void CanUseLeverBack()
    {
        photonView.RPC("CanUseLeverAll", RpcTarget.All);
    }

    [PunRPC]
    public void CanUseLeverAll()
    {
        realLever.canUse = true;
    }
}
