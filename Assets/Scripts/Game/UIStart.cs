using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class UIStart : MonoBehaviourPun
{
    [SerializeField] private GameObject loadImage = null;
    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("GameStart", RpcTarget.All);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    public void GameStart()
    {
        loadImage.SetActive(false);
    }
}
