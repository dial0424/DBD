using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UiManager : MonoBehaviourPun
{
    [SerializeField] private Image[] survivorStateUi = new Image[2];
    [SerializeField] private Text[] survivorNickName = new Text[2];

    //stateuiCall

    public void SurvivorNickName(int _n, string _name)
    {
        photonView.RPC("ChangeNameUi", RpcTarget.All, _n, _name);
    }

    [PunRPC]
    public void ChangeNameUi(int _n, string _name)
    {
        survivorNickName[_n - 1].text = _name;

        if (PhotonNetwork.NickName == _name) survivorNickName[_n - 1].color = Color.yellow;
    }

    public void currentSurvivorStateRPC(string _state, int _num)
    {
        photonView.RPC("changeUiState", RpcTarget.All, _state, _num);
    }

    public void DecreaseLifeGaugeStateRPC(float _lifeTime, int _num)
    {
        photonView.RPC("DecreaseLifeGauge", RpcTarget.All, _lifeTime, _num);
    }

    [PunRPC]
    public void changeUiState(string _state, int _clientNum)
    {
        if (_state == "WALK") survivorStateUi[_clientNum - 1].sprite = Resources.Load("Ui/UiTexture/AlivePlayerUIImg", typeof(Sprite)) as Sprite;
        else if (_state == "LIMP") survivorStateUi[_clientNum - 1].sprite = Resources.Load("Ui/UiTexture/HurtPlayerUIImg", typeof(Sprite)) as Sprite;
        else if (_state == "CRAWL") survivorStateUi[_clientNum - 1].sprite = Resources.Load("Ui/UiTexture/PlayerFlatUIImg", typeof(Sprite)) as Sprite;
        else if (_state == "HANG") survivorStateUi[_clientNum - 1].sprite = Resources.Load("Ui/UiTexture/WellMakeItImg", typeof(Sprite)) as Sprite;
        else if (_state == "DEAD") survivorStateUi[_clientNum - 1].sprite = Resources.Load("Ui/UiTexture/PlayerDieUIImg", typeof(Sprite)) as Sprite;
    }

    [PunRPC]
    public void DecreaseLifeGauge(float _lifeTime, int _clientNum)
    {
        survivorStateUi[_clientNum - 1].transform.GetChild(1).GetChild(0).
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal , _lifeTime / 30f * 100f);
    }
}
