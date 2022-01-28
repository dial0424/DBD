using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ReadyCheck : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button readyButton;

    public bool playerReady = false;
    private ExitGames.Client.Photon.Hashtable _playerCustomProperties = new ExitGames.Client.Photon.Hashtable();

    public delegate void StartGame();
    public StartGame startGame = null;

    [SerializeField] private List<Image> checkImages = new List<Image>();
    public int clientNum = 0;

    private int allReady = 0;

    private void Awake()
    {
        clientNum = PhotonNetwork.CurrentRoom.PlayerCount - 1;
    }

    private void Start()
    {


        if(PhotonNetwork.IsMasterClient)
        {
            readyButton.GetComponentInChildren<Text>().text = "Start";
        }
        else
        {
            readyButton.GetComponentInChildren<Text>().text = "Ready";
            _playerCustomProperties["PlayerReady"] = playerReady;
            PhotonNetwork.SetPlayerCustomProperties(_playerCustomProperties);
        }

    }

    private void Ready()
    {
        if(playerReady)
        {
            playerReady = false;
            _playerCustomProperties["PlayerReady"] = playerReady;
            PhotonNetwork.SetPlayerCustomProperties(_playerCustomProperties);
            readyButton.image.color = Color.white;

            photonView.RPC("ChangeCheck", RpcTarget.All, playerReady, clientNum);

            photonView.RPC("AllReady", RpcTarget.All, playerReady);
        }
        else
        {
            playerReady = true;
            _playerCustomProperties["PlayerReady"] = playerReady;
            PhotonNetwork.SetPlayerCustomProperties(_playerCustomProperties);
            readyButton.image.color = Color.red;

            photonView.RPC("ChangeCheck", RpcTarget.AllBuffered, playerReady, clientNum);

            photonView.RPC("AllReady", RpcTarget.AllBuffered, playerReady);
        }
    }

    [PunRPC]
    public void ChangeCheck(bool _playerReady, int playerN)
    {
        checkImages[playerN].gameObject.SetActive(_playerReady);
    }

    [PunRPC]
    public void AllReady(bool _playerReady)
    {
        if (_playerReady)
        {
            allReady++;
        }
        else
        {
            allReady--;
        }

        if (allReady == 2) checkImages[0].gameObject.SetActive(true);
        else checkImages[0].gameObject.SetActive(false);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SetNewPlayer", newPlayer, allReady);
        }
    }

    [PunRPC]
    public void SetNewPlayer(int _allReday)
    {
        allReady = _allReday;
    }

    private bool GameStart()
    {
        if (PhotonNetwork.PlayerListOthers.Length > 1)
        {
            foreach (var player in PhotonNetwork.PlayerListOthers)
            {
                if ((bool)player.CustomProperties["PlayerReady"] == false)
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    public void OnButtonClick()
    {
        AudioManager.audioInstance._audioSource.Play();

        if (PhotonNetwork.IsMasterClient)
        {
            if(GameStart())
            {
                startGame();
            }
        }

        else
        {
            Ready();
        }
    }

    public void LeavePlayer(int _client)
    {
        photonView.RPC("AllReady", RpcTarget.All, false);
        photonView.RPC("LeavePlayerCheck", RpcTarget.AllBuffered, _client);
    }

    [PunRPC]
    public void LeavePlayerCheck(int _client)
    {
        checkImages[_client].gameObject.SetActive(false);
        Image temp = checkImages[_client];
        checkImages.RemoveAt(_client);
        checkImages.Add(temp);
    }
}
