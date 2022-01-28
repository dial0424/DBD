using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class HangerManager : MonoBehaviourPun
{
    [SerializeField] public List<GameObject> hangers = new List<GameObject>();
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material shaderMaterial;
    
    void Start()
    {
        hangers.AddRange(GameObject.FindGameObjectsWithTag("HANGER"));
        int num = 1;
        foreach (var hanger in hangers)
        {
            hanger.transform.GetChild(7).tag = "HANGHOOKPOS" + num++.ToString();
            hanger.GetComponent<HangerScr_>().SetSNullCallback = SetSurvivorNullInHanger;
            hanger.GetComponent<HangerScr_>().SetMaterialHanger = SetMaterialHangerFun;
        }

        if(PhotonNetwork.IsMasterClient)
        {
            foreach(var hanger in hangers)
            {
                hanger.GetComponent<MeshRenderer>().material = shaderMaterial;
            }
        }
    }

    public void SetSurvivorNullInHanger(string _tag)
    {
        foreach(var a in hangers)
        {
            if(a.transform.GetChild(7).tag == _tag)
            {
                RPCManager.HangerStartRPC(a.gameObject, "SetNullSurvivor");
                return;
            }
        }
    }

    public void SetMaterialHangerFun(string _tag)
    {
        foreach (var a in hangers)
        {
            if (a.transform.GetChild(7).tag == _tag)
            {
                if (!PhotonNetwork.IsMasterClient)
                {
                    a.GetComponent<MeshRenderer>().material = shaderMaterial;
                    a.GetComponent<HangerScr_>().normalMaterial = normalMaterial;
                }
                return;
            }
        }
    }
}
