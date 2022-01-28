using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class BoardRange : MonoBehaviourPun
{
    public delegate void PushBoard(GameObject gameObject);
    public PushBoard pushBoard = null;

    public Board _board = null;
    [SerializeField] private GameObject jumpPos = null;

    // Start is called before the first frame update
    void Start()
    {
        _board = this.transform.parent.GetComponentInChildren<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_board.isDown == true)
        {

            jumpPos.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }

    public void StartBoardRangeRPC(string _name)
    {
        photonView.RPC(_name, RpcTarget.All);
    }

    [PunRPC]
    public void PushBoardRPC()
    {
        _board.isPush = true;
    }

    private void OnTriggerEnter(Collider other)
    {        
        if (other.gameObject.tag == "SURVIVOR"&&_board.isDown==false)
        {
            other.transform.GetComponentInChildren<SurvivorN>().sBoard = _board;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "SURVIVOR" && _board.isDown == false)
        {
            other.transform.GetComponentInChildren<SurvivorN>().sBoard = null;
        }
    }
}
