using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private DataStorage dataStorage = null;

    [SerializeField] private GameObject murderPrefab = null;

    [SerializeField] private GameObject[] survivorPrefab = new GameObject[2];

    [SerializeField] private ReadyCheck readyCheck = null;

    [SerializeField] private Transform murderPos = null;

    [SerializeField] List<GameObject> Players = new List<GameObject>();
    [SerializeField] private List<Vector3> survivorPos = new List<Vector3>();

    [SerializeField] private GameObject changeSurvBtn = null;
    [SerializeField] private GameObject changeItemBtn = null;
    [SerializeField] private GameObject changeItemBtnMurder = null;

    [SerializeField] private int clientNumber = 0;

    [SerializeField] private List<string> nickNames = new List<string>();

    [SerializeField] private List<GameObject> nickNamesObj = new List<GameObject>();

    [SerializeField] private GameObject CharacterChoicePanel = null;
    [SerializeField] private GameObject ItemChoicePanel = null;
    [SerializeField] private GameObject ItemChoicePanelMurder = null;
    private bool isClick = false;

    [SerializeField] private GameObject LoadingCanvasObj = null;

    private ExitGames.Client.Photon.Hashtable _playerCustomProperties = new ExitGames.Client.Photon.Hashtable();

    [SerializeField] private string[] itemnames = new string[3];

    [SerializeField] private Text healCountT = null;
    [SerializeField] private Text flashCountT = null;
    [SerializeField] private Text axeCountT = null;
    private float healC = 0f;
    private float flashC = 0f;
    private float axeC = 0f;

    private LogoutManager lm = null;

    [SerializeField] private Image choiceItemImage = null;
    [SerializeField] private Image choiceItemImageMurder = null;
    [SerializeField] private Button hb = null;
    [SerializeField] private Button fb = null;

    public bool useAxe = false;

    private void Awake()
    {
        dataStorage = DataStorage.Instance;
        lm = LogoutManager.Instance;

        if (dataStorage.userPosition == "m")
        {
            changeSurvBtn.SetActive(false);
            changeItemBtn.SetActive(false);
        }
        else
        {
            changeItemBtnMurder.SetActive(false);
        }

        PhotonNetwork.AutomaticallySyncScene = true;

        clientNumber = PhotonNetwork.CurrentRoom.PlayerCount - 1;
        _playerCustomProperties["Number"] = clientNumber;
        PhotonNetwork.SetPlayerCustomProperties(_playerCustomProperties);

        readyCheck.startGame = GameStart;

        CharacterChoicePanel.SetActive(false);
    }

    void Start()
    {
        healC = lm.healItem;
        flashC = lm.lightItem;
        axeC = lm.axeItem;

        if (healC == 0f) hb.enabled = false;
        if (flashC == 0f) fb.enabled = false;

        if (PhotonNetwork.IsMasterClient)
        {
            if (murderPrefab != null)
            {
                GameObject obj = PhotonNetwork.Instantiate(murderPrefab.name,
                murderPos.position,
                Quaternion.Euler(new Vector3(0, 180, 0)),
                0
                );

                Players.Add(obj);
                nickNames.Add(PhotonNetwork.NickName);

                photonView.RPC("SetNickName", RpcTarget.All, PhotonNetwork.CurrentRoom.PlayerCount - 1, PhotonNetwork.NickName);
            }
        }
        else
        {
            photonView.RPC("spawnSurv", RpcTarget.MasterClient, PhotonNetwork.CurrentRoom.PlayerCount - 1, PhotonNetwork.NickName);

        }
    }

    [PunRPC]
    public void spawnSurv(int _count, string _nickName)
    {
        photonView.RPC("SetNickName", RpcTarget.All, _count, _nickName);

        if (survivorPrefab != null)
        {
            GameObject obj = PhotonNetwork.Instantiate(survivorPrefab[0].name,
            survivorPos[0],
            Quaternion.Euler(new Vector3(0, 180, 0)),
            0
            );

            survivorPos.RemoveAt(0);
            Players.Add(obj);
            nickNames.Add(_nickName);
        }
    }

    [PunRPC]
    public void SetNickName(int _count, string _realNickName)
    {

        nickNamesObj[_count].GetComponent<Text>().text = _realNickName;

        for (int i = 0; i < nickNames.Count; i++)
        {
            photonView.RPC("ReSetNickName", RpcTarget.Others, i, nickNames[i]);
        }

    }

    [PunRPC]
    public void ReSetNickName(int _count, string _name)
    {
        nickNamesObj[_count].GetComponent<Text>().text = _name;
    }

    private void GameStart()
    {
        photonView.RPC("UseItem", RpcTarget.All);

        if (useAxe == true)
        {
            PlayerPrefs.SetFloat("Axe", 1f);
        }
        else
        {
            PlayerPrefs.SetFloat("Axe", 0f);
        }

        for (int i = 1; i < Players.Count; i++)
        {
            PlayerPrefs.SetString(i.ToString(), nickNames[i]);
            PlayerPrefs.SetString("Survivor" + nickNames[i], Players[i].name.Substring(0, 9));

            PlayerPrefs.SetString("item" + i.ToString(), itemnames[i]);
        }
        PhotonNetwork.LoadLevel("LoadingScene");
    }

    public void surv1Btn()
    {
        if(!readyCheck.playerReady)
            photonView.RPC("ChangeSurv1", RpcTarget.MasterClient, clientNumber);
    }

    [PunRPC]
    public void ChangeSurv1(int _clientNum)
    {
        if (Players[_clientNum].gameObject.name != "Survivor1Model(Clone)")
        {
            photonView.RPC("ChangeModel", RpcTarget.MasterClient, _clientNum, 0);
        }
    }

    public void surv2Btn()
    {
        if (!readyCheck.playerReady)
            photonView.RPC("ChangeSurv2", RpcTarget.MasterClient, clientNumber);
    }

    [PunRPC]
    public void ChangeSurv2(int _clientNum)
    {
        if (Players[_clientNum].gameObject.name != "Survivor2Model(Clone)")
        {
            photonView.RPC("ChangeModel", RpcTarget.MasterClient, _clientNum, 1);
        }
    }

    [PunRPC]
    public void ChangeModel(int _num, int newNum)
    {
        Transform tr = Players[_num].transform;

        PhotonNetwork.Destroy(Players[_num].gameObject);

        Players.RemoveAt(_num);

        GameObject obj = PhotonNetwork.Instantiate(survivorPrefab[newNum].name, tr.position, Quaternion.Euler(new Vector3(0, 180, 0)), 0);
        Players.Insert(_num, obj);
    }

    //내 클라이언트가 나갔을때 마스터 클라이언트의 리스트에서 내 정보를 삭제
    [PunRPC]
    public void SetArrayOnPlayerLeftRoom(int otherNum)
    {
        survivorPos.Add(Players[otherNum].transform.position);
        GameObject removePlayer = Players[otherNum];
        Players.RemoveAt(otherNum);
        PhotonNetwork.Destroy(removePlayer);
        nickNames.RemoveAt(otherNum);
        itemnames[otherNum] = "null";

        photonView.RPC("SetNickList", RpcTarget.AllBuffered, otherNum);

        //플레이어 나갔을 때 체크 해제
        readyCheck.GetComponent<ReadyCheck>().LeavePlayer(otherNum);
    }

    [PunRPC]
    public void SetNickList(int otherNum)
    {
        nickNamesObj[otherNum].GetComponent<Text>().text = "";
        GameObject temp = nickNamesObj[otherNum];
        nickNamesObj.RemoveAt(otherNum);
        nickNamesObj.Add(temp);
    }

    //내 클라이언트가 나갈때 내 순번이 다른 클라이언트 순번보다 작으면
    //다른 클라이언트 순번을 하나 줄여줌
    [PunRPC]
    public void SetNum(int otherNum)
    {
        if (clientNumber > otherNum)
        {
            clientNumber--;
            readyCheck.GetComponent<ReadyCheck>().clientNum = clientNumber;
            _playerCustomProperties["Number"] = clientNumber;
            PhotonNetwork.SetPlayerCustomProperties(_playerCustomProperties);
        }

    }
    public void LeaveRoom()
    {
        AudioManager.audioInstance._audioSource.Play();

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("MasterLeave", RpcTarget.All);
        }
        //photonView.RPC("SetArrayOnPlayerLeftRoom", RpcTarget.MasterClient, clientNumber);
        //photonView.RPC("SetNum", RpcTarget.Others, clientNumber);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if ((int)otherPlayer.CustomProperties["Number"] == 0)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetArrayOnPlayerLeftRoom((int)otherPlayer.CustomProperties["Number"]);
            }
            SetNum((int)otherPlayer.CustomProperties["Number"]);
        }

    }

    [PunRPC]
    public void MasterLeave()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("SelectScene");
    }

    public void CharacterChoiceBtn()
    {
        AudioManager.audioInstance._audioSource.Play();

        if (ItemChoicePanel.activeSelf)
        {
            isClick = !isClick;
            ItemChoicePanel.SetActive(isClick);
        }

        isClick = !isClick;
        CharacterChoicePanel.SetActive(isClick);
    }

    public void CharChoiceOnMouse()
    {
        CharacterChoicePanel.SetActive(true);
    }

    public void charChoiceExitMouse()
    {
        if (isClick == false) CharacterChoicePanel.SetActive(false);
    }

    public void ItemChoiceBtn()
    {
        AudioManager.audioInstance._audioSource.Play();

        if(CharacterChoicePanel.activeSelf)
        {
            isClick = !isClick;
            CharacterChoicePanel.SetActive(isClick);
        }

        isClick = !isClick;
        ItemChoicePanel.SetActive(isClick);

        healCountT.text = healC.ToString();
        flashCountT.text = flashC.ToString();
    }

    public void ItemChoiceBtnMurder()
    {
        AudioManager.audioInstance._audioSource.Play();

        isClick = !isClick;
        ItemChoicePanelMurder.SetActive(isClick);

        axeCountT.text = axeC.ToString();
    }

    public void FlashLightBtn()
    {
        if (!readyCheck.playerReady&&itemnames[clientNumber]!="Flashlight")
        {
            choiceItemImage.sprite= Resources.Load("UI/UITexture/Flashlight", typeof(Sprite)) as Sprite;

            photonView.RPC("ChangeItemMaster", RpcTarget.MasterClient, "Flashlight", clientNumber);

            if (itemnames[clientNumber] =="FirstAidKit")
            {
                healC++;
            }

            flashC--;

            flashCountT.text = flashC.ToString();
            healCountT.text = healC.ToString();
        }
    }

    public void FirstAidKitBtn()
    {
        if (!readyCheck.playerReady && itemnames[clientNumber] != "FirstAidKit")
        {
            choiceItemImage.sprite = Resources.Load("UI/UITexture/FirstAidKit", typeof(Sprite)) as Sprite;

            photonView.RPC("ChangeItemMaster", RpcTarget.MasterClient, "FirstAidKit", clientNumber);

            if (itemnames[clientNumber] =="Flashlight")
            {
                flashC++;
            }

            healC--;

            flashCountT.text = flashC.ToString();
            healCountT.text = healC.ToString();
        }
    }

    public void AxeBtn()
    {
        if (!readyCheck.playerReady && useAxe == false)
        {
            useAxe = true;

            choiceItemImageMurder.sprite = Resources.Load("UI/UITexture/Axe", typeof(Sprite)) as Sprite;

            axeC--;

            axeCountT.text = axeC.ToString();
        }
    }

    public void nullBtn()
    {
        if (!readyCheck.playerReady && dataStorage.userPosition == "s")
        {
            choiceItemImage.sprite = Resources.Load("UI/UITexture/X", typeof(Sprite)) as Sprite;

            photonView.RPC("ChangeItemMaster", RpcTarget.MasterClient, null, clientNumber);

            if (itemnames[clientNumber] == "Flashlight")
            {
                flashC++;
            }
            else if (itemnames[clientNumber] == "FirstAidKit")
            {
                healC++;
            }

            flashCountT.text = flashC.ToString();
            healCountT.text = healC.ToString();
        }
        else if(!readyCheck.playerReady && dataStorage.userPosition == "m" && useAxe == true)
        {
            useAxe = false;
            choiceItemImageMurder.sprite = Resources.Load("UI/UITexture/X", typeof(Sprite)) as Sprite;
            axeC++;
            axeCountT.text = axeC.ToString();
        }
    }

    [PunRPC]
    public void ChangeItemMaster(string _item, int _num)
    {
        photonView.RPC("ChangeItemAry", RpcTarget.All, _item, _num);
    }

    [PunRPC]
    public void ChangeItemAry(string _item,int _num)
    {
        itemnames[_num] = _item;
    }

    [PunRPC]
    public void UseItem()
    {
        lm.axeItem = axeC;
        lm.lightItem = flashC;
        lm.healItem = healC;

        lm.UseItemFuc();
    }
}
