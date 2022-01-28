using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField] private string gameVersion = "0.0.1";
    [SerializeField] private byte maxPlayerPerRoom = 3;

    [SerializeField] private Button murderBtn = null;
    [SerializeField] private Button survivorBtn = null;

    //Room에 들어올 때 user가 Survivor인지 Murder인지 확인
    [SerializeField] private string userPosition = null;

    [SerializeField] private string nickName = string.Empty;

    [SerializeField] private GameObject survText = null;
    [SerializeField] private GameObject murText = null;

    [SerializeField] private GameObject noticeText = null;

    [Header("Shop")]
    [SerializeField] private GameObject shopPanel = null;
    [SerializeField] private Text moneyText = null;
    [SerializeField] private Text totalPriceText = null;
    [SerializeField] private Text healText = null;
    [SerializeField] private Text lightText = null;
    [SerializeField] private Text axeText = null;
    [SerializeField] private Text haveHealText = null;
    [SerializeField] private Text haveLightText = null;
    [SerializeField] private Text haveAxeText = null;
    float totalPrice = 0f;
    float healCount = 0f;
    float lightCount = 0f;
    float axeCount = 0f;

    private DataStorage dataStorage = null;

    private LogoutManager lm = null;

    [SerializeField] private GameObject noticeWindow = null;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        dataStorage = DataStorage.Instance;
        lm = LogoutManager.Instance;
    }

    void Start()
    {
        noticeText.SetActive(false);

        murderBtn.interactable = true;
        survivorBtn.interactable = true;

        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public void MurderBtn()
    {
        AudioManager.audioInstance._audioSource.Play();

        if (string.IsNullOrEmpty(LogoutManager.Instance.loginID)) return;

        userPosition = "m";
        dataStorage.userPosition = userPosition;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom });
        }
        else
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void SurvivorBtn()
    {
        AudioManager.audioInstance._audioSource.Play();

        if (string.IsNullOrEmpty(LogoutManager.Instance.loginID)) return;

        userPosition = "s";
        dataStorage.userPosition = userPosition;


        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        murderBtn.interactable = false;
        survivorBtn.interactable = false;

        if (userPosition == "m")
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom });
        }
        else if (userPosition == "s")
        {
            PhotonNetwork.JoinRandomRoom();
        }        
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
    }

    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene("Lobby");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        murderBtn.interactable = true;
        survivorBtn.interactable = true;

        //플레이 가능한 살인마 없음 텍스트 띄우기
        //StartCoroutine(EmptyRoomNotice());

        OpenNoticeWindow("no room to go in");

    }

    IEnumerator EmptyRoomNotice()
    {
        noticeText.SetActive(true);
        yield return new WaitForSeconds(2f);
        noticeText.SetActive(false);
    }

    public void SBonMouse()
    {
        survText.SetActive(true);
    }

    public void SBexitMouse()
    {
        survText.SetActive(false);
    }

    public void MBonMouse()
    {
        murText.SetActive(true);
    }

    public void MBexitMouse()
    {
        murText.SetActive(false);
    }

    //public void SBUp()
    //{
    //    survivorBtn.GetComponent<Button>().
    //}

    public void ShopOnOff()
    {
        if (shopPanel.activeSelf)
        {
            shopPanel.SetActive(false);
        }
        else
        {
            shopPanel.SetActive(true);
            moneyText.text = lm.money.ToString();
            haveHealText.text = lm.healItem.ToString();
            haveLightText.text = lm.lightItem.ToString();
            haveAxeText.text = lm.axeItem.ToString();
        }
    }

    public void HealItemUp()
    {
        if (totalPrice <= lm.money - 500f)
        {
            healCount++;
            totalPrice = totalPrice + 500f;
            totalPriceText.text = totalPrice.ToString();
            healText.text = healCount.ToString();
        }
    }

    public void HealItemDown()
    {
        if (healCount > 0)
        {
            healCount--;
            totalPrice = totalPrice - 500f;
            totalPriceText.text = totalPrice.ToString();
            healText.text = healCount.ToString();
        }
    }

    public void LightItemUp()
    {
        if (totalPrice <= lm.money - 500f)
        {
            lightCount++;
            totalPrice = totalPrice + 500f;
            totalPriceText.text = totalPrice.ToString();
            lightText.text = lightCount.ToString();
        }
    }

    public void LightItemDown()
    {
        if (lightCount > 0)
        {
            lightCount--;
            totalPrice = totalPrice - 500f;
            totalPriceText.text = totalPrice.ToString();
            lightText.text = lightCount.ToString();
        }
    }

    public void AxeItemUp()
    {
        if (totalPrice < lm.money - 500f)
        {
            axeCount++;
            totalPrice = totalPrice + 500f;
            totalPriceText.text = totalPrice.ToString();
            axeText.text = axeCount.ToString();
        }
    }

    public void AxeItemDown()
    {
        if (axeCount > 0)
        {
            axeCount--;
            totalPrice = totalPrice - 500f;
            totalPriceText.text = totalPrice.ToString();
            axeText.text = axeCount.ToString();
        }
    }

    public void BuyItem()
    {
        lm.money -= totalPrice;
        lm.healItem += healCount;
        lm.lightItem += lightCount;
        lm.axeItem += axeCount;

        lm.BuyItemFuc();

        ResetShop();

        OpenNoticeWindow("purchase complete");
    }

    private void ResetShop()
    {
        totalPrice = 0f;
        healCount = 0f;
        lightCount = 0f;
        axeCount = 0f;
        totalPriceText.text = "0";
        healText.text = "0";
        lightText.text = "0";
        axeText.text = "0";
        moneyText.text = lm.money.ToString();
        haveHealText.text = lm.healItem.ToString();
        haveLightText.text = lm.lightItem.ToString();
        haveAxeText.text = lm.axeItem.ToString();
    }

    private void OpenNoticeWindow(string _text)
    {
        noticeWindow.GetComponentInChildren<Text>().text = _text;
        noticeWindow.SetActive(true);
    }

    public void CloseNoticeWindow()
    {
        noticeWindow.SetActive(false);
    }
}
