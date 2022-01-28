using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class HangerScr_ : MonoBehaviourPun
{
    public delegate void SetNullSurvivorDelegate(string _tag);
    public SetNullSurvivorDelegate SetSNullCallback = null;
    public delegate void SetMaterial(string _tag);
    public SetMaterial SetMaterialHanger = null;

    public GameObject survivor = null;
    public Transform hangPos = null;
    public Material normalMaterial = null;

    private void Awake()
    {
        hangPos = transform.GetChild(7);
    }

    [PunRPC]
    public void SetNullDelegate(string _tag)
    {
        SetSNullCallback?.Invoke(_tag);
    }

    public void StartRPC(string _rpcName)
    {
        photonView.RPC(_rpcName, RpcTarget.All);
    }

    public void StartRPCMaster(string _rpcName, string _tag)
    {
        photonView.RPC(_rpcName, RpcTarget.MasterClient, _tag);
    }

    [PunRPC]
    private void SetNullSurvivor()
    {
        survivor = null;
        GetComponent<MeshRenderer>().material = normalMaterial;
    }

}
