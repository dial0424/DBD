using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class BoardManager : MonoBehaviourPun
{
    [SerializeField] List<GameObject> board = new List<GameObject>();

    private void Start()
    {
        if (board.Count > 0)
        {
            foreach (var _board in board)
            {
                _board.GetComponentInChildren<BoardRange>().pushBoard = OnPushBoard;
                _board.GetComponentInChildren<Board>().breakBoard = OnBreakBoard;
            }
        }
    }

    public void OnPushBoard(GameObject _gameObject)
    {
        
        for(int i=0;i<board.Count;i++)
        {
            if(board[i].name == _gameObject.name)
            {
                RPCManager.BoardRangeStartRPC(_gameObject, "PushBoardRPC");
            }
        }
    }

    public void OnBreakBoard(GameObject _gameObject)
    {
        for(int i=0;i<board.Count;i++)
        {
            if(board[i].name == _gameObject.name)
            {
                //board.RemoveAt(i);
                photonView.RPC("test", RpcTarget.All, i);

                RPCManager.BoardStartRPC(_gameObject, "BreakBoardRPC");
            }
        }
    }

    [PunRPC]
    public void test(int i)
    {
        board.RemoveAt(i);
    }

}
